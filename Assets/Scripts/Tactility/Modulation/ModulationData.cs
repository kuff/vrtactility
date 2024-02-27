namespace Tactility.Modulation
{
    public enum ModulationType
    {
        Amp,
        Width
    }
    
    public struct ModulationData
    {
        public ModulationType Type;
        public float[] Values;
    }
}