namespace Tactility.Modulation
{
    public class ConstantWidthModulator : AbstractModulator
    {
        public override ModulationData GetModulationData()
        {
            var values = new float[32];
            for (var i = 0; i < 32; i++)
                values[i] = /*PadData[i].GetPulseWidth();*/ 1f;
            
            var data = new ModulationData
            {
                Type = ModulationType.Width,
                Values = values
            };
            
            return data;
        }

        public override bool IsCompatibleWithDevice(string deviceName)
        {
            return false;
        }
    }
}