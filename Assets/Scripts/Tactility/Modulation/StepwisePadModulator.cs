using Tactility.Calibration;
using UnityEngine;

namespace Tactility.Modulation
{
    [RequireComponent(typeof(ITactilityDataProvider))]
    public class StepwisePadModulator : AbstractModulator
    {
        private ITactilityDataProvider _dataProvider;

        protected override void Start()
        {
            _dataProvider = GetComponent<ITactilityDataProvider>();
        }
        
        public override ModulationData GetModulationData()
        {
            throw new System.NotImplementedException();
        }

        public override bool IsCompatibleWithDevice(TactilityDeviceConfig deviceConfig)
        {
            throw new System.NotImplementedException();
        }
    }
}
