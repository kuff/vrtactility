using Internal.Calibration;
using UnityEngine;

namespace Internal.Modulation
{
    [RequireComponent(typeof(TactilityManager))]
    [RequireComponent(typeof(ICalibrationDataProvider))]
    public abstract class AbstractModulator : MonoBehaviour
    {
        private TactilityManager _tactilityManager;
        protected ICalibrationDataProvider CalibrationDataProvider;
        
        protected virtual void Start()
        {
            _tactilityManager = GetComponent<TactilityManager>();
            CalibrationDataProvider = GetComponent<ICalibrationDataProvider>();
        }
        
        protected virtual void Update()
        {
            _tactilityManager.UpdateModulation(GetModulationData());
        }

        protected abstract ModulationData GetModulationData();
    }
}
