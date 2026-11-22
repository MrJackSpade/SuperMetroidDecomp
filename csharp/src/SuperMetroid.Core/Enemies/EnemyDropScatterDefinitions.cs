namespace SuperMetroid.Core.Enemies;

/// <summary>The random selections that scatter a multi-drop's positions.</summary>
internal static partial class EnemyDropScatterDefinitions
{
    /// <summary>The random selection that scatters each drop horizontally before its centring (<see cref="ScatterCentring"/>).</summary>
    internal static readonly RandomSampleField HorizontalScatter = RandomSampleField.OneOf(32);

    /// <summary>The random selection that scatters each drop vertically before its centring (<see cref="ScatterCentring"/>).</summary>
    internal static readonly RandomSampleField VerticalScatter = RandomSampleField.SecondOf(32);

    /// <summary>
    /// The random selection that scatters a boss's death drops 0-127 pixels across a 128-pixel arena region from its
    /// left edge.
    /// </summary>
    internal static readonly RandomSampleField NarrowArenaHorizontalScatter = RandomSampleField.OneOf(128);

    /// <summary>
    /// The random selection that scatters a boss's death drops 0-255 pixels across a 256-pixel arena strip from its left
    /// edge.
    /// </summary>
    internal static readonly RandomSampleField WideArenaHorizontalScatter = RandomSampleField.OneOf(256);

    /// <summary>
    /// The random selection that scatters a boss's death drops 0-63 pixels down a 64-pixel arena band from its top edge.
    /// </summary>
    internal static readonly RandomSampleField ArenaBandVerticalScatter = RandomSampleField.SecondOf(64);

    /// <summary>How far each scattered drop is drawn back on each axis, in pixels, centring its scatter on the origin.</summary>
    internal const int ScatterCentring = 16;
}
