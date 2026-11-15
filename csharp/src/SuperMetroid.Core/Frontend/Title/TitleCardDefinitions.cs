namespace SuperMetroid.Core.Frontend.Title;

/// <summary>
/// The four title cards (#1979): each types its text one letter per eight updates, holds its whole text, then triggers
/// its turning-floor scene and removes itself. <see cref="TitleCards"/> builds each card's steps.
/// </summary>
internal static class TitleCardDefinitions
{
    /// <summary>Each letter's duration before the text's final hold, in title updates.</summary>
    internal const int LetterFrames = 8;

    /// <summary>
    /// How far a card's text moves each time it types a letter (a space included): one letter's width leftward, so each new
    /// letter appears at the same place on the right while the letters before it scroll left.
    /// </summary>
    internal static readonly Offset16<SpritePixels> TypedScroll = Offset16<SpritePixels>.Of(-8, 0);

    /// <summary>The year card: a 60-update blank opening, then 1994, held for 45 updates before the lower pan.</summary>
    internal static readonly TitleCardDefinition Year =
        new(TitleSequencePhase.YearText, "1994", OpeningFrames: 60, HoldFrames: 45, TitleCardCue.StartLowerPan);

    /// <summary>The Nintendo card, held for 45 updates before the upper pan.</summary>
    internal static readonly TitleCardDefinition Nintendo =
        new(TitleSequencePhase.NintendoText, "NINTENDO", OpeningFrames: 0, HoldFrames: 45, TitleCardCue.StartUpperPan);

    /// <summary>The Presents card, held for 45 updates before the downward pan.</summary>
    internal static readonly TitleCardDefinition Presents =
        new(TitleSequencePhase.PresentsText, "PRESENTS", OpeningFrames: 0, HoldFrames: 45, TitleCardCue.StartDownwardPan);

    /// <summary>The Metroid 3 card, the space before its numeral typed as a letter, held for 120 updates before the final zoom.</summary>
    internal static readonly TitleCardDefinition MetroidThree =
        new(TitleSequencePhase.MetroidThreeText, "METROID 3", OpeningFrames: 0, HoldFrames: 120, TitleCardCue.StartFinalZoom);
}
