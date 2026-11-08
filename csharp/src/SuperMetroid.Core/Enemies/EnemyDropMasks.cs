namespace SuperMetroid.Core.Enemies;

/// <summary>The random selections that scatter a multi-drop's positions.</summary>
internal static class EnemyDropMasks
{
    /// <summary>The random selection that scatters each drop 0-31 pixels horizontally before the 16-pixel centring.</summary>
    internal static readonly RandomSampleField HorizontalScatter = new(Shift: 0, Width: 5);

    /// <summary>The random selection that scatters each drop 0-31 pixels vertically before the 16-pixel centring.</summary>
    internal static readonly RandomSampleField VerticalScatter = new(Shift: 8, Width: 5);
}
