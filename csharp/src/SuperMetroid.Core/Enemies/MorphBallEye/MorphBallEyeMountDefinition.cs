namespace SuperMetroid.Core.Enemies.MorphBallEye;

/// <summary>Where the morph-ball eye is mounted, one tile from its center, and the program it starts.</summary>
/// <param name="Offset">The mount's offset from the eye's center.</param>
/// <param name="Program">The program the eye starts.</param>
internal readonly record struct MorphBallEyeMountDefinition(Offset16<RoomPixels> Offset, MorphBallEyeProgram Program);
