namespace SuperMetroid.Core.Enemies.Corpses;

/// <summary>
/// How far one pixel row of a rotting corpse has fallen (#1472): the pixel row it moves next, the rot calls it still waits
/// before it starts falling, and whether it has fallen out of the corpse.
/// </summary>
internal sealed class CorpseRowFall
{
    /// <summary>Starts a row waiting <paramref name="delayCalls"/> rot calls at pixel row <paramref name="pixelRow"/>.</summary>
    internal CorpseRowFall(int pixelRow, int delayCalls)
    {
        PixelRow = pixelRow;
        DelayCalls = delayCalls;
    }

    /// <summary>The corpse pixel row, from the top, the row moves next; it falls two rows each call once its delay is over.</summary>
    public int PixelRow { get; internal set; }

    /// <summary>The rot calls the row still waits before it starts falling.</summary>
    public int DelayCalls { get; internal set; }

    /// <summary>
    /// Whether the row has fallen out of the corpse and is skipped from now on. The corpse's last row never sets it: its
    /// fall ends the whole rot instead.
    /// </summary>
    public bool HasFallen { get; internal set; }
}
