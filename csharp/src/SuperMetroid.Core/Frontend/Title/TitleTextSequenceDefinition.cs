using SuperMetroid.Core.Frontend.Title.Artwork;

namespace SuperMetroid.Core.Frontend.Title;

/// <summary>One title card's showing: the phase it runs in, the card it types, where it draws and in which of the title's colours.</summary>
/// <param name="Phase">Runtime title-card phase entered when the card starts.</param>
/// <param name="Card">The card the phase types.</param>
/// <param name="Origin">Drawing origin in screen pixels.</param>
/// <param name="Colouring">What the title shows the card's compositions as (#1512).</param>
public readonly record struct TitleTextSequenceDefinition(
    TitleSequencePhase Phase,
    TitleCardDefinition Card,
    Offset16<ScreenPixels> Origin,
    TitleSpriteColouring Colouring);
