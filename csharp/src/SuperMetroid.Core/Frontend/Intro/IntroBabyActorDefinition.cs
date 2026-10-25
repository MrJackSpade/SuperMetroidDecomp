namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>
/// One native six-byte cinematic-object definition together with the fixed position and
/// OBJ attributes written by its initialization callback.
/// </summary>
internal readonly record struct IntroBabyActorDefinition(
    CinematicSpritePreInstruction PreInstruction,
    CinematicSpriteProgram Program,
    Position16<ScreenPixels> Position,
    int PaletteBits);
