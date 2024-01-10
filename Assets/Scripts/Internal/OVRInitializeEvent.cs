using System;
using UnityEngine;
using UnityEngine.Events;

namespace Internal
{
    [Serializable]
    public class OVRInitializeEvent : UnityEvent<Transform, OVRSkeleton, OVRSkeleton> { }
}