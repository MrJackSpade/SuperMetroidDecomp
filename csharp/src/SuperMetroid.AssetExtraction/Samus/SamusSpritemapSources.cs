using System.Diagnostics.CodeAnalysis;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Samus.Artwork;
using static SuperMetroid.AssetExtraction.Samus.SamusBodySheetDefinitions;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>
/// Samus's pose-half table as the import pipeline holds it (#1463): beside the selection gameplay reads, each
/// composition's editable sprite parts, which the body and atmosphere sheets are composed from and which decide the body
/// halves each composition's picture shows.
/// </summary>
public sealed class SamusSpritemapSources : InstalledSamusSpritemaps
{
    /// <summary>The largest number of parts one composition may hold.</summary>
    private const int MaxParts = 128;

    /// <summary>Validated selected compositions, each at its identity's ordinal.</summary>
    internal readonly SamusSpritemapDefinition[] definitions;

    /// <summary>Validates pose selectors, table entries, and compositions, copying sprite parts and retaining independently edited selections without replacing animation or collision mechanics.</summary>
    /// <param name="topBases">253 upper-half table element indices in native pose order, each below <see cref="InstalledSamusSpritemaps.EntryCount"/>.</param>
    /// <param name="bottomBases">253 lower-half table element indices in native pose order, each below <see cref="InstalledSamusSpritemaps.EntryCount"/>.</param>
    /// <param name="entries">2,096 table entries; each names a supplied composition, or none for a slot that draws nothing.</param>
    /// <param name="definitions">Non-null compositions, each identified by its position, with at most 128 non-null-array sprite parts.</param>
    /// <exception cref="InvalidDataException">Selector dimensions or indices, compositions, or entry coverage are invalid.</exception>
    public SamusSpritemapSources(ushort[] topBases, ushort[] bottomBases,
        SamusSpritemapId?[] entries, SamusSpritemapDefinition[] definitions)
        : base(topBases, bottomBases, entries, HalvesShown(Validated(definitions)))
    {
        this.definitions = Validated(definitions);
    }

    /// <summary>SHA-256 of selected pose bases, table entries and ordered composition parts.</summary>
    public override string ContentIdentity => field ??= SelectedPresentationHash.Create("samus-spritemaps-v3", content =>
    {
        content.AppendWords("top bases", TopBases);
        content.AppendWords("bottom bases", BottomBases);
        foreach (SamusSpritemapId? entry in Entries)
            content.Append("entry", entry?.Ordinal ?? -1);
        foreach (SamusSpritemapDefinition definition in definitions)
        {
            content.Append("part count", definition.Parts.Length);
            foreach (SpritePart part in definition.Parts)
                part.AppendIdentity(content);
        }
    });

    /// <summary>The stored compositions, each at its identity's ordinal.</summary>
    internal IReadOnlyList<SamusSpritemapDefinition> Definitions => definitions;

    /// <summary>Whole frames already built, by composition.</summary>
    private readonly System.Collections.Concurrent.ConcurrentDictionary<SamusSpritemapId, SpriteFrame> frames = new();

    /// <summary>
    /// The spritemap at <paramref name="index"/> as a whole frame named by its composition, whose parts' characters
    /// are their sheet characters. Returns false only for an entry with no composition, which draws nothing.
    /// </summary>
    public bool TryGetFrame(int index, [NotNullWhen(true)] out SpriteFrame? frame)
    {
        if (!TryGetComposition(index, out SamusSpritemapId composition))
        {
            frame = null;
            return false;
        }
        frame = frames.GetOrAdd(composition, selected => Frame(selected, definitions[selected.Ordinal]));
        return true;
    }

    /// <summary>Returns false only for an entry with no composition, which draws nothing.</summary>
    public bool TryGet(int index, [NotNullWhen(true)] out SamusSpritemapDefinition? definition)
    {
        definition = TryGetComposition(index, out SamusSpritemapId composition) ? definitions[composition.Ordinal] : null;
        return definition is not null;
    }

    /// <summary>Requires every composition to be present with at most <see cref="MaxParts"/> parts, and copies them.</summary>
    private static SamusSpritemapDefinition[] Validated(SamusSpritemapDefinition[] definitions) =>
        [.. Ensure.NotNull(definitions).Select(definition =>
            definition is not { Parts: { Length: <= MaxParts } parts }
                ? throw new InvalidDataException("Samus spritemap has an invalid composition.")
                : new SamusSpritemapDefinition((SpritePart[])parts.Clone()))];

    /// <summary><paramref name="definition"/> as a whole frame named by <paramref name="composition"/>.</summary>
    private static SpriteFrame Frame(SamusSpritemapId composition, SamusSpritemapDefinition definition) =>
        new($"Samus.{composition.Ordinal}", [.. definition.Parts.Select(part => SpritePartFrames.Part(part, 0))]);

    /// <summary>The body halves each composition's picture shows, at its ordinal.</summary>
    private static SamusBodyHalvesShown[] HalvesShown(SamusSpritemapDefinition[] definitions) =>
        [.. definitions.Select((definition, ordinal) => HalvesShown(Frame(new SamusSpritemapId(ordinal), definition)))];

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
