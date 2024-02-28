using Tactility.Ball;
using UnityEngine;

namespace Tactility.Modulation
{
    public class UniformGrabbableTactilityDataProvider : MonoBehaviour, ITactilityDataProvider
    {
        [SerializeField]
        [Tooltip("The UniformGrabbable component to provide tactility data from.")]
        private UniformGrabbable grabbable;
        private TactilityData _tactilityData;

        public ref TactilityData GetTactilityData()
        {
            // Initialize new TactilityData
            _tactilityData = new TactilityData
            {
                BoneIds = new System.Collections.Generic.List<OVRSkeleton.BoneId>(),
                Values = new System.Collections.Generic.List<float>()
            };
            
            if (grabbable.isGrabbed)
            {
                var bonePressures = grabbable.touchingBonePressures;
                var boneIds = grabbable.touchingBoneIds;
                _tactilityData.Values = bonePressures;
                _tactilityData.BoneIds = boneIds;
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
            return grabbable.isGrabbed;
        }
    }
}
