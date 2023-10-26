using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class CommandStringBuilder
{
    protected Dictionary<string, string> commandFields = new()
    {
        { "velec", "11" },
        { "special_anodes", "1" },
        { "name", "test" },
        { "elec", "1" },
        { "selected", "1" },
        { "sync", "0" },
        { "pads", ""},
        { "amp", ""},
        { "width", ""}
    };

    private CommandStringBuilder SetPart(string key, string value)
    {
        commandFields[key] = value;
        return this;
    }

    public CommandStringBuilder Pads(IEnumerable<int> pads)
    {
        commandFields["pads"] = pads
            .Aggregate("", (current, pad) => current + $"{GetPadRemap(pad)}=C,");
        return this;
    }

    public CommandStringBuilder Amp(Dictionary<int, int> amplitudes)
    {
        commandFields["amp"] = amplitudes
            .Aggregate("", (current, kvp) => current + $"{GetPadRemap(kvp.Key)}={kvp.Value},");
        return this;
    }

    public CommandStringBuilder Width(Dictionary<int, int> frequencies)
    {
        commandFields["width"] = frequencies
            .Aggregate("", (current, kvp) => current + $"{GetPadRemap(kvp.Key)}={kvp.Value},");
        return this;
    }

    private int GetPadRemap(int padIndex)
    {
        // TODO: Implement this...
        return padIndex;
    }

    public abstract string GetFinalString();

    public override string ToString()
    {
        return GetFinalString();
    }
}