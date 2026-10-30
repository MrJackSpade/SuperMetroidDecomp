namespace SuperMetroid.Core.Frontend.Title.Artwork;

/// <summary>
/// One stop of the title's gradient (#1468): the line its band begins on, the colour the band blends in, which title
/// layers it blends with, and whether it is added to them or subtracted from them. The band runs down to the next stop.
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
    /// <summary>Whether the band's colour blends with the title's perspective floor.</summary>
    public required bool BlendsFloor { get; init; }
    /// <summary>Whether the band's colour blends with the title objects drawn in palettes four through seven.</summary>
    public required bool BlendsObjects { get; init; }
    /// <summary>Whether the band's colour blends with the backdrop behind the floor and objects.</summary>
    public required bool BlendsBackdrop { get; init; }
    /// <summary>Whether the band's colour is subtracted from the layers it blends with rather than added to them.</summary>
    public required bool Subtracts { get; init; }
}
