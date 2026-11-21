using System.Diagnostics.CodeAnalysis;
using SuperMetroid.Core;
using static SuperMetroid.AssetExtraction.Samus.SamusBodySheetDefinitions;
using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>
/// Samus's compositions as the import pipeline holds them (#1463, #1465, #2556): the composition each pose half shows at
/// each of its decoded picture frames, the compositions the suit explosion and the diving splash and bubbles show, and
/// each composition's editable sprite parts, which the body, death and atmosphere sheets are composed from and which
/// decide the body halves each composition's picture shows. The import decoded them once from the cartridge's tables; the
/// body sheet holds one whole picture per pose and picture frame, composed from them (#1585).
/// </summary>
public sealed class SamusSpritemapSources
{
    /// <summary>The largest number of parts one composition may hold.</summary>
    private const int MaxParts = 128;

    /// <summary>Validated compositions, by identity.</summary>
    private readonly Dictionary<SamusSpritemapId, SamusSpritemapDefinition> definitions;
    /// <summary>The composition each decoded pose half frame shows, or none for one that draws nothing.</summary>
    private readonly Dictionary<SamusBodyHalfFrame, SamusSpritemapId?> halves;
    /// <summary>The composition each suit-explosion sheet frame shows, or none.</summary>
    private readonly Dictionary<string, SamusSpritemapId?> explosion;
    /// <summary>The composition each diving-splash and bubbles sheet frame shows, or none.</summary>
    private readonly Dictionary<string, SamusSpritemapId?> atmosphere;
    /// <summary>The body halves each composition reads characters of.</summary>
    private readonly Dictionary<SamusSpritemapId, SamusBodyHalvesShown> halvesShown;
    /// <summary>Whole frames already built, by composition.</summary>
    private readonly System.Collections.Concurrent.ConcurrentDictionary<SamusSpritemapId, SpriteFrame> frames = new();

    /// <summary>Validates the compositions and every reference to them, copying sprite parts.</summary>
    /// <param name="definitions">Each composition by identity, with at most 128 parts.</param>
    /// <param name="halves">The composition each decoded pose half frame shows, or none for one that draws nothing.</param>
    /// <param name="explosion">The composition each suit-explosion sheet frame shows, or none.</param>
    /// <param name="atmosphere">The composition each diving-splash and bubbles sheet frame shows, or none.</param>
    /// <exception cref="InvalidDataException">A composition is invalid, or a reference names none of them.</exception>
    public SamusSpritemapSources(IReadOnlyDictionary<SamusSpritemapId, SamusSpritemapDefinition> definitions,
        IReadOnlyDictionary<SamusBodyHalfFrame, SamusSpritemapId?> halves, IReadOnlyDictionary<string, SamusSpritemapId?> explosion,
        IReadOnlyDictionary<string, SamusSpritemapId?> atmosphere)
    {
        this.definitions = Ensure.NotNull(definitions).ToDictionary(pair => pair.Key, pair =>
            pair.Value is not { Parts: { Length: <= MaxParts } parts }
                ? throw new InvalidDataException($"Samus spritemap {pair.Key.Name} has an invalid composition.")
                : new SamusSpritemapDefinition((SpritePart[])parts.Clone()));
        this.halves = Referenced(Ensure.NotNull(halves));
        this.explosion = Referenced(Ensure.NotNull(explosion));
        this.atmosphere = Referenced(Ensure.NotNull(atmosphere));
        halvesShown = this.definitions.ToDictionary(pair => pair.Key, pair => HalvesShown(Frame(pair.Key, pair.Value)));
    }

    /// <summary>The composition <paramref name="frame"/> shows; false only for a frame with none, which composes nothing.</summary>
    /// <exception cref="InvalidDataException">The pose half frame is not decoded.</exception>
    private bool TryGetComposition(SamusBodyHalfFrame frame, out SamusSpritemapId composition)
    {
        SamusSpritemapId? shown = halves.TryGetValue(frame, out SamusSpritemapId? decoded)
            ? decoded
            : throw new InvalidDataException($"Samus {frame} is outside installed art.");
        composition = shown ?? default;
        return shown is not null;
    }

    /// <summary>
    /// What the whole picture <paramref name="body"/> is composed of with <paramref name="pictures"/> selected (#1513,
    /// #1585): the compositions its top and bottom halves show, the selected half picture of each body half either
    /// composition reads, and the chest cover between them. Null when neither half has a composition and there is no
    /// chest cover: the picture shows nothing.
    /// </summary>
    /// <param name="body">The halves and chest cover the picture is composed of.</param>
    /// <param name="pictures">The half pictures the frame selects.</param>
    /// <exception cref="InvalidDataException">A half frame is not decoded.</exception>
    internal SamusBodyPictureKey? PictureKey(SamusBodyComposition body, SamusBodyFramePictures pictures)
    {
        SamusSpritemapId? topComposition = TryGetComposition(body.Top, out SamusSpritemapId shownTop) ? shownTop : null;
        SamusSpritemapId? bottomComposition = body.Bottom is { } lower && TryGetComposition(lower, out SamusSpritemapId shownBottom)
            ? shownBottom : null;
        if (topComposition is null && bottomComposition is null && body.ChestCover is null)
            return null;
        bool showsTop = Shows(topComposition, SamusBodyHalvesShown.Top) || Shows(bottomComposition, SamusBodyHalvesShown.Top);
        bool showsBottom = Shows(topComposition, SamusBodyHalvesShown.Bottom) || Shows(bottomComposition, SamusBodyHalvesShown.Bottom);
        return new SamusBodyPictureKey(topComposition, bottomComposition, showsTop ? pictures.Top : null,
            showsBottom ? pictures.Bottom : null, body.ChestCover);
    }

    /// <summary>Whether <paramref name="composition"/> reads <paramref name="half"/>'s picture.</summary>
    private bool Shows(SamusSpritemapId? composition, SamusBodyHalvesShown half) =>
        composition is { } shown && (halvesShown[shown] == half || halvesShown[shown] == SamusBodyHalvesShown.Both);

    /// <summary>Whether <paramref name="frame"/> is a decoded pose half frame.</summary>
    internal bool Covers(SamusBodyHalfFrame frame) => halves.ContainsKey(frame);

    /// <summary>
    /// The composition <paramref name="halfFrame"/> shows as a whole frame; false for a frame that is not decoded or one
    /// with no composition, which draws nothing.
    /// </summary>
    public bool TryGetFrame(SamusBodyHalfFrame halfFrame, [NotNullWhen(true)] out SpriteFrame? frame) =>
        TryGetFrame(halves.GetValueOrDefault(halfFrame), out frame);

    /// <summary>The composition the suit-explosion sheet frame <paramref name="name"/> shows as a whole frame; false for one that shows nothing.</summary>
    /// <exception cref="InvalidDataException">The explosion has no such frame.</exception>
    public bool TryGetExplosionFrame(string name, [NotNullWhen(true)] out SpriteFrame? frame) =>
        TryGetFrame(explosion.TryGetValue(name, out SamusSpritemapId? composition)
            ? composition : throw new InvalidDataException($"Samus's explosion has no frame {name}."), out frame);

    /// <summary>The composition the diving-splash or bubbles sheet frame <paramref name="name"/> shows as a whole frame; false for one that shows nothing.</summary>
    /// <exception cref="InvalidDataException">The atmosphere has no such frame.</exception>
    public bool TryGetAtmosphereFrame(string name, [NotNullWhen(true)] out SpriteFrame? frame) =>
        TryGetFrame(atmosphere.TryGetValue(name, out SamusSpritemapId? composition)
            ? composition : throw new InvalidDataException($"Samus's atmosphere has no frame {name}."), out frame);

    /// <summary><paramref name="composition"/> as a whole frame whose parts' characters are their sheet characters; false for none.</summary>
    private bool TryGetFrame(SamusSpritemapId? composition, [NotNullWhen(true)] out SpriteFrame? frame)
    {
        if (composition is not { } shown)
        {
            frame = null;
            return false;
        }
        frame = frames.GetOrAdd(shown, selected => Frame(selected, definitions[selected]));
        return true;
    }

    /// <summary><paramref name="references"/>, copied, each of which must name one of the compositions.</summary>
    /// <exception cref="InvalidDataException">A reference names no composition.</exception>
    private Dictionary<TKey, SamusSpritemapId?> Referenced<TKey>(IReadOnlyDictionary<TKey, SamusSpritemapId?> references) where TKey : notnull
    {
        foreach (SamusSpritemapId? reference in references.Values)
            if (reference is { } composition && !definitions.ContainsKey(composition))
                throw new InvalidDataException($"Samus spritemap {composition.Name} is absent from installed art.");
        return references.ToDictionary();
    }

    /// <summary><paramref name="definition"/> as a whole frame named by <paramref name="composition"/>.</summary>
    private static SpriteFrame Frame(SamusSpritemapId composition, SamusSpritemapDefinition definition) =>
        new(composition.Name, [.. definition.Parts.Select(part => SpritePartFrames.Part(part, 0))]);

    /// <summary>
    /// The body halves <paramref name="frame"/> reads characters of: a body character in the first eight columns of
    /// its sixteen-wide row is the top half's, any other body character the bottom half's.
    /// </summary>
    private static SamusBodyHalvesShown HalvesShown(SpriteFrame frame)
    {
        bool top = false, bottom = false;
        foreach (SpriteFramePart part in frame.Parts)
        {
            int columns = part.Size.X / SpriteArtwork.CharacterPixels, rows = part.Size.Y / SpriteArtwork.CharacterPixels;
            for (int row = 0; row < rows; row++)
            {
                for (int column = 0; column < columns; column++)
                {
                    int character = part.Character + row * SpriteArtwork.CharactersPerRow + column;
                    if (character >= BodyCharacterCount)
                        continue;
                    if (Column(character) < TopHalfColumns)
                        top = true;
                    else
                        bottom = true;
                }
            }
        }
        if (top)
            return bottom ? SamusBodyHalvesShown.Both : SamusBodyHalvesShown.Top;
        return bottom ? SamusBodyHalvesShown.Bottom : SamusBodyHalvesShown.Neither;
    }

    /// <summary>The column of <paramref name="character"/> within its sixteen-wide character row, 0 through 15.</summary>
    private static int Column(int character)
    {
        int remainder = character % SpriteArtwork.CharactersPerRow;
        return remainder < 0 ? remainder + SpriteArtwork.CharactersPerRow : remainder;
    }
}
