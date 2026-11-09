using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>
/// What one whole Samus body picture is composed of (#1513, #1585): the compositions its top and bottom halves show, the
/// half pictures whose characters they read, and the chest cover drawn between them. Bodies with equal keys show the same
/// picture, which the body sheet holds once under each body's name.
/// </summary>
/// <param name="Top">The top half's composition, or null when the top half shows none.</param>
/// <param name="Bottom">The bottom half's composition, or null when the body shows no bottom half or it shows none.</param>
/// <param name="TopPicture">The top half picture it reads, or null when no composition reads a top character.</param>
/// <param name="BottomPicture">The bottom half picture it reads, or null when no composition reads a bottom character or the frame names none.</param>
/// <param name="ChestCover">Where the chest cover sits relative to the picture's origin, or null for a picture without one.</param>
internal readonly record struct SamusBodyPictureKey(SamusSpritemapId? Top, SamusSpritemapId? Bottom, SamusBodyPicture? TopPicture,
    SamusBodyPicture? BottomPicture, Offset16<ScreenPixels>? ChestCover);
