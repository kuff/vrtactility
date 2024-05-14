using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Tactility.Calibration;
using Tactility.Modulation;
using UnityEngine;

public class FrequencyPadModulator : AbstractModulator
{
    public override ModulationData? GetModulationData()
    {
        var remap_strip = new[] { 31, 32, 29, 16, 15, 14, 11, 12, 13, 10, 9, 8, 5, 6, 7, 4, 3, 2, 30, 27, 28, 23, 26, 25, 24, 21, 22, 17, 20, 19, 1, 18 };
        var listPads = new List<int> { 5, 11, 2, 8 }; //we need only two pads for finger active! 

        var activePads = new float[32];
        for (var i = 0; i < 32; i++)
        {
            activePads[remap_strip[i] - 1] = listPads.Contains(i + 1)
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
