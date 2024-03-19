// Copyright (C) 2024 Peter Leth

#region
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
#endregion

namespace Tactility
{
    [Serializable]
    // ReSharper disable once InconsistentNaming
    public class OVRInitializeEvent : UnityEvent<Transform, OVRSkeleton, OVRSkeleton>
    {
    }

    // ReSharper disable once InconsistentNaming
    public class OVRInitializer : MonoBehaviour
    {
        public OVRInitializeEvent onInitialized = new OVRInitializeEvent();

        [Tooltip("Reference to the Transform component of the player's head. This is used to track and utilize the position and orientation of the player's head in VR.")]
        public Transform headTransform;
        [Tooltip("Reference to the OVRSkeleton component for the left hand. This component is used for tracking and managing the skeletal representation of the player's left hand in VR.")]
        public OVRSkeleton leftHandSkeleton;
        [Tooltip("Reference to the OVRSkeleton component for the right hand. This component is used for tracking and managing the skeletal representation of the player's right hand in VR.")]
        public OVRSkeleton rightHandSkeleton;

        [HideInInspector] public bool isInitialized;
        public List<OVRBoneCapsule> LeftHandBoneCapsules, RightHandBoneCapsules;

        public List<OVRBone> LeftHandBones, RightHandBones;

        protected IEnumerator Start()
        {
            // Busy-wait until both hands are initialized
            while (!leftHandSkeleton.IsInitialized && !rightHandSkeleton.IsInitialized)
            {
                yield return null;
            }

            isInitialized = true;

            LeftHandBones = leftHandSkeleton.Bones.ToList();
            RightHandBones = rightHandSkeleton.Bones.ToList();
            LeftHandBoneCapsules = leftHandSkeleton.Capsules.ToList();
            RightHandBoneCapsules = rightHandSkeleton.Capsules.ToList();

            // Invoke initialization event
            onInitialized.Invoke(headTransform, leftHandSkeleton, rightHandSkeleton);
            Debug.Log("OVR initialization complete");
        }
    }
}
