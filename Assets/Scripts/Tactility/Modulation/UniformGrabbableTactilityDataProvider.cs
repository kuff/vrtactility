// Copyright (C) 2024 Peter Leth

#region
using System.Collections.Generic;
using Tactility.Ball;
using UnityEngine;
#endregion

namespace Tactility.Modulation
{
    public class UniformGrabbableTactilityDataProvider : MonoBehaviour, ITactilityDataProvider
    {
        [SerializeField]
        [Tooltip("The UniformGrabbable component to provide tactility data from.")]
        private UniformGrabbable grabbable;
        private TactilityData _tactilityData = new TactilityData
        {
            BoneIds = new List<OVRSkeleton.BoneId>(),
            Values = new List<float>()
        };

        public ref TactilityData GetTactilityData()
        {
            if (grabbable.isGrabbed)
            {
                _tactilityData.Values = grabbable.touchingBonePressures;
                _tactilityData.BoneIds = grabbable.touchingBoneIds;
            }
            else
            {
                _tactilityData.Values.Clear();
                _tactilityData.BoneIds.Clear();
            }
            return ref _tactilityData;
        }

        public bool IsActive()
        {
            return grabbable.isGrabbed && grabbable.touchingBonePressures.Count > 0;
        }
    }
}
