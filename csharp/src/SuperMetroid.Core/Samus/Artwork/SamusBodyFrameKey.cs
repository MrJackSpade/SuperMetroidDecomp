namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>
/// A pre-coloured full-body Samus frame (#1513): the compositions its top and bottom halves show, the half pictures whose
/// characters they show, and the chest cover drawn between them.
/// </summary>
/// <param name="Top">The top half's composition, or null when the top half shows none.</param>
/// <param name="Bottom">The bottom half's composition, or null when the frame shows no bottom half or it shows none.</param>
/// <param name="TopPicture">The top half picture it shows, or null when no composition reads a top character or none is selected.</param>
/// <param name="BottomPicture">The bottom half picture it shows, or null when no composition reads a bottom character or none is selected.</param>
/// <param name="ChestCover">Where the chest cover sits relative to the frame's origin, or null for a frame without one.</param>
public readonly record struct SamusBodyFrameKey(SamusSpritemapId? Top, SamusSpritemapId? Bottom, SamusBodyPicture? TopPicture,
    SamusBodyPicture? BottomPicture, Offset16<ScreenPixels>? ChestCover)
{
    /// <summary>The frame's name on the body sheet.</summary>
    public string Name => $"{Ordinal(Top?.Ordinal)}.{Ordinal(Bottom?.Ordinal)}.{Ordinal(TopPicture?.Ordinal)}.{Ordinal(BottomPicture?.Ordinal)}" +
        (ChestCover is { } chest
            ? $".chest.{chest.X.ToString(System.Globalization.CultureInfo.InvariantCulture)}.{chest.Y.ToString(System.Globalization.CultureInfo.InvariantCulture)}"
            : "");

    /// <summary>A named part's ordinal, or a dash for none.</summary>
    private static string Ordinal(int? ordinal) =>
        ordinal is { } value ? value.ToString(System.Globalization.CultureInfo.InvariantCulture) : "-";
}
