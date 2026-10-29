using System.Diagnostics.CodeAnalysis;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.Core.Samus.Poses;
using static SuperMetroid.AssetExtraction.Samus.SamusBodySheetDefinitions;
using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>
/// Samus's pose-half table as the import pipeline holds it (#1463, #1465): the cartridge's per-pose first entries and
/// its table of entries, from which the import chooses once the composition each pose half shows at each animation
/// frame; and each composition's editable sprite parts, which the body and atmosphere sheets are composed from and
/// which decide the body halves each composition's picture shows. The table's indices are the import's private
/// knowledge: gameplay selects a picture by pose half and animation frame.
/// </summary>
public sealed class SamusSpritemapSources : InstalledSamusSpritemaps
{
    /// <summary>Number of entries in the pose-half table, including repeated compositions and entries with none; not the number of unique compositions or poses.</summary>
    public const int EntryCount = 2096;

    /// <summary>The largest number of parts one composition may hold.</summary>
    private const int MaxParts = 128;

    /// <summary>Validated selected compositions, each at its identity's ordinal.</summary>
    internal readonly SamusSpritemapDefinition[] definitions;
    /// <summary>Each pose's first upper-half table entry, in pose-number order.</summary>
    private readonly ushort[] topBases;
    /// <summary>Each pose's first lower-half table entry, in pose-number order.</summary>
    private readonly ushort[] bottomBases;
    /// <summary>The composition each table entry names, or none for an entry that draws nothing.</summary>
    private readonly SamusSpritemapId?[] entries;

    /// <summary>
    /// Validates pose selectors, table entries, and compositions, copying sprite parts, and resolves the composition
    /// each pose half shows at every animation frame the body sheets cover.
    /// </summary>
    /// <param name="topBases">253 upper-half table element indices in native pose order, each below <see cref="EntryCount"/>.</param>
    /// <param name="bottomBases">253 lower-half table element indices in native pose order, each below <see cref="EntryCount"/>.</param>
    /// <param name="entries">2,096 table entries; each names a supplied composition, or none for a slot that draws nothing.</param>
    /// <param name="definitions">Non-null compositions, each identified by its position, with at most 128 non-null-array sprite parts.</param>
    /// <exception cref="InvalidDataException">Selector dimensions or indices, compositions, or entry coverage are invalid.</exception>
    public SamusSpritemapSources(ushort[] topBases, ushort[] bottomBases,
        SamusSpritemapId?[] entries, SamusSpritemapDefinition[] definitions)
        : base(PoseFrames(topBases, bottomBases, entries, Validated(definitions).Length), HalvesShown(Validated(definitions)))
    {
        this.definitions = Validated(definitions);
        this.topBases = (ushort[])topBases.Clone();
        this.bottomBases = (ushort[])bottomBases.Clone();
        this.entries = (SamusSpritemapId?[])entries.Clone();
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

    /// <summary>Each pose's first upper-half table entry, in pose-number order.</summary>
    public ReadOnlySpan<ushort> TopBases => topBases;

    /// <summary>Each pose's first lower-half table entry, in pose-number order, including poses sharing the upper/lower composition.</summary>
    public ReadOnlySpan<ushort> BottomBases => bottomBases;

    /// <summary>The complete table in native order, preserving repeated compositions and entries with none.</summary>
    public ReadOnlySpan<SamusSpritemapId?> Entries => entries;

    /// <summary>The table entry at which <paramref name="pose"/>'s upper half begins (native $92:9263).</summary>
    /// <exception cref="ArgumentOutOfRangeException">The pose is outside the 253 supported identities.</exception>
    public ushort TopBase(SamusPoseId pose) => topBases[Number(pose)];

    /// <summary>The table entry at which <paramref name="pose"/>'s lower half begins (native $92:945D).</summary>
    /// <exception cref="ArgumentOutOfRangeException">The pose is outside the 253 supported identities.</exception>
    public ushort BottomBase(SamusPoseId pose) => bottomBases[Number(pose)];

    /// <summary>The table entry <paramref name="frame"/> reads: its pose half's first entry plus its animation frame.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The pose is outside the 253 supported identities.</exception>
    public int EntryIndex(SamusBodyHalfFrame frame) => frame.Half switch
    {
        SamusBodyHalf.Top => TopBase(frame.Pose),
        SamusBodyHalf.Bottom => BottomBase(frame.Pose),
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame.Half, "Samus's body has only a top and a bottom half."),
    } + frame.AnimationFrame;

    /// <summary>The composition the entry at <paramref name="index"/> names; false only for an entry with none, which draws nothing.</summary>
    /// <exception cref="InvalidDataException">The index is outside the table.</exception>
    public bool TryGetEntry(int index, out SamusSpritemapId composition)
    {
        SamusSpritemapId? entry = entries[Checked(index)];
        composition = entry ?? default;
        return entry is not null;
    }

    /// <summary>Whole frames already built, by composition.</summary>
    private readonly System.Collections.Concurrent.ConcurrentDictionary<SamusSpritemapId, SpriteFrame> frames = new();

    /// <summary>
    /// The spritemap at <paramref name="index"/> as a whole frame named by its composition, whose parts' characters
    /// are their sheet characters. Returns false only for an entry with no composition, which draws nothing.
    /// </summary>
    public bool TryGetFrame(int index, [NotNullWhen(true)] out SpriteFrame? frame)
    {
        if (!TryGetEntry(index, out SamusSpritemapId composition))
        {
            frame = null;
            return false;
        }
        frame = frames.GetOrAdd(composition, selected => Frame(selected, definitions[selected.Ordinal]));
        return true;
    }

    /// <summary>
    /// The composition <paramref name="halfFrame"/> shows as a whole frame; false for a frame past the table's end or
    /// one with no composition, which draws nothing.
    /// </summary>
    public bool TryGetFrame(SamusBodyHalfFrame halfFrame, [NotNullWhen(true)] out SpriteFrame? frame)
    {
        if (!Covers(halfFrame))
        {
            frame = null;
            return false;
        }
        return TryGetFrame(EntryIndex(halfFrame), out frame);
    }

    /// <summary>Returns false only for an entry with no composition, which draws nothing.</summary>
    public bool TryGet(int index, [NotNullWhen(true)] out SamusSpritemapDefinition? definition)
    {
        definition = TryGetEntry(index, out SamusSpritemapId composition) ? definitions[composition.Ordinal] : null;
        return definition is not null;
    }

    /// <summary>Requires <paramref name="index"/> to be a table entry.</summary>
    private static int Checked(int index) => index is < 0 or >= EntryCount
        ? throw new InvalidDataException($"Samus spritemap index {index} is outside extracted art.")
        : index;

    /// <summary>The pose's place in the base tables.</summary>
    private static int Number(SamusPoseId pose) => SamusPoseNumbering.HasRecord(pose)
        ? SamusPoseNumbering.Number(pose)
        : throw new ArgumentOutOfRangeException(nameof(pose));

    /// <summary>
    /// Maps the native tables to the composition each pose half shows at each animation frame: frame <c>n</c> of a pose
    /// half shows the entry <c>n</c> places after the half's first, for every frame the body sheets cover that the table
    /// holds. Frames reach past a pose's own program into the entries that follow it, as the cartridge's do.
    /// </summary>
    /// <exception cref="InvalidDataException">Selector dimensions or indices, or entry coverage, are invalid.</exception>
    private static Dictionary<SamusPoseId, SamusPoseSpritemaps> PoseFrames(ushort[] topBases, ushort[] bottomBases,
        SamusSpritemapId?[] entries, int compositionCount)
    {
        Ensure.NotNull(topBases);
        Ensure.NotNull(bottomBases);
        Ensure.NotNull(entries);
        if (topBases.Length != InstalledSamusBodyArtwork.PoseCount ||
            bottomBases.Length != InstalledSamusBodyArtwork.PoseCount ||
            entries.Length != EntryCount)
            throw new InvalidDataException("Samus spritemap selector tables have an invalid length.");
        foreach (int index in topBases.Concat(bottomBases))
            if (index >= EntryCount)
                throw new InvalidDataException($"Samus spritemap base index {index} is outside the table.");
        foreach (SamusSpritemapId? entry in entries)
            if (entry is { } composition && (uint)composition.Ordinal >= (uint)compositionCount)
                throw new InvalidDataException($"Samus spritemap {composition.Ordinal} is absent from installed art.");
        SamusSpritemapId?[] table = (SamusSpritemapId?[])entries.Clone();
        return Enumerable.Range(0, InstalledSamusBodyArtwork.PoseCount).ToDictionary(SamusPoseNumbering.FromNumber,
            pose => new SamusPoseSpritemaps(Run(table, topBases[pose]), Run(table, bottomBases[pose])));
    }

    /// <summary>The entries a pose half's animation frames read, from its first entry to the table's end or the last covered frame.</summary>
    private static ArraySegment<SamusSpritemapId?> Run(SamusSpritemapId?[] table, int first) =>
        new(table, first, Math.Min(AnimationFrames, table.Length - first));

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
