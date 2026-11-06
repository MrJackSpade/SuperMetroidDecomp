namespace SuperMetroid.Core.Enemies.Powamp;

/// <summary>Whether a Powamp placement is its balloon, and how far the grapple pulls it (#1472, #1519).</summary>
/// <param name="Part">Which part of the Powamp it is.</param>
/// <param name="GrappleTravelDistance">The distance in pixels the grapple pulls the balloon; a nonzero distance also refuses contact.</param>
internal sealed record PowampSpawnSettings(PowampPart Part, int GrappleTravelDistance) : EnemySpawnSettings;
