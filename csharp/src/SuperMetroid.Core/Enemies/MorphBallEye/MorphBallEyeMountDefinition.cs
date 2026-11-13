namespace SuperMetroid.Core.Enemies.MorphBallEye;

/// <summary>Where the morph-ball eye is mounted, eight pixels from its center, and the animation it starts.</summary>
/// <param name="Offset">The mount's offset from the eye's center.</param>
/// <param name="Animation">The animation the eye starts.</param>
internal readonly record struct MorphBallEyeMountDefinition(Offset16<RoomPixels> Offset, MorphBallEyeSegment Animation);
