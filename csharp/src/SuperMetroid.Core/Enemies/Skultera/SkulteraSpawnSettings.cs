namespace SuperMetroid.Core.Enemies.Skultera;

/// <summary>How a Skultera swims (#1472).</summary>
/// <param name="SpeedIndex">The common linear-speed record, 0 through 255, it swims at.</param>
/// <param name="StartsLeft">Whether it starts swimming left.</param>
/// <param name="Radius">Its wave radius, 0 through 255.</param>
/// <param name="AngleDelta">Its wave step, 0 through 255.</param>
internal sealed record SkulteraSpawnSettings(int SpeedIndex, bool StartsLeft, int Radius, int AngleDelta) : EnemySpawnSettings
{
    /// <summary>The common linear-speed record, 0 through 255, it swims at.</summary>
    public int SpeedIndex { get; init; } = Ensure.BetweenInclusive(0, byte.MaxValue, SpeedIndex);

    /// <summary>Its wave radius, 0 through 255.</summary>
    public int Radius { get; init; } = Ensure.BetweenInclusive(0, byte.MaxValue, Radius);

    /// <summary>Its wave step, 0 through 255.</summary>
    public int AngleDelta { get; init; } = Ensure.BetweenInclusive(0, byte.MaxValue, AngleDelta);
}
