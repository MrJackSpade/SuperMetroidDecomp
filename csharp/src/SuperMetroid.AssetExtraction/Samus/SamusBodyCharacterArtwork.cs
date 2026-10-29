using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Samus.Artwork;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>
/// Samus's body characters: the bank-$92 tile definitions each numbered half picture transfers, bound to the body
/// catalog whose frames select them. The body sheets are composed from these at installation; gameplay draws the sheets
/// and never reads these characters.
/// </summary>
public sealed class SamusBodyCharacterArtwork
{
    /// <summary>Maximum tile count represented by one definition slot.</summary>
    public const int TilesPerDefinition = 16;
    /// <summary>Maximum planar-byte capacity of one definition slot.</summary>
    public const int BytesPerDefinitionSlot = TilesPerDefinition * 32;
    /// <summary>Characters in Samus's body artwork: two sixteen-wide rows.</summary>
    public const int BodyCharacterCount = 2 * SpriteArtwork.CharactersPerRow;
    /// <summary>Characters in one half row, filled by one transfer.</summary>
    private const int HalfRowCharacters = SpriteArtwork.CharactersPerRow / 2;

    /// <summary>Upper-body tile definitions grouped by native pointer-table entry.</summary>
    private readonly SamusBodyTileTransfer[][] top;
    /// <summary>Lower-body tile definitions grouped by native pointer-table entry.</summary>
    private readonly SamusBodyTileTransfer[][] bottom;
    /// <summary>Definition lookup by physical record, including cross-group selections.</summary>
    private readonly Dictionary<int, SamusBodyTileTransfer> definitionsByRecord = [];
    /// <summary>Owned ordinal of each definition in authored order (top sets, then bottom sets); the body artwork identity uses it.</summary>
    private readonly Dictionary<int, int> definitionOrdinals = [];
    /// <summary>Composed body characters per selected pair, built once.</summary>
    private readonly System.Collections.Concurrent.ConcurrentDictionary<SamusBodyArtworkKey, SpriteArtwork> bodyArtwork = new();

    /// <summary>The body catalog whose frames select these characters.</summary>
    public InstalledSamusBodyArtwork Body { get; }

    /// <summary>Binds <paramref name="top"/> and <paramref name="bottom"/> to the frames of <paramref name="body"/>.</summary>
    /// <param name="body">The body catalog whose frames select these characters; <see cref="Create"/> builds one that names them.</param>
    /// <param name="top">Upper-body tile definitions grouped by pointer table entry.</param>
    /// <param name="bottom">Lower-body tile definitions grouped by pointer table entry.</param>
    /// <exception cref="InvalidDataException">A group is missing, malformed or does not fill its set's records.</exception>
    public SamusBodyCharacterArtwork(InstalledSamusBodyArtwork body, SamusBodyTileTransfer[][] top, SamusBodyTileTransfer[][] bottom)
    {
        Body = Ensure.NotNull(body);
        Ensure.NotNull(top);
        Ensure.NotNull(bottom);
        if (top.Length != SamusBodySources.TopSetCount || bottom.Length != SamusBodySources.BottomSetCount)
            throw new InvalidDataException("Samus body selector tables have an invalid length.");
        int[] topStarts = body.Source.TopSetStarts.ToArray(), bottomStarts = body.Source.BottomSetStarts.ToArray();
        this.top = CloneAndValidate(topStarts, top);
        this.bottom = CloneAndValidate(bottomStarts, bottom);
        ValidateCompleteGroups(topStarts, bottomStarts, this.top, this.bottom);
        IndexDefinitions(SamusBodyHalf.Top, this.top);
        IndexDefinitions(SamusBodyHalf.Bottom, this.bottom);
        // The contiguous bank image also contains bytes reached only by a frame
        // counter running past its authored list. Validate a selection when it is
        // actually used, not every four-byte word in the backing ROM interval.
        BindTransfers(SamusBodyHalf.Top, this.top);
        BindTransfers(SamusBodyHalf.Bottom, this.bottom);
        definitionsByRecord.Clear();
        definitionOrdinals.Clear();
        IndexDefinitions(SamusBodyHalf.Top, this.top);
        IndexDefinitions(SamusBodyHalf.Bottom, this.bottom);
    }

    /// <summary>Builds the body catalog from <paramref name="parts"/> naming these characters, and binds the characters to it.</summary>
    /// <param name="parts">The body's tables and catalogs; its characters' identity is calculated from <paramref name="top"/> and <paramref name="bottom"/>.</param>
    /// <param name="top">Upper-body tile definitions grouped by pointer table entry.</param>
    /// <param name="bottom">Lower-body tile definitions grouped by pointer table entry.</param>
    public static SamusBodyCharacterArtwork Create(SamusBodyArtworkParts parts, SamusBodyTileTransfer[][] top, SamusBodyTileTransfer[][] bottom)
    {
        Ensure.NotNull(parts);
        return new(new SamusBodySources(parts, ContentIdentityOf(parts.TopSetStarts, parts.BottomSetStarts, top, bottom)), top, bottom);
    }

    /// <summary>SHA-256 of each numbered half picture's transfer sizes and characters, in record order.</summary>
    /// <param name="topStarts">Each upper-body definition group's first record.</param>
    /// <param name="bottomStarts">Each lower-body definition group's first record.</param>
    /// <param name="top">Upper-body tile definitions grouped by pointer table entry.</param>
    /// <param name="bottom">Lower-body tile definitions grouped by pointer table entry.</param>
    public static string ContentIdentityOf(int[] topStarts, int[] bottomStarts, SamusBodyTileTransfer[][] top, SamusBodyTileTransfer[][] bottom)
    {
        Ensure.NotNull(topStarts);
        Ensure.NotNull(bottomStarts);
        Ensure.NotNull(top);
        Ensure.NotNull(bottom);
        var records = new SortedDictionary<int, SamusBodyTileTransfer>();
        Add(topStarts, top);
        Add(bottomStarts, bottom);
        return SelectedPresentationHash.Create("samus-body-characters-v1", content =>
        {
            foreach ((int record, SamusBodyTileTransfer definition) in records)
            {
                content.Append("definition record", record);
                content.Append("first transfer size", definition.FirstSize);
                content.Append("second transfer size", definition.SecondSize);
                content.Append("characters", definition.Planar.Span);
            }
        });

        void Add(int[] starts, SamusBodyTileTransfer[][] groups)
        {
            if (starts.Length != groups.Length)
                throw new InvalidDataException("Samus body selector tables have an invalid length.");
            for (int set = 0; set < groups.Length; set++)
            {
                SamusBodyTileTransfer[] group = groups[set] ?? throw new InvalidDataException($"Samus body definition set {set} is missing.");
                for (int position = 0; position < group.Length; position++)
                    if (!records.TryAdd(starts[set] + position, group[position]))
                        throw new InvalidDataException($"Samus body definition record {starts[set] + position} is duplicated.");
            }
        }
    }

    /// <summary>Resolves each definition's native transfer geometry against the selected frame tables.</summary>
    private void BindTransfers(SamusBodyHalf half, SamusBodyTileTransfer[][] groups)
    {
        ReadOnlySpan<int> firstFrames = Body.Source.PoseFirstFrames;
        ReadOnlySpan<SamusBodyFrameSelection> frameSnapshot = Body.Source.Frames;
        for (int set = 0; set < groups.Length; set++)
        for (int position = 0; position < groups[set].Length; position++)
            groups[set][position] = groups[set][position].WithTransferGeometry(this, half == SamusBodyHalf.Top, set, position, firstFrames, frameSnapshot);
    }

    /// <summary>Gets an upper-body definition group by pointer-table index.</summary>
    public IReadOnlyList<SamusBodyTileTransfer> TopSet(int set) => top[set];
    /// <summary>Gets a lower-body definition group by pointer-table index.</summary>
    public IReadOnlyList<SamusBodyTileTransfer> BottomSet(int set) => bottom[set];

    /// <summary>
    /// Samus's 32 body characters as the selected definitions make them visible: each definition's first transfer
    /// fills one eight-character half row (top: characters 0-7, bottom: 8-15) and its second transfer the half row
    /// below it (top: 16-23, bottom: 24-31). A half with no selected picture is blank.
    /// </summary>
    /// <param name="topPicture">The selected top half picture, or null.</param>
    /// <param name="bottomPicture">The selected bottom half picture, or null.</param>
    public SpriteArtwork BodyArtwork(SamusBodyPicture? topPicture, SamusBodyPicture? bottomPicture)
    {
        int? topRecord = topPicture is { } shownTop ? SamusBodySources.Record(shownTop) : null;
        int? bottomRecord = bottomPicture is { } shownBottom ? SamusBodySources.Record(shownBottom) : null;
        int topOrdinal = topRecord is { } upper ? definitionOrdinals[upper] : -1;
        int bottomOrdinal = bottomRecord is { } lower ? definitionOrdinals[lower] : -1;
        return bodyArtwork.GetOrAdd(new SamusBodyArtworkKey(topOrdinal, bottomOrdinal), key =>
        {
            var characters = new byte[BodyCharacterCount * SpriteArtwork.CharacterByteCount];
            if (topRecord is { } upperRecord)
                CopyHalves(DefinitionAt(upperRecord), characters, 0);
            if (bottomRecord is { } lowerRecord)
                CopyHalves(DefinitionAt(lowerRecord), characters, HalfRowCharacters);
            return new SpriteArtwork(new SpriteArtworkId(SpriteArtworkKind.SamusBody, key.SpriteVariant), characters);
        });

        static void CopyHalves(SamusBodyTileTransfer definition, byte[] characters, int firstCharacter)
        {
            ReadOnlySpan<byte> planar = definition.Planar.Span;
            planar[..definition.FirstSize].CopyTo(characters.AsSpan(firstCharacter * SpriteArtwork.CharacterByteCount));
            planar[definition.FirstSize..].CopyTo(characters.AsSpan(
                (firstCharacter + SpriteArtwork.CharactersPerRow) * SpriteArtwork.CharacterByteCount));
        }
    }

    /// <summary>Resolve a saved definition record after a debugger-state rebind.</summary>
    public SamusBodyTileTransfer DefinitionAt(int record)
    {
        if (!definitionsByRecord.TryGetValue(record, out SamusBodyTileTransfer? definition))
            throw new InvalidDataException($"Samus body definition record {record} is absent from installed art.");
        return definition;
    }

    /// <summary>Indexes each tile definition at its physical record.</summary>
    private void IndexDefinitions(SamusBodyHalf half, SamusBodyTileTransfer[][] groups)
    {
        ReadOnlySpan<int> starts = half == SamusBodyHalf.Top ? Body.Source.TopSetStarts : Body.Source.BottomSetStarts;
        for (int set = 0; set < groups.Length; set++)
        for (int position = 0; position < groups[set].Length; position++)
        {
            int record = starts[set] + position;
            if (!definitionsByRecord.TryAdd(record, groups[set][position]))
                throw new InvalidDataException($"Samus body definition record {record} is duplicated.");
            definitionOrdinals.Add(record, definitionOrdinals.Count);
        }
    }

    /// <summary>Clones authored groups and validates each transfer slot against native capacity.</summary>
    private static SamusBodyTileTransfer[][] CloneAndValidate(int[] starts, SamusBodyTileTransfer[][] groups)
    {
        var result = new SamusBodyTileTransfer[groups.Length][];
        for (int set = 0; set < groups.Length; set++)
        {
            SamusBodyTileTransfer[] source = groups[set] ??
                throw new InvalidDataException($"Samus body definition set {set} is missing.");
            result[set] = (SamusBodyTileTransfer[])source.Clone();
            if (source.Length == 0 || starts[set] < 0)
                throw new InvalidDataException($"Samus body definition set {set} is malformed.");
            foreach (SamusBodyTileTransfer definition in source)
            {
                if (definition is null || definition.FirstSize == 0 ||
                    definition.FirstSize + definition.SecondSize > BytesPerDefinitionSlot ||
                    (definition.FirstSize + definition.SecondSize) % 32 != 0 ||
                    definition.Planar.Length != definition.FirstSize + definition.SecondSize)
                    throw new InvalidDataException($"Samus body definition set {set} has invalid tile data.");
            }
        }
        return result;
    }

    /// <summary>Requires each set to hold exactly the records between its start and the next start in sorted order.</summary>
    private static void ValidateCompleteGroups(int[] topStarts, int[] bottomStarts,
        SamusBodyTileTransfer[][] top, SamusBodyTileTransfer[][] bottom)
    {
        IReadOnlyDictionary<int, int> counts = SamusBodySources.GroupLengths(topStarts, bottomStarts);
        ValidateHalf(topStarts, top);
        ValidateHalf(bottomStarts, bottom);

        void ValidateHalf(int[] starts, SamusBodyTileTransfer[][] groups)
        {
            for (int set = 0; set < starts.Length; set++)
                if (groups[set].Length != counts[starts[set]])
                    throw new InvalidDataException(
                        $"Samus definition group at record {starts[set]} requires {counts[starts[set]]} physical records, not {groups[set].Length}.");
        }
    }
}
