namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>
/// One six-byte cinematic-object definition together with the fixed position and
/// sprite colour set written by its initialization callback.
/// </summary>
internal readonly record struct IntroBabyActorDefinition(
    CinematicSpriteFrameBehavior FrameBehavior,
    CinematicSpriteProgram Program,
    Position16<ScreenPixels> Position,
    ScreenColourGroup Colouring);
