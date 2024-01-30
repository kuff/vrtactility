using System;
using UnityEngine;
using UnityEngine.Events;

namespace Internal
{
    [Serializable]
    // ReSharper disable once InconsistentNaming
    public class OVRInitializeEvent : UnityEvent<Transform, OVRSkeleton, OVRSkeleton> { }
}