using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class OVRInitializer : MonoBehaviour
{
    public OVRInitializeEvent onInitialized = new();
    
    public Transform headTransform;
    public OVRSkeleton leftHandSkeleton, rightHandSkeleton;
    
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