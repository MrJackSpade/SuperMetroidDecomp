namespace SuperMetroid.AssetExtraction.Enemies.Corpses;

/// <summary>
/// The cadence of the dead Torizo's falling sand: every fifteenth frame of its rot, the next of its sand lines falls, from
/// the fifteenth down to the first.
/// </summary>
internal sealed class DeadTorizoSandCadence
{
    /// <summary>The rot frames since the last line fell.</summary>
    private int frames;

    /// <summary>The next line to fall; none once it reaches zero.</summary>
    private int line = 15;

    /// <summary>Advances one rot frame, returning the line that falls on it, if one does.</summary>
    internal int? Advance()
    {
        frames++;
        if (frames < 15)
            return null;
        frames = 0;
        if (line == 0)
            return null;
        return line--;
    }
}
