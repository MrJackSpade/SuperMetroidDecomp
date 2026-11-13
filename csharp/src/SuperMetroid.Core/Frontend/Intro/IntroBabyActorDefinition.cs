using SuperMetroid.Core.Frontend.Intro.Artwork;

namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>
/// One cinematic-object definition together with the fixed position and
/// colouring its initialization callback draws it in (#1512).
/// </summary>
internal readonly record struct IntroBabyActorDefinition(
    CinematicSpriteFrameBehavior FrameBehavior,
    CinematicSpriteSegment Animation,
    Position16<ScreenPixels> Position,
    IntroSpriteColouring Colouring);
