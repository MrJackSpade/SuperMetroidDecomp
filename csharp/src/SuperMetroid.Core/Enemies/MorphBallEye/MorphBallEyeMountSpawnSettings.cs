namespace SuperMetroid.Core.Enemies.MorphBallEye;

/// <summary>A morph-ball eye's beam mount's placement: the side of the eye's centre it sits on.</summary>
/// <param name="Side">The side of the eye's centre the mount sits on.</param>
internal sealed partial record MorphBallEyeMountSpawnSettings(MorphBallEyeMountSide Side) : MorphBallEyeSpawnSettings;
