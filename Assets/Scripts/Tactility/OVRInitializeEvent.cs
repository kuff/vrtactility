using System;
using UnityEngine;
using UnityEngine.Events;

namespace Tactility
{
    [Serializable]
    // ReSharper disable once InconsistentNaming
    public class OVRInitializeEvent : UnityEvent<Transform, OVRSkeleton, OVRSkeleton> { }
}