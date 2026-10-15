namespace SuperMetroid.Core.Frontend.Title;

/// <summary>Initial Mode-7 transform installed by one title-scene command.</summary>
/// <param name="Phase">Pan or zoom phase entered by the scene-trigger command.</param>
/// <param name="Scale">Initial native 8.8 transform scale, with $0100 representing unity.</param>
/// <param name="Position">Background position in pixels as two's-complement words, before fixed-point pan updates.</param>
public readonly record struct TitleMode7SceneDefinition(
    TitleSequencePhase Phase,
    int Scale,
    Position16<Mode7Pixels> Position);
