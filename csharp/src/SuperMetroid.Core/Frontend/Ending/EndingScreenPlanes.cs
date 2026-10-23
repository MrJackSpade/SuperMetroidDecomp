using SuperMetroid.Core.Frontend.Ending.Artwork;

namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>
/// What the ending's background planes show (#1404): each plane's words are those of the newest upload that writes the
/// plane, a tilemap the ending's scenes wrote or the installed artwork a scene loaded for it.
/// </summary>
internal static class EndingScreenPlanes
{
    /// <summary>Cells in one plane: one 32-cell screen.</summary>
    internal const int Cells = 32 * 32;

    /// <summary>The words <paramref name="plane"/> holds once <paramref name="uploads"/> have been made, in order.</summary>
    /// <exception cref="InvalidOperationException">An upload names installed artwork that is not bound.</exception>
    internal static ushort[] Of(EndingPlane plane, IReadOnlyList<EndingUpload> uploads, EndingObjectArtworkCatalog? objects)
    {
        var words = new ushort[Cells];
        foreach (EndingUpload upload in uploads)
        {
            switch (plane)
            {
                case EndingPlane.Credits:
                    if (upload.Kind == EndingUploadKind.CreditsTilemap || IsText(upload, EndingTextTilemap.ResultPanel))
                        Words(upload).CopyTo(words);
                    break;
                case EndingPlane.Text:
                    if (IsText(upload, EndingTextTilemap.FinalText))
                        Words(upload).CopyTo(words);
                    else if (upload.Kind == EndingUploadKind.CreditsCharacterArt)
                        Load(Objects(objects).WaitingTilemap.Transfer.Span[..EndingCreditsRomData.Rendering.WaitingTilemapBytes], words);
                    break;
                case EndingPlane.TextLower:
                    if (upload.Kind == EndingUploadKind.PostShotLowerTilemap)
                        Words(upload).CopyTo(words);
                    break;
                case EndingPlane.Logo:
                    if (upload.Kind == EndingUploadKind.PostShotTransfer &&
                        EndingPostShotUploadDefinitions.Get(upload.Key) is { Source: EndingPostShotUploadSource.LogoMap } logoMap)
                        Load(Objects(objects).PostShotLogoMap.Transfer.Span[..logoMap.Length], words);
                    break;
                case EndingPlane.WavySamus:
                    if (upload.Kind == EndingUploadKind.CreditsCharacterArt)
                        Load(Objects(objects).PostCreditsFragmentB.Transfer.Span[..EndingCreditsRomData.Rendering.ObjectFragmentBytes], words);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(plane), plane, "Undefined ending plane.");
            }
        }
        return words;
    }

    /// <summary>Whether <paramref name="upload"/> writes the text tilemap <paramref name="tilemap"/>.</summary>
    private static bool IsText(EndingUpload upload, EndingTextTilemap tilemap) =>
        upload.Kind == EndingUploadKind.TextTilemap && (EndingTextTilemap)upload.Key == tilemap;

    /// <summary>The words the explosion's <paramref name="fragment"/> map holds once <paramref name="uploads"/> have been made.</summary>
    /// <exception cref="InvalidOperationException">An upload names installed artwork that is not bound.</exception>
    internal static ushort[] Explosion(EndingObjectFragmentId fragment, IReadOnlyList<EndingUpload> uploads, EndingObjectArtworkCatalog? objects)
    {
        var words = new ushort[Cells];
        foreach (EndingUpload upload in uploads)
        {
            if (upload.Kind == EndingUploadKind.ObjectFragment && (EndingObjectFragmentId)upload.Key == fragment)
                Load(Objects(objects).Fragment(fragment).Transfer.Span[..EndingCreditsRomData.Rendering.ObjectFragmentBytes], words);
        }
        return words;
    }

    /// <summary>The tilemap words a tilemap upload recorded.</summary>
    private static ReadOnlySpan<ushort> Words(EndingUpload upload) =>
        upload.Words ?? throw new InvalidOperationException($"The {upload.Kind} transfer records no tilemap words.");

    /// <summary>Loads little-endian words over the start of <paramref name="words"/>.</summary>
    private static void Load(ReadOnlySpan<byte> littleEndianWords, Span<ushort> words)
    {
        for (int word = 0; word < littleEndianWords.Length / sizeof(ushort) && word < words.Length; word++)
            words[word] = SnesLittleEndian.Word(littleEndianWords[word * 2], littleEndianWords[word * 2 + 1]);
    }

    /// <summary>The bound OBJ artwork, which a plane's artwork uploads require.</summary>
    private static EndingObjectArtworkCatalog Objects(EndingObjectArtworkCatalog? objects) =>
        objects ?? throw new InvalidOperationException("Ending objects require installed artwork.");
}
