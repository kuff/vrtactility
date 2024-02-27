using System.Collections.Generic;
using UnityEngine;

namespace Tactility.Modulation
{
    public class DebugOscillatingTactilityDataProvider : MonoBehaviour, ITactilityDataProvider
    {
        // Public field to define the duration of one oscillation cycle in seconds
        [Tooltip("The duration of one oscillation cycle in seconds.")]
        public float oscillationPeriodSeconds = 3.0f;

        // This is a placeholder for the TactilityData that will be updated and returned by GetTactilityData()
        private TactilityData _tactilityData;

        private void Start()
        {
            // Initialize your TactilityData with all bone IDs and initial values
            _tactilityData = new TactilityData
            {
                BoneIds = new List<OVRSkeleton.BoneId>(),  // Populate with all bone IDs you are interested in
                Values = new List<float>()  // Initial values, these will be updated in GetTactilityData()
            };

            // Example: Adding all finger bone IDs
            foreach (OVRSkeleton.BoneId boneId in System.Enum.GetValues(typeof(OVRSkeleton.BoneId)))
            {
                _tactilityData.BoneIds.Add(boneId);
                _tactilityData.Values.Add(0);  // Initialize with 0, will be updated dynamically
            }
        }

        public ref TactilityData GetTactilityData()
        {
            // Calculate the oscillating value based on the current time and the oscillation period
            var elapsedTime = Time.time;
            var oscillation = (Mathf.Sin((elapsedTime / oscillationPeriodSeconds) * 2 * Mathf.PI) + 1) / 2;  // Oscillates between 0 and 1
            
            // Update all values with the current oscillating value
            for (var i = 0; i < _tactilityData.Values.Count; i++)
            {
                _tactilityData.Values[i] = oscillation;
            }

            // Return the reference to the updated TactilityData
            return ref _tactilityData;
        }
    }
}