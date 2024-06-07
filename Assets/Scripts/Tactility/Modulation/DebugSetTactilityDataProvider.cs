// Copyright (C) 2024 Peter Leth

#region
using System;
using System.Collections.Generic;
using UnityEngine;
#endregion

namespace Tactility.Modulation
{
    public class DebugSetTactilityDataProvider : MonoBehaviour, ITactilityDataProvider
    {
        private TactilityData _tactilityData;
        private float _continuousForceLevel;

        private void Start()
        {
            _tactilityData = new TactilityData
            {
                BoneIds = new List<OVRSkeleton.BoneId>(),
                Values = new List<float>()
            };

            foreach (OVRSkeleton.BoneId boneId in Enum.GetValues(typeof(OVRSkeleton.BoneId)))
            {
                _tactilityData.BoneIds.Add(boneId);
                _tactilityData.Values.Add(0); // Initialize with 0, will be updated dynamically
            }
        }
        
        public void SetForceLevel(int forceLevel)
        {
            _continuousForceLevel = forceLevel switch
            {
                6 => 1.0f,
                5 => 0.85f,
                4 => 0.6f,
                3 => 0.45f,
                2 => 0.3f,
                1 => 0.15f,
                _ => throw new ArgumentOutOfRangeException(nameof(forceLevel), forceLevel, null)
            };
        }

        public ref TactilityData GetTactilityData()
        {
            for (var i = 0; i < _tactilityData.Values.Count; i++)
            {
                _tactilityData.Values[i] = _continuousForceLevel;
            }

            return ref _tactilityData;
        }

        public bool IsActive()
        {
            return true; // Always active for debugging
        }
    }
}
