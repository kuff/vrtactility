namespace Internal.Modulation
{
    public interface IDeviceStringEncoder
    {
        string EncodeCommandString(float[] pads, float[] amps, float[] widths);
    }
}