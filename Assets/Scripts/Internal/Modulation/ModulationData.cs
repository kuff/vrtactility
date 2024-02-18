// ReSharper disable UnassignedField.Global
namespace Internal.Modulation
{
    public enum ModulationType
    {
        Amp,
        Pad,
        Width
    }
    
    public struct ModulationData
    {
        public ModulationType Type;
        public float[] Values;
    }
}