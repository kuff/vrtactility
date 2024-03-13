using System.Collections.Generic;
using UnityEngine;

namespace Tactility.Calibration.Old
{
    //[CreateAssetMenu(fileName = "CalibrationData", menuName = "Tactility/Calibration Data", order = 1)]
    public class CalibrationData : ScriptableObject
    {
        public List<PadScript.Pad> values = new List<PadScript.Pad>();
        public string port;
    }
}
