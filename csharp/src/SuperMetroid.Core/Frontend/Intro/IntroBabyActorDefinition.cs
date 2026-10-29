namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>
/// One native six-byte cinematic-object definition together with the fixed position and
/// sprite palette row written by its initialization callback.
/// </summary>
internal readonly record struct IntroBabyActorDefinition(
    CinematicSpritePreInstruction PreInstruction,
    CinematicSpriteProgram Program,
    Position16<ScreenPixels> Position,
    ScreenColourGroup Colouring);
