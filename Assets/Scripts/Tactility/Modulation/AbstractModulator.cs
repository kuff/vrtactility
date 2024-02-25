using Tactility.Calibration;
using UnityEngine;

namespace Tactility.Modulation
{
    [RequireComponent(typeof(TactilityManager))]
    public abstract class AbstractModulator : MonoBehaviour
    {
        private TactilityManager _tactilityManager;
        // ReSharper disable once NotAccessedField.Local
        private CalibrationManager _cm;

        protected virtual void Start()
        {
            _tactilityManager = GetComponent<TactilityManager>();
            _cm = CalibrationManager.Instance;
        }
        
        protected virtual void Update()
        {
            _tactilityManager.UpdateModulation(GetModulationData());
        }

        public abstract ModulationData GetModulationData();
        public abstract bool IsCompatibleWithDevice(string deviceName);
    }
}
