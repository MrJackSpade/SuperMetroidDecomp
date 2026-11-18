namespace SuperMetroid.Core.Enemies.MorphBallEye;

/// <summary>
/// Which part of a morph-ball eye a placement is (#1472, #1519): the eye's body or its beam mount, each its own settings
/// (#2474), so the body carries only its facing and the mount only its side.
/// </summary>
internal abstract record MorphBallEyeSpawnSettings : EnemySpawnSettings;

/// <summary>A morph-ball eye body's placement: the way it faces.</summary>
/// <param name="Facing">The way it faces while it waits.</param>
internal sealed record MorphBallEyeBodySpawnSettings(HorizontalDirection Facing) : MorphBallEyeSpawnSettings;

/// <summary>A morph-ball eye's beam mount's placement: the side of the eye's centre it sits on.</summary>
/// <param name="Side">The side of the eye's centre the mount sits on.</param>
internal sealed record MorphBallEyeMountSpawnSettings(MorphBallEyeMountSide Side) : MorphBallEyeSpawnSettings;
