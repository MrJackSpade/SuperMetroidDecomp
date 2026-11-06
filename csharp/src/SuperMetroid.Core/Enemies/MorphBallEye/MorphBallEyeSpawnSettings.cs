namespace SuperMetroid.Core.Enemies.MorphBallEye;

/// <summary>Where a morph-ball eye is mounted (#1472, #1519).</summary>
/// <param name="Facing">The way it faces while it waits.</param>
/// <param name="Part">Which part of the morph-ball eye it is.</param>
/// <param name="MountSide">The side of the eye's centre a beam mount sits on; the eye body ignores it.</param>
internal sealed record MorphBallEyeSpawnSettings(HorizontalDirection Facing, MorphBallEyePart Part, MorphBallEyeMountSide MountSide) : EnemySpawnSettings;
