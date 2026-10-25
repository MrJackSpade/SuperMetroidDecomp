namespace SuperMetroid.Core.Hardware;

/// <summary>
/// One named PPU layout location or bounded range (#630): a VRAM tilemap/character region, a CGRAM
/// palette range or an OAM range. Layout catalogs declare every location the port knows
/// as a static <see cref="PpuLayoutCell"/> with its native identity and meaning in XML documentation;
/// rendering views bind from these descriptors, and the inventory and alias-coherence checks
/// enumerate them.
/// </summary>
/// <param name="Name">The semantic identity, unique within its declaring catalog.</param>
/// <param name="Region">The store and offset unit.</param>
/// <param name="Offset">The first word, colour or object.</param>
/// <param name="Count">The number of units (one for a single colour).</param>
/// <param name="Encoding">How the units encode the domain value, e.g. "BG tilemap words" or "RgbColor palette".</param>
/// <param name="AliasOf">The cell this one intentionally shares storage with under a different meaning, if any.</param>
[PpuLayoutAccess]
public sealed record PpuLayoutCell(string Name, PpuLayoutRegion Region, int Offset, int Count, string Encoding,
    PpuLayoutCell? AliasOf = null)
{
    /// <summary>The first unit; checked against the region's capacity.</summary>
    public int Offset { get; } = Offset >= 0 && Offset + Count <= Capacity(Region)
        ? Offset
        : throw new ArgumentOutOfRangeException(nameof(Offset), Offset, $"{Name} does not fit in {Region}.");

    /// <summary>The number of units; at least one.</summary>
    public int Count { get; } = Count > 0
        ? Count
        : throw new ArgumentOutOfRangeException(nameof(Count), Count, "A PPU layout cell holds at least one unit.");

    /// <summary>The number of addressable units in <paramref name="region"/>.</summary>
    public static int Capacity(PpuLayoutRegion region) => region switch
    {
        PpuLayoutRegion.Vram => SnesPpuLayout.VramWordCount,
        PpuLayoutRegion.Cgram => SnesPpuLayout.CgramColorCount,
        PpuLayoutRegion.OamLow or PpuLayoutRegion.OamHigh => SnesPpuLayout.OamSpriteCount,
        _ => throw new ArgumentOutOfRangeException(nameof(region), region, "Unknown PPU layout region."),
    };

    /// <inheritdoc />
    public override string ToString() => $"{Name} ({Region} ${Offset:X4}+{Count}, {Encoding}{(AliasOf is null ? "" : $", alias of {AliasOf.Name}")})";
}
