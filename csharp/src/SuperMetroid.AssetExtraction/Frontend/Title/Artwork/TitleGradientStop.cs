namespace SuperMetroid.AssetExtraction.Frontend.Title.Artwork;

/// <summary>
/// One stop of the title's gradient (#1468): the line its band begins on, the colour the band blends in and how it blends
/// it (#1709). The band runs down to the next stop.
/// </summary>
public sealed record TitleGradientStop
{
    /// <summary>The screen line the band begins on, counted from the top of the screen.</summary>
    public required int Line { get; init; }
    /// <summary>The band colour's red intensity from 0 through 31, not an eight-bit display-channel value.</summary>
    public required int Red { get; init; }
    /// <summary>The band colour's green intensity from 0 through 31, not an eight-bit display-channel value.</summary>
    public required int Green { get; init; }
    /// <summary>The band colour's blue intensity from 0 through 31, not an eight-bit display-channel value.</summary>
    public required int Blue { get; init; }
    /// <summary>
    /// The name of how the band's colour blends: <c>DarkensFloorAndBackdrop</c>, subtracted from the floor and the backdrop,
    /// or <c>BrightensFloorBackdropAndSprites</c>, added to the floor, the backdrop and the sprites.
    /// </summary>
    public required string Blend { get; init; }
}
