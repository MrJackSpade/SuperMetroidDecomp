namespace SuperMetroid.Core.Enemies;

/// <summary>The random selections that scatter a multi-drop's positions.</summary>
internal static class EnemyDropScatterDefinitions
{
    /// <summary>The random selection that scatters each drop 0-31 pixels horizontally before the 16-pixel centring.</summary>
    internal static readonly RandomSampleField HorizontalScatter = new(Shift: 0, Width: 5);

    /// <summary>The random selection that scatters each drop 0-31 pixels vertically before the 16-pixel centring.</summary>
    internal static readonly RandomSampleField VerticalScatter = new(Shift: 8, Width: 5);

    /// <summary>
    /// The random selection that scatters a boss's death drops 0-127 pixels across a 128-pixel arena region from its
    /// left edge: the low seven bits.
    /// </summary>
    internal static readonly RandomSampleField NarrowArenaHorizontalScatter = new(Shift: 0, Width: 7);

    /// <summary>
    /// The random selection that scatters a boss's death drops 0-255 pixels across a 256-pixel arena strip from its left
    /// edge: the random sample modulo 256.
    /// </summary>
    internal static readonly RandomSampleField WideArenaHorizontalScatter = new(Shift: 0, Width: 8);

    /// <summary>
    /// The random selection that scatters a boss's death drops 0-63 pixels down a 64-pixel arena band from its top edge:
    /// the random sample divided by 256, modulo 64.
    /// </summary>
    internal static readonly RandomSampleField ArenaBandVerticalScatter = new(Shift: 8, Width: 6);
}
