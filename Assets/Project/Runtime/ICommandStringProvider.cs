using System.Collections.Generic;

public interface ICommandStringProvider
{
    public string GenerateCommandString(IReadOnlyList<float> pressureValues);
}
