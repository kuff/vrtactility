using Tactility.Calibration;
using UnityEngine;

namespace Tactility.Modulation
{
    [RequireComponent(typeof(ITactilityDataProvider))]
    public class StepwisePadModulator : AbstractModulator
    {
        private ITactilityDataProvider _dataProvider;

        protected override void OnEnable()
        {
            _dataProvider = GetComponent<ITactilityDataProvider>();
            
            // If no ITactilityDataProvider is found, disable the modulator
            if (_dataProvider != null) return;
            Debug.LogWarning("No ITactilityDataProvider found. Disabling StepwiseWidthModulator.");
            enabled = false;
        }
        
        public override ModulationData GetModulationData()
        {
            ref var modulationData = ref _dataProvider.GetTactilityData();
            var remap = new[] { 30, 27, 29, 28, 25, 31, 32, 26, 17, 18, 20, 1, 2, 22, 19, 3, 23, 21, 24, 4, 5, 8, 9, 6, 7, 10, 13, 14, 11, 12, 15, 16 };
            
            // TODO: Implement spatial modulation logic
            throw new System.NotImplementedException();
        }

        public override bool IsCompatibleWithDevice(TactilityDeviceConfig deviceConfig)
        {
            // This modulator only supports the "glove" device at the moment
            //return deviceConfig.deviceName == "glove";
            return false;
        }
    }
}
