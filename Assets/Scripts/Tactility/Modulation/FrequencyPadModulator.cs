using System.Collections;
using System.Collections.Generic;
using Tactility.Calibration;
using Tactility.Task;

namespace Tactility.Modulation
{
    public class FrequencyPadModulator : AbstractModulator
    {
        private ITactilityDataProvider _dataProvider;
        private GrabAndMoveScenario _grabScenario;

        protected override IEnumerator Start()
        {
            yield return base.Start();
            _dataProvider = GetComponent<ITactilityDataProvider>();
            _grabScenario = FindObjectOfType<GrabAndMoveScenario>();
        }

        public override ModulationData? GetModulationData()
        {
            if (!_dataProvider.IsActive())
            {
                return null;
            }
            
            var remapStrip = new[] { 31, 32, 29, 16, 15, 14, 11, 12, 13, 10, 9, 8, 5, 6, 7, 4, 3, 2, 30, 27, 28, 23, 26, 25, 24, 21, 22, 17, 20, 19, 1, 18 };
            var listPads = new List<int> { 4, 10, 1, 7 }; //we need only two pads for finger active! 

            if (_grabScenario!=null)
            {
                var grabScenario = _grabScenario.currentForceLevel;
                if (grabScenario==0)
                {
                    listPads = new List<int> { 28 };
                }
            }
            
            var activePads = new float[32];
            for (var i = 0; i < 32; i++)
            {
                activePads[remapStrip[i] - 1] = listPads.Contains(i)
                    ? 1f
                    : 0f;
            }

            return new ModulationData
            {
                Type = ModulationType.Pad,
                Values = activePads
            };
        }
        public override bool IsCompatibleWithDevice(TactilityDeviceConfig deviceConfig)
        {
            // This should always be true, as the base amps should always be compatible with the device
            // Otherwise, this is a problem elsewhere.
            return true;
        }
    }
}
