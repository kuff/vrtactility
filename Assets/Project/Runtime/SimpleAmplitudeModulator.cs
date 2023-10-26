using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(GlovePadProvider))]
public class SimpleAmplitudeModulator : MonoBehaviour, ICommandStringProvider
{
    private List<PadScript.Pad> _pads;

    private void Start()
    {
        _pads = GetComponent<IPadProvider>()!.GetPadRemap();
    }

    public string GenerateCommandString(IReadOnlyList<float> pressureValues)
    {
        var completeString = "";
        
        const string invariablePart1 = "velec 11 *special_anodes 1 *name test *elec 1 *pads ";
        const string invariablePart2 = " *amp ";
        const string invariablePart3 = " *width ";
        const string finalPart = " *selected 1 *sync 0\r";

        var variablePart1 = "";
        var variablePart2 = "";
        var variablePart3 = "";

        for (var i = 0; i < 32; i++)
        {
            // Use remap value to determine which finger pressure value we use
            var pressureValue = i switch
            {
                < 8  => pressureValues[0],
                < 21 => pressureValues[1],
                < 26 => pressureValues[2],
                < 31 => pressureValues[3],
                _    => pressureValues[4]  // == 31
            };

            // Ignore pads to implicitly assign them as Anodes
            if (i is 1 or 2 or 9 or 10 or 11 or 12 or 23 or 28) 
                continue;

            var amplitudeValue = GetConstrainedValue(_pads[i].GetAmplitude() * pressureValue);

            variablePart1 += _pads[i].GetRemap() + "=C,";
            variablePart2 += _pads[i].GetRemap() + "=" + amplitudeValue + ",";
            variablePart3 += _pads[i].GetRemap() + "=" + _pads[i].GetPulseWidth() + ",";
        }

        // Build final string and return
        completeString = invariablePart1 
                         + variablePart1 
                         + invariablePart2 
                         + variablePart2 
                         + invariablePart3 
                         + variablePart3 
                         + finalPart;
        return completeString;
    }
    
    private static float GetConstrainedValue(float value)
    {
        // Constrain the stimulator input such that overloading is avoided
        var rounded = Mathf.Round(value * 10) / 10;  // Fix the input value to 1 decimal increments
        var clamped = Mathf.Clamp(rounded, 0.5f, 9.0f);  // Fix input to compatible range
        
        return clamped;
    }
}
