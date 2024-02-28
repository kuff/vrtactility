namespace Tactility.Modulation
{
    public enum ModulationType
    {
        Pad,
        Amplitude,
        Width,
        Frequency
    }
    
    public struct ModulationData
    {
        public ModulationType Type;
        public float[] Values;
    }
}