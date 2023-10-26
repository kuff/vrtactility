using System.Collections.Generic;
using UnityEngine;

public class GlovePadProvider : MonoBehaviour, IPadProvider
{
    [SerializeField] private CalibrationScriptableObject calibrationData;
    
    public List<PadScript.Pad> GetPadRemap()
    {
        return calibrationData.values;
    }
}
