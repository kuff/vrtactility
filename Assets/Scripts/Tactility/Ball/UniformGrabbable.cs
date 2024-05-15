// Copyright (C) 2024 Peter Leth

#region
using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using UnityEngine;
#endregion

namespace Tactility.Ball
{
    [RequireComponent(typeof(Collider))]
    public class UniformGrabbable : MonoBehaviour
    {
        private const float MatchingThreshold = 0.001f;

        [Tooltip("Defines the minimum pressure required for a bone to be considered as grabbing the object. This threshold helps distinguish between a light touch and an actual grip.")]
        [Range(0.1f, 1.0f)]
        public float pressureThreshold;
        [Tooltip("Reference to the OVRInitializer component. This component is used for initializing and managing Oculus VR hand tracking data.")]
        [SerializeField]
        private OVRInitializer ovrInitializer;

        // Exposing touch
        [HideInInspector] public List<OVRSkeleton.BoneId> touchingBoneIds;
        [HideInInspector] public List<float> touchingBonePressures;

        // Exposing grab
        [HideInInspector] public bool isGrabbed;
        private List<OVRBoneCapsule> _boneCapsules;

        // Managing touch
        private List<OVRBone> _bones;

        private SphereCollider _collider;
        private List<OVRBoneCapsule> _touchingBoneCapsules;
        private Dictionary<OVRSkeleton.BoneId, Vector3> _touchingPoints;
        private Dictionary<OVRSkeleton.BoneId, Vector3> _touchingNormals;
        // private Renderer _renderer;

        private void Start()
        {
            _collider = GetComponent<SphereCollider>();
            // _renderer = GetComponent<Renderer>();

            // if (pressureThreshold > _collider.radius) pressureThreshold = _collider.radius;

            _touchingBoneCapsules = new List<OVRBoneCapsule>();
            _touchingPoints = new Dictionary<OVRSkeleton.BoneId, Vector3>();
            _touchingNormals = new Dictionary<OVRSkeleton.BoneId, Vector3>();
        }

        private void Update()
        {
            // Wait for OVR to initialize bones
            if (_boneCapsules is null && ovrInitializer.isInitialized)
            {
                // ...Save bone capsules
                _boneCapsules = ovrInitializer.LeftHandBoneCapsules.Concat(ovrInitializer.RightHandBoneCapsules).ToList();
                SetIsKinematic(false);
                
                // Go through each capsule and set the weight to zero
                foreach (var boneCapsule in _boneCapsules)
                {
                    boneCapsule.CapsuleRigidbody.SetDensity(0f);
                }
            }
            if (_bones is null && ovrInitializer.isInitialized)
            {
                // ...Save bones
                _bones = ovrInitializer.LeftHandBones.Concat(ovrInitializer.RightHandBones).ToList();
            }

            // Update applied pressure for each touching bone if any
            // TODO: Optimize this to only loop through finger tips
            for (var i = 0; i < _touchingBoneCapsules.Count; i++)
            {
                touchingBonePressures[i] = GetAppliedPressure(_touchingBoneCapsules[i]);
            }

            // Stop updating if the applied pressure is less than would be required to grib the object
            // TODO: New pressure calculations must be reflected here...
            /*if (touchingBonePressures.Count > 0 && touchingBonePressures.Max() < pressureThreshold)
            {
                isGrabbed = false;
                return;
            }*/

            try
            {
                // Below here is new
                // Get the touching point and normal from the index and thumb only
                var indexPoint = _touchingPoints[OVRSkeleton.BoneId.Hand_Index3];
                var thumbPoint = _touchingPoints[OVRSkeleton.BoneId.Hand_Thumb3];
                var indexNormal = _touchingNormals[OVRSkeleton.BoneId.Hand_Index3];
                var thumbNormal = _touchingNormals[OVRSkeleton.BoneId.Hand_Thumb3];
                
                // If we don't have either the index or thumb touching, we can't grab
                // Debug.Log($"1: {indexPoint == Vector3.zero}");
                // Debug.Log($"2: {thumbPoint == Vector3.zero}");
                if (indexPoint == Vector3.zero || thumbPoint == Vector3.zero)
                {
                    isGrabbed = false;
                    return;
                }
                
                // If the normals don't cancel each other out or are equal, we can't grab
                // Debug.Log($"3: {Vector3.Dot(indexNormal, thumbNormal) > 0.1f || indexNormal == thumbNormal}");
                // Debug.Log($"4: {Vector3.Dot(indexNormal, thumbNormal)}");
                if (Vector3.Dot(indexNormal, thumbNormal) > 0f || indexNormal == thumbNormal)
                {
                    isGrabbed = false;
                    return;
                }
                
                // If the distance between the two points is greater than the object's width plus a small buffer, we can't grab
                var objectWidth = transform.localScale.x;
                // Debug.Log($"5: {Vector3.Distance(indexPoint, thumbPoint)}");
                // Debug.Log($"6: {objectWidth + 0.01f}");
                var distance = Vector3.Distance(indexPoint, thumbPoint);
                if (distance > objectWidth + 0.01f)
                {
                    isGrabbed = false;
                    return;
                }

                isGrabbed = true;
            }
            catch (Exception e)
            {
                isGrabbed = false;
            }
            
            // Calculate union of all collision point vectors to indicate grip distribution
            // var gripVector = _touchingPoints.Values.Aggregate(Vector3.zero, (current, vec) => current + (vec - transform.position));
            // if (gripVector == Vector3.zero)
            // {
            //     isGrabbed = false;
            //     return;
            // }
            // gripVector /= _touchingPoints.Count;

            // Manage FreeFloatable in accordance with grip
            // if (gripVector.magnitude < 0.014)
            // {
            //     isGrabbed = true;
            // }
            // else if (gripVector.magnitude > 0.014)
            // {
            //     isGrabbed = false;
            // }
            // Debug.Log(gripVector.magnitude);
        }

        private void OnCollisionEnter(Collision collision)
        {
            // Add the closest matching OVRBone to list of touching bones
            var closestBoneCapsule = FindMatchingBone(in collision);
            if (closestBoneCapsule is null || IsTouching(in closestBoneCapsule))
            {
                return; // Don't add a bone that doesn't exist or is already touching
            }

            if (!IsBoneOnValidHand(in closestBoneCapsule))
            {
                // Clear state and return if the colliding hand is not the hand currently touching
                ClearTrackedBones();

                SetIsKinematic(in closestBoneCapsule, true);
                SetIsKinematic(in closestBoneCapsule, false);

                return;
            }

            // Add bone and calculate applied pressure
            var boneId = GetBoneId(in closestBoneCapsule);

            // switch (boneId)
            // {
            //     case OVRSkeleton.BoneId.Hand_Thumb3:
            //     case OVRSkeleton.BoneId.Hand_Index3:
            //     case OVRSkeleton.BoneId.Hand_Middle3:
            //     case OVRSkeleton.BoneId.Hand_Ring3:
            //     case OVRSkeleton.BoneId.Hand_Pinky3:
            //         break;
            //     default:
            //         return;
            // }

            try
            {
                _touchingPoints.Add(boneId, collision.contacts[0].point);
                _touchingNormals.Add(boneId, collision.contacts[0].normal);
            }
            catch (ArgumentException)
            {
                return;
            }
            _touchingBoneCapsules.Add(closestBoneCapsule);
            touchingBoneIds.Add(boneId);
            touchingBonePressures.Add(GetAppliedPressure(in closestBoneCapsule));
        }

        private void OnCollisionExit(Collision collision)
        {
            // Find the OVRBone that best matches the colliding object
            var closestBoneCapsule = FindMatchingBone(in collision);
            if (closestBoneCapsule is null)
            {
                return; // If the colliding object is not an OVRBone
            }

            var touchingIdx = _touchingBoneCapsules.IndexOf(closestBoneCapsule);
            if (touchingIdx == -1)
            {
                return; // If the colliding bone was not previously touching
            }

            if (!IsBoneOnValidHand(in closestBoneCapsule))
            {
                return; // If the colliding bone is from the wrong hand
            }

            if (_touchingBoneCapsules.Count != 1)
            {
                SwapRemove(in touchingIdx); // Remove the no-longer-colliding OVRBone

                // Make the bone kinematic briefly to reset position in relation to the rest of the hand
                SetIsKinematic(in closestBoneCapsule, true);
                SetIsKinematic(in closestBoneCapsule, false);
                return;
            }
            ClearTrackedBones();

            // If no bones are touching anymore we do the same for all bones in the hand, even those that haven't directly touched the object
            SetIsKinematic(true);
            SetIsKinematic(false);
        }

        private void OnCollisionStay(Collision collision)
        {
            // Find matching bone
            var closestBoneCapsule = FindMatchingBone(in collision);
            if (closestBoneCapsule is null)
            {
                return; // If the colliding object is not a bone
            }

            if (!IsBoneOnValidHand(in closestBoneCapsule) || _touchingBoneCapsules.Count == 0)
            {
                return; // Ignore collision if the colliding bone is from the wrong hand or state has been reset
            }

            // Update the contact points of each touching OVRBoneCapsule
            var boneId = GetBoneId(in closestBoneCapsule);
            _touchingPoints[boneId] = collision.contacts[0].point;
            _touchingNormals[boneId] = collision.contacts[0].normal;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void SetIsKinematic(in bool state)
        {
            // Loop through all Rigidbodies on all OVRBoneCapsules and update their state
            foreach (var boneCapsule in _boneCapsules)
            {
                SetIsKinematic(in boneCapsule, in state);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void SetIsKinematic(in OVRBoneCapsule boneCapsule, in bool state)
        {
            boneCapsule.CapsuleRigidbody.isKinematic = state;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private float GetAppliedPressure(in OVRBoneCapsule boneCapsule)
        {
            /*var r = _collider.transform.localScale.x;

            // Find corresponding OVRBone (which doesn't collide with the sphere surface) and its position
            var targetBone = _bones[GetBoneIndex(in boneCapsule)];
            var bonePosition = targetBone.Transform.position;

            // Calculate distance between bone and sphere center and project that into a pressure value between 0 and 1
            var distance = Mathf.Sqrt((bonePosition - transform.position).sqrMagnitude);
            var pressure = Mathf.Clamp(r - distance, 0, r) / r;

            // Return distance as pressure applied
            return pressure;*/

            try
            {
                var indexPoint = _touchingPoints[OVRSkeleton.BoneId.Hand_Index3];
                var thumbPoint = _touchingPoints[OVRSkeleton.BoneId.Hand_Thumb3];

                // The lesser the distance between the two points, the greater the pressure
                var distance = Vector3.Distance(indexPoint, thumbPoint);
                
                Debug.Log("Scale: " + transform.localScale.x);
                Debug.Log("distance: " + distance);
                // Debug.Log("Before clamp: " + distance / transform.localScale.x);

                // Project the distance into a pressure value between 0 and 1
                return 1 - Mathf.Clamp01(distance / transform.localScale.x);
            }
            catch (Exception e)
            {
                return 0f;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetBoneIndex(in OVRBoneCapsule boneCapsule)
        {
            // Find out what hand boneCapsule belongs to in order to index properly
            var isLeftHand = _boneCapsules.IndexOf(boneCapsule) < _boneCapsules.Count / 2;
            var indexOffset = isLeftHand
                ? 0
                : _bones.Count / 2;

            return indexOffset + boneCapsule.BoneIndex;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool IsIndexOnLeftHand(in int index)
        {
            return index < _bones.Count / 2;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ClearTrackedBones()
        {
            _touchingPoints.Clear();
            _touchingNormals.Clear();
            _touchingBoneCapsules.Clear();

            touchingBoneIds.Clear();
            touchingBonePressures.Clear();
            isGrabbed = false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool IsBoneOnValidHand(in OVRBoneCapsule boneCapsule)
        {
            if (_touchingBoneCapsules.Count == 0)
            {
                return true;
            }

            var collidingBoneIndex = GetBoneIndex(in boneCapsule);
            var firstTouchingBoneIndex = GetBoneIndex(_touchingBoneCapsules[0]);

            return IsIndexOnLeftHand(in collidingBoneIndex) == IsIndexOnLeftHand(in firstTouchingBoneIndex);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private OVRSkeleton.BoneId GetBoneId(in OVRBoneCapsule boneCapsule)
        {
            return _bones[boneCapsule.BoneIndex].Id;
        }

        [CanBeNull]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private OVRBoneCapsule FindMatchingBone(in Collision collision)
        {
            if (_boneCapsules is null)
            {
                // If an attempt is being made to process a collision before the OVR bones have properly initialized...
                return null;
            }
            
            // Find the OVRBone that best matches the colliding object
            OVRBoneCapsule closestBone = null;
            var smallestDistance = float.MaxValue;
            foreach (var bone in _boneCapsules)
            {
                var distance = Vector3.Distance(bone.CapsuleCollider.transform.position, collision.transform.position);
                if (distance > MatchingThreshold || distance > smallestDistance)
                {
                    continue; // NOTE: MATCHING_THRESHOLD check may be redundant
                }

                closestBone = bone;
                smallestDistance = distance;
            }

            return closestBone;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool IsTouching(in OVRBoneCapsule bone)
        {
            foreach (var b in _touchingBoneCapsules)
            {
                if (b.BoneIndex == bone.BoneIndex)
                {
                    return true;
                }
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsLeftHandTouching()
        {
            // Although GetBoneIndex returns _bones index and not _boneCapsules, we're still free to pipe it in to the left hand check
            return _touchingBoneCapsules.Count != 0 && IsIndexOnLeftHand(GetBoneIndex(_touchingBoneCapsules[0]));
        }

        [CanBeNull]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Rigidbody GetTouchingHandRoot()
        {
            if (_touchingBoneCapsules.Count == 0)
            {
                return null;
            }

            return IsLeftHandTouching()
                ? _boneCapsules[0].CapsuleRigidbody
                : _boneCapsules[19].CapsuleRigidbody;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void SwapRemove(in int index)
        {
            // A more efficient way of removing elements from lists when the order of elements doesn't matter

            // For touchingBoneIds, _touchingBoneCapsules, touchingBonePressures
            touchingBoneIds[index] = touchingBoneIds[^1];
            _touchingBoneCapsules[index] = _touchingBoneCapsules[^1];
            touchingBonePressures[index] = touchingBonePressures[^1];
            touchingBoneIds.RemoveAt(touchingBoneIds.Count - 1);
            _touchingBoneCapsules.RemoveAt(_touchingBoneCapsules.Count - 1);
            touchingBonePressures.RemoveAt(touchingBonePressures.Count - 1);

            // For _touchingPoints and _touchingNormals
            var pairAtIndex = _touchingPoints.ElementAt(index);
            var lastPair = _touchingPoints.ElementAt(_touchingPoints.Count - 1);
            var pairAtIndexNormal = _touchingNormals.ElementAt(index);
            var lastPairNormal = _touchingNormals.ElementAt(_touchingNormals.Count - 1);

            _touchingPoints.Remove(lastPair.Key);
            _touchingNormals.Remove(lastPairNormal.Key);

            if (!EqualityComparer<OVRSkeleton.BoneId>.Default.Equals(pairAtIndex.Key, lastPair.Key))
            {
                _touchingPoints[pairAtIndex.Key] = lastPair.Value;
                _touchingNormals[pairAtIndexNormal.Key] = lastPairNormal.Value;
            }
        }
    }
}
