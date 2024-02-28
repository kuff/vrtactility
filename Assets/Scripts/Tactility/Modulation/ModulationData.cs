namespace Tactility.Modulation
{
    public enum ModulationType
    {
        Pad,
        Amp,
        Width
    }
    
    public struct ModulationData
    {
        public ModulationType Type;
        public float[] Values;
    }
}