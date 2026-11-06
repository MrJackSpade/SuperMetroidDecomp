namespace SuperMetroid.Core.Enemies.FuneNamihe;

/// <summary>How a Fune or Namihe faces and spits (#1472, #1519).</summary>
/// <param name="Variant">Whether it behaves as a Fune or a Namihe; a placement names this apart from its species.</param>
/// <param name="Facing">The way it faces.</param>
/// <param name="CooldownTime">The updates a Fune waits between spits.</param>
/// <param name="LaunchSpeed">The speed, in pixels per update, at which its fireballs fly away from it.</param>
/// <param name="YProximity">The vertical distance in pixels Samus must come within to wake a Namihe.</param>
internal sealed record FuneNamiheSpawnSettings(FuneNamiheVariant Variant, HorizontalDirection Facing, int CooldownTime, SignedEightEight LaunchSpeed, int YProximity) : EnemySpawnSettings;
