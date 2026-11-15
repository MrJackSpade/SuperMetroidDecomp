using SuperMetroid.Core.Frontend.Intro.Artwork;

namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>
/// One cinematic-actor definition together with the fixed position and
/// colouring its initialization draws it in (#1512).
/// </summary>
internal readonly record struct IntroBabyActorDefinition(
    CinematicSpriteSegment Animation,
    Offset16<ScreenPixels> Position,
    IntroSpriteColouring Colouring);
