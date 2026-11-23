namespace SuperMetroid.Core.Enemies.MorphBallEye;

/// <summary>A morph-ball eye body's placement: the way it faces.</summary>
/// <param name="Facing">The way it faces while it waits.</param>
internal sealed partial record MorphBallEyeBodySpawnSettings(HorizontalDirection Facing) : MorphBallEyeSpawnSettings;
