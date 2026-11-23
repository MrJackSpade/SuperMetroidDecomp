using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>
/// Samus's body characters: the tile transfers each named half picture makes visible, bound to the body catalog whose
/// frames select them. The body sheets are composed from these at installation; gameplay draws the sheets and never reads
/// these characters.
/// </summary>
public sealed partial class SamusBodyCharacterArtwork
{
    /// <summary>Maximum tile count represented by one definition slot.</summary>
    public const int TilesPerDefinition = 16;
    /// <summary>Maximum planar-byte capacity of one definition slot.</summary>
    public const int BytesPerDefinitionSlot = TilesPerDefinition * 32;
    /// <summary>Characters in Samus's body artwork: two sixteen-wide rows.</summary>
    internal const int BodyCharacterCount = 2 * SpriteArtwork.CharactersPerRow;
    /// <summary>Characters in one half row, filled by one transfer.</summary>
    private const int HalfRowCharacters = SpriteArtwork.CharactersPerRow / 2;

    /// <summary>The tile transfers each half picture makes, by picture.</summary>
    internal readonly Dictionary<SamusBodyPicture, SamusBodyTileTransfer> pictures;
    /// <summary>Composed body characters per selected pair, built once.</summary>
    private readonly System.Collections.Concurrent.ConcurrentDictionary<BodyHalvesVariant, SpriteArtwork> bodyArtwork = new();

    /// <summary>A composed body sheet, distinguished by the top and bottom half pictures it composes.</summary>
    /// <param name="Top">The top half picture, or none.</param>
    /// <param name="Bottom">The bottom half picture, or none.</param>
    private sealed partial record BodyHalvesVariant(SamusBodyPicture? Top, SamusBodyPicture? Bottom) : SpriteArtworkVariant;

    /// <summary>The body whose frames select these characters.</summary>
    public SamusBodySources Body { get; }

    /// <summary>Binds <paramref name="pictures"/> to the frames of <paramref name="body"/>.</summary>
    /// <param name="body">The body catalog whose frames select these characters.</param>
    /// <param name="pictures">The tile transfers each half picture makes.</param>
    /// <exception cref="InvalidDataException">A transfer is malformed, or a frame selects a picture with none.</exception>
    internal SamusBodyCharacterArtwork(SamusBodySources body, IReadOnlyDictionary<SamusBodyPicture, SamusBodyTileTransfer> pictures)
    {
        Body = Ensure.NotNull(body);
        this.pictures = Ensure.NotNull(pictures).ToDictionary();
        foreach ((SamusBodyPicture picture, SamusBodyTileTransfer transfer) in this.pictures)
        {
            if (transfer is null || transfer.FirstSize == 0 || transfer.FirstSize + transfer.SecondSize > BytesPerDefinitionSlot ||
                (transfer.FirstSize + transfer.SecondSize) % 32 != 0 || transfer.Planar.Length != transfer.FirstSize + transfer.SecondSize)
                throw new InvalidDataException($"Samus body picture {picture.Name} has invalid tile data.");
        }
        foreach (SamusBodyDecodedFrame frame in body.Frames.Values.SelectMany(pose => pose.Values))
        {
            foreach (SamusBodyPicture? selected in new SamusBodyPicture?[] { frame.Pictures.Top, frame.Pictures.Bottom })
                if (selected is { } picture && !this.pictures.ContainsKey(picture))
                    throw new InvalidDataException($"Samus body picture {picture.Name} is absent from installed art.");
        }
    }

    /// <summary>Builds the body catalog from <paramref name="parts"/> and binds <paramref name="pictures"/> to it.</summary>
    /// <param name="parts">The body's decoded frames and catalogs.</param>
    /// <param name="pictures">The tile transfers each half picture makes.</param>
    public static SamusBodyCharacterArtwork Create(SamusBodyArtworkParts parts, IReadOnlyDictionary<SamusBodyPicture, SamusBodyTileTransfer> pictures)
    {
        Ensure.NotNull(parts);
        return new(new SamusBodySources(parts), pictures);
    }

    /// <summary>
    /// Samus's 32 body characters as the selected half pictures make them visible: each picture's first transfer fills
    /// one eight-character half row (top: characters 0-7, bottom: 8-15) and its second transfer the half row below it
    /// (top: 16-23, bottom: 24-31). A half with no selected picture is blank.
    /// </summary>
    /// <param name="topPicture">The selected top half picture, or null.</param>
    /// <param name="bottomPicture">The selected bottom half picture, or null.</param>
    public SpriteArtwork BodyArtwork(SamusBodyPicture? topPicture, SamusBodyPicture? bottomPicture) =>
        bodyArtwork.GetOrAdd(new BodyHalvesVariant(topPicture, bottomPicture), key =>
        {
            var characters = new byte[BodyCharacterCount * SpriteArtwork.CharacterByteCount];
            if (key.Top is { } upper)
                CopyHalves(Transfer(upper), characters, 0);
            if (key.Bottom is { } lower)
                CopyHalves(Transfer(lower), characters, HalfRowCharacters);
            return new SpriteArtwork(new SpriteArtworkId(SpriteArtworkKind.SamusBody, key), characters);
        });

    /// <summary>The tile transfers <paramref name="picture"/> makes.</summary>
    /// <exception cref="InvalidDataException">The picture is absent from installed art.</exception>
    internal SamusBodyTileTransfer Transfer(SamusBodyPicture picture) =>
        pictures.TryGetValue(picture, out SamusBodyTileTransfer? transfer)
            ? transfer
            : throw new InvalidDataException($"Samus body picture {picture.Name} is absent from installed art.");

    /// <summary>Copies <paramref name="definition"/>'s two transfers into their half rows from <paramref name="firstCharacter"/>.</summary>
    private static void CopyHalves(SamusBodyTileTransfer definition, byte[] characters, int firstCharacter)
    {
        ReadOnlySpan<byte> planar = definition.Planar.Span;
        planar[..definition.FirstSize].CopyTo(characters.AsSpan(firstCharacter * SpriteArtwork.CharacterByteCount));
        planar[definition.FirstSize..].CopyTo(characters.AsSpan(
            (firstCharacter + SpriteArtwork.CharactersPerRow) * SpriteArtwork.CharacterByteCount));
    }
}
