namespace SuperMetroid.Core.Enemies.Ripper;

/// <summary>A GRipper's patrol speed, starting direction and patrol bounds (#1471, #1472).</summary>
/// <param name="SpeedIndex">The common linear-speed record, 0 through 255, the GRipper patrols at.</param>
/// <param name="Direction">The direction it starts moving in.</param>
/// <param name="MinimumX">The room column, 0 through 65535, it turns back at moving left.</param>
/// <param name="MaximumX">The room column, 0 through 65535, it turns back at moving right.</param>
internal sealed record GRipperSpawnSettings(int SpeedIndex, GRipperStartDirection Direction, int MinimumX, int MaximumX) : EnemySpawnSettings
{
    /// <summary>The common linear-speed record, 0 through 255, the GRipper patrols at.</summary>
    public int SpeedIndex { get; init; } = Ensure.BetweenInclusive(0, byte.MaxValue, SpeedIndex);

    /// <summary>The room column, 0 through 65535, it turns back at moving left.</summary>
    public int MinimumX { get; init; } = Ensure.BetweenInclusive(0, ushort.MaxValue, MinimumX);

    /// <summary>The room column, 0 through 65535, it turns back at moving right.</summary>
    public int MaximumX { get; init; } = Ensure.BetweenInclusive(0, ushort.MaxValue, MaximumX);
}
