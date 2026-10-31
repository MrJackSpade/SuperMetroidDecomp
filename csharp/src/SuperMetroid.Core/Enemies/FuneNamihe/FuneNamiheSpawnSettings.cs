namespace SuperMetroid.Core.Enemies.FuneNamihe;

/// <summary>How a Fune or Namihe faces and spits (#1472).</summary>
/// <param name="Variant">Its variant, 0 through 15.</param>
/// <param name="Facing">The way it faces.</param>
/// <param name="CooldownTime">Updates, 0 through 255, between spits.</param>
/// <param name="LaunchIndex">Its spit speed, 0 through 255.</param>
/// <param name="YProximity">The vertical distance, 0 through 255 pixels, Samus must come within.</param>
internal sealed record FuneNamiheSpawnSettings(int Variant, HorizontalDirection Facing, int CooldownTime, int LaunchIndex, int YProximity) : EnemySpawnSettings
{
    /// <summary>Its variant, 0 through 15.</summary>
    public int Variant { get; init; } = Ensure.BetweenInclusive(0, 15, Variant);

    /// <summary>Updates, 0 through 255, between spits.</summary>
    public int CooldownTime { get; init; } = Ensure.BetweenInclusive(0, byte.MaxValue, CooldownTime);

    /// <summary>Its spit speed, 0 through 255.</summary>
    public int LaunchIndex { get; init; } = Ensure.BetweenInclusive(0, byte.MaxValue, LaunchIndex);

    /// <summary>The vertical distance, 0 through 255 pixels, Samus must come within.</summary>
    public int YProximity { get; init; } = Ensure.BetweenInclusive(0, byte.MaxValue, YProximity);
}
