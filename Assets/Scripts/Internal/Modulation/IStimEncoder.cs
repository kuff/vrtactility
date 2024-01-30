namespace Internal.Modulation
{
    public interface IStimEncoder
    {
        string GetStimString();
        int GetIndexRemap(int index);
    }
}
