using Internal.Calibration;
using UnityEngine;
// ReSharper disable MemberCanBePrivate.Global

namespace Internal.Modulation
{
    [RequireComponent(typeof(TactilityManager))]
    [RequireComponent(typeof(ICalibrationDataProvider))]
    public abstract class AbstractModulator : MonoBehaviour
    {
        protected TactilityManager TactilityManager;
        protected ICalibrationDataProvider CalibrationDataProvider;
        
        protected virtual void Start()
        {
            TactilityManager = GetComponent<TactilityManager>();
            CalibrationDataProvider = GetComponent<ICalibrationDataProvider>();
        }
        
        protected virtual void Update()
        {
            TactilityManager.UpdateModulation(GetModulationData());
        }

        protected abstract ModulationData GetModulationData();
    }
}
