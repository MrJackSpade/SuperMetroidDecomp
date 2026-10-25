namespace SuperMetroid.Core.Frontend.Title;

/// <summary>One title card: the phase it runs in, where it draws and with which sprite palette.</summary>
/// <param name="Phase">Runtime title-card phase entered when the card starts.</param>
/// <param name="Origin">Drawing origin in screen pixels.</param>
/// <param name="Palette">The sprite palette row the card's compositions draw with.</param>
public readonly record struct TitleTextSequenceDefinition(
    TitleSequencePhase Phase,
    Position16<ScreenPixels> Origin,
    int Palette);
