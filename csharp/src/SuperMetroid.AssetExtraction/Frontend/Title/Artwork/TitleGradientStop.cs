namespace SuperMetroid.AssetExtraction.Frontend.Title.Artwork;

/// <summary>
/// One stop of the title's gradient (#1468): the line its band begins on, the colour the band blends in and how it blends
/// it (#1709). The band runs down to the next stop. The colour is written as <c>#rrggbb</c> (#2564).
/// </summary>
public sealed record TitleGradientStop
{
    /// <summary>The screen line the band begins on, counted from the top of the screen.</summary>
    public required int Line { get; init; }
    /// <summary>The colour the band blends in, on the game's fade scale.</summary>
    public required string Colour { get; init; }
    /// <summary>
    /// The name of how the band's colour blends: <c>DarkensFloorAndBackdrop</c>, subtracted from the floor and the backdrop,
    /// or <c>BrightensFloorBackdropAndSprites</c>, added to the floor, the backdrop and the sprites.
    /// </summary>
    public required string Blend { get; init; }
}
