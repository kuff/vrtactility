using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Internal
{
    public class OVRInitializer : MonoBehaviour
    {
        public OVRInitializeEvent onInitialized = new();
    
        [Tooltip("Reference to the Transform component of the player's head. This is used to track and utilize the position and orientation of the player's head in VR.")]
        public Transform headTransform;
        [Tooltip("Reference to the OVRSkeleton component for the left hand. This component is used for tracking and managing the skeletal representation of the player's left hand in VR.")]
        public OVRSkeleton leftHandSkeleton;
        [Tooltip("Reference to the OVRSkeleton component for the right hand. This component is used for tracking and managing the skeletal representation of the player's right hand in VR.")]
        public OVRSkeleton rightHandSkeleton;

        [HideInInspector] public List<OVRBone> leftHandBones, rightHandBones;
        [HideInInspector] public List<OVRBoneCapsule> leftHandBoneCapsules, rightHandBoneCapsules;

        [HideInInspector] public bool isInitialized;

        protected IEnumerator Start()
        {
            // Busy-wait until both hands are initialized
            while (!leftHandSkeleton.IsInitialized && !rightHandSkeleton.IsInitialized)
                yield return null;

            isInitialized = true;

            leftHandBones = leftHandSkeleton.Bones.ToList();
            rightHandBones = rightHandSkeleton.Bones.ToList();
            leftHandBoneCapsules = leftHandSkeleton.Capsules.ToList();
            rightHandBoneCapsules = rightHandSkeleton.Capsules.ToList();
        
            // Invoke initialization event
            onInitialized.Invoke(headTransform, leftHandSkeleton, rightHandSkeleton);
            Debug.Log("OVR initialization complete");
        }
    }
}