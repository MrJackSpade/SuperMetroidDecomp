using SuperMetroid.Core.Frontend.Title.Artwork;

namespace SuperMetroid.Core.Frontend.Title;

/// <summary>One title card: the phase it runs in, where it draws and in which of the title's colours.</summary>
/// <param name="Phase">Runtime title-card phase entered when the card starts.</param>
/// <param name="Origin">Drawing origin in screen pixels.</param>
/// <param name="Colouring">What the title shows the card's compositions as (#1512).</param>
public readonly record struct TitleTextSequenceDefinition(
    TitleSequencePhase Phase,
    Position16<ScreenPixels> Origin,
    TitleSpriteColouring Colouring);
