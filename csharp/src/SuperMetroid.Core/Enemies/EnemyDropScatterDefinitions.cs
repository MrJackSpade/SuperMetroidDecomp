namespace SuperMetroid.Core.Enemies;

/// <summary>The random selections that scatter a multi-drop's positions.</summary>
internal static class EnemyDropScatterDefinitions
{
    /// <summary>The random selection that scatters each drop horizontally before its centring (<see cref="ScatterCentring"/>).</summary>
    internal static readonly RandomSampleField HorizontalScatter = new(Shift: 0, Width: 5);

    /// <summary>The random selection that scatters each drop vertically before its centring (<see cref="ScatterCentring"/>).</summary>
    internal static readonly RandomSampleField VerticalScatter = new(Shift: 8, Width: 5);

    /// <summary>
    /// The random selection that scatters a boss's death drops 0-127 pixels across a 128-pixel arena region from its
    /// left edge.
    /// </summary>
    internal static readonly RandomSampleField NarrowArenaHorizontalScatter = new(Shift: 0, Width: 7);

    /// <summary>
    /// The random selection that scatters a boss's death drops 0-255 pixels across a 256-pixel arena strip from its left
    /// edge.
    /// </summary>
    internal static readonly RandomSampleField WideArenaHorizontalScatter = new(Shift: 0, Width: 8);

    /// <summary>
    /// The random selection that scatters a boss's death drops 0-63 pixels down a 64-pixel arena band from its top edge.
    /// </summary>
    internal static readonly RandomSampleField ArenaBandVerticalScatter = new(Shift: 8, Width: 6);

    /// <summary>How far each scattered drop is drawn back on each axis, in pixels, centring its scatter on the origin.</summary>
    internal const int ScatterCentring = 16;
}
