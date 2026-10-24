using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Samus.Drawing;

namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>Editable bank-$92 Samus body characters and their native frame/half selectors.</summary>
/// <remarks>
/// These records affect presentation only. Pose transitions, animation delays, collision,
/// and projectile timing remain in the compiled simulation. A selected definition is named by its record, so the
/// debugger can still identify the original seven-byte DMA record.
/// </remarks>
public sealed class SamusBodyArtworkCatalog
{
    /// <summary>Number of four-byte frame selectors, stored one after another.</summary>
    public const int FrameCount = 1143;
    /// <summary>Number of native Samus pose entries with body-art selectors.</summary>
    public const int PoseCount = 253;
    /// <summary>Number of upper-body definition groups.</summary>
    public const int TopSetCount = 13;
    /// <summary>Number of lower-body definition groups.</summary>
    public const int BottomSetCount = 11;
    /// <summary>Maximum tile count represented by one definition slot.</summary>
    public const int TilesPerDefinition = 16;
    /// <summary>Maximum planar-byte capacity of one definition slot.</summary>
    public const int BytesPerDefinitionSlot = TilesPerDefinition * 32;

    /// <summary>Edited upper-body set starts keyed by native set index.</summary>
    internal readonly Dictionary<int, int> topSetStarts;
    /// <summary>Edited lower-body set starts keyed by native set index.</summary>
    internal readonly Dictionary<int, int> bottomSetStarts;
    /// <summary>Edited per-pose first frames, with stock frames calculated on demand.</summary>
    internal readonly Dictionary<int, int> poseFirstFrames;
    /// <summary>Pose art-origin edits independent of physical collision offsets.</summary>
    internal readonly Dictionary<int, sbyte> graphicsYOffsets;
    /// <summary>Edited landing visual bytes, including an adjacent unaligned-read byte.</summary>
    internal readonly Dictionary<int, ushort> landingYOffsets;
    /// <summary>Edited posture-transition art offsets.</summary>
    internal readonly Dictionary<int, sbyte> postureYOffsets;
    /// <summary>Edited drained-state art offsets.</summary>
    internal readonly Dictionary<int, sbyte> drainedYOffsets;
    /// <summary>Edited components of four-byte native frame selectors.</summary>
    internal readonly Dictionary<int, byte> frames;
    /// <summary>Characters in Samus's body artwork: two sixteen-wide rows.</summary>
    public const int BodyCharacterCount = 2 * SpriteArtwork.CharactersPerRow;
    /// <summary>Characters in one half row, filled by one transfer.</summary>
    private const int HalfRowCharacters = SpriteArtwork.CharactersPerRow / 2;
    /// <summary>Upper-body tile definitions grouped by native pointer-table entry.</summary>
    private readonly SamusBodyTileDefinition[][] top;
    /// <summary>Lower-body tile definitions grouped by native pointer-table entry.</summary>
    private readonly SamusBodyTileDefinition[][] bottom;
    /// <summary>Definition lookup by physical record, including cross-group selections.</summary>
    private readonly Dictionary<int, SamusBodyTileDefinition> definitionsByRecord = [];
    /// <summary>Owned ordinal of each definition in authored order (top sets, then bottom sets); the body artwork identity uses it.</summary>
    private readonly Dictionary<int, int> definitionOrdinals = [];
    /// <summary>Composed body characters per selected pair, built once.</summary>
    private readonly System.Collections.Concurrent.ConcurrentDictionary<SamusBodyArtworkKey, SpriteArtwork> bodyArtwork = new();

    /// <summary>Editable bank-$92 OAM composition for these body frames.</summary>
    public SamusSpritemapArtworkCatalog Spritemaps { get; }

    /// <summary>Editable direct small-OBJ attributes for Samus's atmospheric effects.</summary>
    public SamusAtmosphericArtworkCatalog Atmosphere { get; }

    /// <summary>Editable fatal-damage suit, suitless, and whiteout colors.</summary>
    public SamusDeathPaletteArtworkCatalog DeathPalettes { get; }

    /// <summary>Editable tile characters uploaded during the five death-explosion phases.</summary>
    public SamusDeathTileAtlas DeathTiles { get; }

    /// <summary>Editable pose placement and cover characters for the independent arm-cannon OBJ.</summary>
    public SamusArmCannonArtworkCatalog ArmCannon { get; }

    /// <summary>Creates and validates a complete editable Samus body-art catalog.</summary>
    /// <param name="parts">Samus's body pointer tables, frame tiles, offsets and dependent artwork catalogs (#633).</param>
    public SamusBodyArtworkCatalog(SamusBodyArtworkParts parts)
    {
        Ensure.NotNull(parts.TopSetStarts);
        Ensure.NotNull(parts.BottomSetStarts);
        Ensure.NotNull(parts.PoseFirstFrames);
        Ensure.NotNull(parts.GraphicsYOffsets);
        Ensure.NotNull(parts.Frames);
        Ensure.NotNull(parts.Top);
        Ensure.NotNull(parts.Bottom);
        Ensure.NotNull(parts.Spritemaps);
        Ensure.NotNull(parts.Atmosphere);
        Ensure.NotNull(parts.DeathPalettes);
        Ensure.NotNull(parts.DeathTiles);
        Ensure.NotNull(parts.ArmCannon);
        Ensure.NotNull(parts.LandingYOffsets);
        Ensure.NotNull(parts.PostureYOffsets);
        Ensure.NotNull(parts.DrainedYOffsets);
        if (parts.TopSetStarts.Length != TopSetCount || parts.BottomSetStarts.Length != BottomSetCount ||
            parts.PoseFirstFrames.Length != PoseCount || parts.GraphicsYOffsets.Length != PoseCount ||
            parts.LandingYOffsets.Length != SamusDrawingDefinitions.BodyOffsets.LandingVerticalOffsetByteCount ||
            parts.PostureYOffsets.Length != SamusDrawingDefinitions.BodyOffsets.PostureTransitionVerticalOffsetByteCount ||
            parts.DrainedYOffsets.Length != SamusDrawingDefinitions.BodyOffsets.DrainedVerticalOffsetByteCount ||
            parts.Frames.Length != FrameCount ||
            parts.Top.Length != TopSetCount || parts.Bottom.Length != BottomSetCount)
            throw new InvalidDataException("Samus body selector tables have an invalid length.");

        this.topSetStarts = Enumerable.Range(0, parts.TopSetStarts.Length)
            .Where(index => parts.TopSetStarts[index] != SamusBodyDefinitionLayout.DefaultTopStart(index))
            .ToDictionary(index => index, index => parts.TopSetStarts[index]);
        this.bottomSetStarts = Enumerable.Range(0, parts.BottomSetStarts.Length)
            .Where(index => parts.BottomSetStarts[index] != SamusBodyDefinitionLayout.DefaultBottomStart(index))
            .ToDictionary(index => index, index => parts.BottomSetStarts[index]);
        this.poseFirstFrames = Enumerable.Range(0, parts.PoseFirstFrames.Length)
            .Where(pose => parts.PoseFirstFrames[pose] != SamusBodyPoseDefinitions.DefaultFirstFrame((SamusPoseId)pose))
            .ToDictionary(pose => pose, pose => parts.PoseFirstFrames[pose]);
        this.graphicsYOffsets = Enumerable.Range(0, parts.GraphicsYOffsets.Length)
            .Where(index => parts.GraphicsYOffsets[index] != SamusBodyPlacementDefinitions.DefaultGraphicsYOffset((SamusPoseId)index))
            .ToDictionary(index => index, index => parts.GraphicsYOffsets[index]);
        if (parts.LandingYOffsets.Any(value => value > byte.MaxValue))
            throw new InvalidDataException("Samus landing visual bytes must fit in one byte.");
        this.landingYOffsets = Enumerable.Range(0, parts.LandingYOffsets.Length)
            .Where(index => parts.LandingYOffsets[index] != (SamusBodyPlacementDefinitions.LandingSourceIndex(index) == index
                ? SamusBodyPlacementDefinitions.DefaultLandingByte(index)
                : parts.LandingYOffsets[SamusBodyPlacementDefinitions.LandingSourceIndex(index)]))
            .ToDictionary(index => index, index => parts.LandingYOffsets[index]);

        if (parts.Frames.Any(frame => !IsSelectorComponent(frame.TopSet) || !IsSelectorComponent(frame.TopPosition) ||
                !IsSelectorComponent(frame.BottomSet) || !IsSelectorComponent(frame.BottomPosition)))
            throw new InvalidDataException("Samus body frame selector components must be table indices from 0 through 255.");
        byte[] components = parts.Frames.SelectMany(frame => new[] { (byte)frame.TopSet, (byte)frame.TopPosition,
            (byte)frame.BottomSet, (byte)frame.BottomPosition }).ToArray();
        this.frames = Enumerable.Range(0, components.Length)
            .Where(index => SamusBodyFrameDefinitions.SourceComponent(index) != index
                ? components[index] != components[SamusBodyFrameDefinitions.SourceComponent(index)]
                : !SamusBodyFrameDefinitions.TryComponent(index, out byte calculated) || components[index] != calculated)
            .ToDictionary(index => index, index => components[index]);
        Spritemaps = parts.Spritemaps;
        Atmosphere = parts.Atmosphere;
        DeathPalettes = parts.DeathPalettes;
        DeathTiles = parts.DeathTiles;
        ArmCannon = parts.ArmCannon.WithBodyGeometry(this);
        this.top = CloneAndValidate(parts.TopSetStarts, parts.Top);
        this.bottom = CloneAndValidate(parts.BottomSetStarts, parts.Bottom);
        SamusBodyDefinitionLayout.ValidateCompleteGroups(parts.TopSetStarts, parts.BottomSetStarts, this.top, this.bottom);
        IndexDefinitions(SamusBodyHalf.Top, this.top);
        IndexDefinitions(SamusBodyHalf.Bottom, this.bottom);
        this.postureYOffsets = Enumerable.Range(0, parts.PostureYOffsets.Length)
            .Where(index => SamusBodyPlacementDefinitions.PostureSourceIndex(index) == index
                ? !SamusBodyPlacementDefinitions.TryDefaultPostureByte(this, index, out sbyte calculated) || parts.PostureYOffsets[index] != calculated
                : parts.PostureYOffsets[index] != parts.PostureYOffsets[SamusBodyPlacementDefinitions.PostureSourceIndex(index)])
            .ToDictionary(index => index, index => parts.PostureYOffsets[index]);
        this.drainedYOffsets = Enumerable.Range(0, parts.DrainedYOffsets.Length)
            .Where(index => !SamusBodyPlacementDefinitions.TryDefaultDrainedByte(this, index, out sbyte calculated) || parts.DrainedYOffsets[index] != calculated)
            .ToDictionary(index => index, index => parts.DrainedYOffsets[index]);
        foreach (int firstFrame in parts.PoseFirstFrames)
            if ((uint)firstFrame >= FrameCount)
                throw new InvalidDataException($"Samus pose selects invalid frame list {firstFrame}.");
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

    /// <summary>Resolves each definition's native transfer geometry against the selected frame tables.</summary>
    private void BindTransfers(SamusBodyHalf half, SamusBodyTileDefinition[][] groups)
    {
        ReadOnlySpan<int> firstFrames = PoseFirstFrames;
        ReadOnlySpan<SamusBodyFrameSelection> frameSnapshot = Frames;
        for (int set = 0; set < groups.Length; set++)
        for (int position = 0; position < groups[set].Length; position++)
            groups[set][position] = groups[set][position].WithTransferGeometry(this, half == SamusBodyHalf.Top, set, position, firstFrames, frameSnapshot);
    }

    /// <summary>SHA-256 of selected body art, all visual selectors and every bundled Samus catalog.</summary>
    public string ContentIdentity => CreateContentIdentity();

    /// <summary>Gets each upper-body definition group's resolved first record.</summary>
    public ReadOnlySpan<int> TopSetStarts => Enumerable.Range(0, TopSetCount).Select(index => SetStart(true, index)).ToArray();
    /// <summary>Gets each lower-body definition group's resolved first record.</summary>
    public ReadOnlySpan<int> BottomSetStarts => Enumerable.Range(0, BottomSetCount).Select(index => SetStart(false, index)).ToArray();
    /// <summary>Gets each pose's resolved first frame.</summary>
    public ReadOnlySpan<int> PoseFirstFrames => Enumerable.Range(0, PoseCount).Select(pose => PoseFirstFrame((SamusPoseId)pose)).ToArray();
    /// <summary>Gets the resolved signed art-origin offset for every pose.</summary>
    public ReadOnlySpan<sbyte> GraphicsYOffsets => Enumerable.Range(0, PoseCount).Select(index => GraphicsYOffset((SamusPoseId)index)).ToArray();
    /// <summary>Native landing table, including the one adjacent byte read by an unaligned word.</summary>
    public ReadOnlySpan<ushort> LandingYOffsets => Enumerable.Range(0, SamusDrawingDefinitions.BodyOffsets.LandingVerticalOffsetByteCount).Select(LandingByte).ToArray();
    /// <summary>Gets the resolved posture-transition vertical-offset table.</summary>
    public ReadOnlySpan<sbyte> PostureYOffsets => Enumerable.Range(0, SamusDrawingDefinitions.BodyOffsets.PostureTransitionVerticalOffsetByteCount).Select(PostureByte).ToArray();
    /// <summary>Gets the resolved drained-state vertical-offset table.</summary>
    public ReadOnlySpan<sbyte> DrainedYOffsets => Enumerable.Range(0, SamusDrawingDefinitions.BodyOffsets.DrainedVerticalOffsetByteCount).Select(DrainedByte).ToArray();
    /// <summary>Resolves an edited landing byte or its native shared-source alias.</summary>
    private ushort LandingByte(int index) => landingYOffsets.TryGetValue(index, out ushort value)
        ? value : SamusBodyPlacementDefinitions.LandingSourceIndex(index) == index
            ? SamusBodyPlacementDefinitions.DefaultLandingByte(index) : LandingByte(SamusBodyPlacementDefinitions.LandingSourceIndex(index));
    /// <summary>Resolves an edited posture offset, following native aliases when needed.</summary>
    private sbyte PostureByte(int index) => postureYOffsets.TryGetValue(index, out sbyte value)
        ? value : SamusBodyPlacementDefinitions.PostureSourceIndex(index) != index
            ? PostureByte(SamusBodyPlacementDefinitions.PostureSourceIndex(index))
            : SamusBodyPlacementDefinitions.TryDefaultPostureByte(this, index, out sbyte calculated)
                ? calculated : throw new InvalidDataException("Selected posture geometry no longer supplies its installed offset.");

    /// <summary>Resolves a drained-state offset from edits or the selected art geometry.</summary>
    private sbyte DrainedByte(int index) => drainedYOffsets.TryGetValue(index, out sbyte value)
        ? value : SamusBodyPlacementDefinitions.TryDefaultDrainedByte(this, index, out sbyte calculated)
            ? calculated : throw new InvalidDataException("Selected drained geometry no longer supplies its installed offset.");
    /// <summary>Attempts to read the native unaligned landing-offset word at a byte index.</summary>
    /// <param name="index">The starting byte index.</param>
    /// <param name="value">Receives the little-endian landing-offset word.</param>
    /// <returns><see langword="true"/> when two bytes are available.</returns>
    public bool TryLandingYOffset(int index, out ushort value)
    {
        if ((uint)index >= SamusDrawingDefinitions.BodyOffsets.LandingVerticalOffsetByteCount - 1)
        {
            value = 0;
            return false;
        }
        value = (ushort)(LandingByte(index) | LandingByte(index + 1) << 8);
        return true;
    }
    /// <summary>Attempts to read a signed posture-transition art offset.</summary>
    /// <param name="index">The table byte index.</param>
    /// <param name="value">Receives the signed vertical offset.</param>
    /// <returns><see langword="true"/> when the index is in range.</returns>
    public bool TryPostureYOffset(int index, out sbyte value)
    {
        if ((uint)index >= SamusDrawingDefinitions.BodyOffsets.PostureTransitionVerticalOffsetByteCount) { value = 0; return false; }
        value = PostureByte(index);
        return true;
    }
    /// <summary>Attempts to read a signed drained-state art offset.</summary>
    /// <param name="index">The table byte index.</param>
    /// <param name="value">Receives the signed vertical offset.</param>
    /// <returns><see langword="true"/> when the index is in range.</returns>
    public bool TryDrainedYOffset(int index, out sbyte value)
    {
        if ((uint)index >= SamusDrawingDefinitions.BodyOffsets.DrainedVerticalOffsetByteCount) { value = 0; return false; }
        value = DrainedByte(index);
        return true;
    }
    /// <summary>Signed pose art origin; changing it never changes a physical projectile origin.</summary>
    public sbyte GraphicsYOffset(SamusPoseId pose) =>
        (int)pose < PoseCount ? graphicsYOffsets.TryGetValue((int)pose, out sbyte value) ? value : SamusBodyPlacementDefinitions.DefaultGraphicsYOffset(pose) :
            throw new InvalidDataException($"Pose ${(int)pose:X2} has no authored graphics Y offset.");
    /// <summary>Gets the resolved contiguous bank-$92 frame-selector interval.</summary>
    public ReadOnlySpan<SamusBodyFrameSelection> Frames => Enumerable.Range(0, FrameCount).Select(FrameAt).ToArray();
    /// <summary>Resolves a frame-selector byte through edited values, aliases, or compiled defaults.</summary>
    private byte FrameComponent(int index) => frames.TryGetValue(index, out byte value)
        ? value : SamusBodyFrameDefinitions.SourceComponent(index) != index
            ? FrameComponent(SamusBodyFrameDefinitions.SourceComponent(index))
            : SamusBodyFrameDefinitions.TryComponent(index, out byte calculated) ? calculated
                : throw new InvalidDataException("Installed body frame has no selected component.");
    /// <summary>Assembles one native four-byte upper/lower body selector.</summary>
    private SamusBodyFrameSelection FrameAt(int index) => new(FrameComponent(index * 4), FrameComponent(index * 4 + 1),
        FrameComponent(index * 4 + 2), FrameComponent(index * 4 + 3));
    /// <summary>Gets an upper-body definition group by pointer-table index.</summary>
    public IReadOnlyList<SamusBodyTileDefinition> TopSet(int set) => top[set];
    /// <summary>Gets a lower-body definition group by pointer-table index.</summary>
    public IReadOnlyList<SamusBodyTileDefinition> BottomSet(int set) => bottom[set];

    /// <summary>Resolves an edited pose's first frame or the compiled stock one.</summary>
    private int PoseFirstFrame(SamusPoseId pose) => poseFirstFrames.TryGetValue((int)pose, out int value)
        ? value : SamusBodyPoseDefinitions.DefaultFirstFrame(pose);

    /// <summary>The frame <paramref name="animationFrame"/> places after the pose's first.</summary>
    public SamusBodyFrameSelection Frame(SamusPoseId pose, int animationFrame)
    {
        if ((int)pose >= PoseCount)
            throw new InvalidDataException($"Pose ${(int)pose:X2} has no authored Samus body frame list.");
        int index = PoseFirstFrame(pose) + animationFrame;
        if ((uint)index >= FrameCount)
            throw new InvalidDataException($"Samus frame {index} is outside extracted visual selectors.");
        SamusBodyFrameSelection frame = FrameAt(index);
        GetDefinition(true, frame.TopSet, frame.TopPosition);
        if (frame.NamesBottom)
            GetDefinition(false, frame.BottomSet, frame.BottomPosition);
        return frame;
    }

    /// <summary>Resolves the selected upper or lower definition group's first record.</summary>
    private int SetStart(bool upperHalf, int set)
    {
        Dictionary<int, int> overrides = upperHalf ? topSetStarts : bottomSetStarts;
        return overrides.TryGetValue(set, out int value) ? value : upperHalf
            ? SamusBodyDefinitionLayout.DefaultTopStart(set) : SamusBodyDefinitionLayout.DefaultBottomStart(set);
    }
    /// <summary>Whether a frame-selector component is a table index the selector can hold (0 through 255).</summary>
    private static bool IsSelectorComponent(int component) => component is >= 0 and <= byte.MaxValue;

    /// <summary>Resolves a native set and position to its physical body tile definition.</summary>
    public SamusBodyTileDefinition GetDefinition(bool upperHalf, int set, int position)
    {
        if (set < 0 || set >= (upperHalf ? TopSetCount : BottomSetCount) || !IsSelectorComponent(position))
            throw new InvalidDataException(
                $"Samus {(upperHalf ? "top" : "bottom")} definition {set:X2}/{position:X2} is absent.");
        // The native selector steps position records past the chosen set's first without a
        // per-set bounds check. Several authored frames intentionally land in the
        // next definition group; resolve the physical record, not a C# jagged index.
        return DefinitionAt(SetStart(upperHalf, set) + position);
    }

    /// <summary>
    /// The frame <paramref name="animationFrame"/> places after the pose's first, when it lies in the frame table and both
    /// of its definitions are installed; false for any other pair, which <see cref="Frame"/> rejects.
    /// </summary>
    public bool TryFrame(SamusPoseId pose, int animationFrame, out SamusBodyFrameSelection frame)
    {
        frame = default;
        int index = (int)pose < PoseCount ? PoseFirstFrame(pose) + animationFrame : -1;
        if ((uint)index >= FrameCount)
            return false;
        frame = FrameAt(index);
        return HasDefinition(SamusBodyHalf.Top, frame.TopSet, frame.TopPosition) &&
            (!frame.NamesBottom || HasDefinition(SamusBodyHalf.Bottom, frame.BottomSet, frame.BottomPosition));
    }

    /// <summary>Whether a native set and position resolve to an installed definition.</summary>
    private bool HasDefinition(SamusBodyHalf half, int set, int position) =>
        set >= 0 && set < (half == SamusBodyHalf.Top ? TopSetCount : BottomSetCount) && IsSelectorComponent(position) &&
        definitionsByRecord.ContainsKey(SetStart(half == SamusBodyHalf.Top, set) + position);

    /// <summary>Returns the record selected by a native body set and position.</summary>
    public int DefinitionRecord(SamusBodyHalf half, int set, int position)
    {
        _ = GetDefinition(half == SamusBodyHalf.Top, set, position);
        return SetStart(half == SamusBodyHalf.Top, set) + position;
    }

    /// <summary>
    /// Samus's 32 body characters as the selected definitions make them visible: each definition's first transfer
    /// fills one eight-character half row (top: characters 0-7, bottom: 8-15) and its second transfer the half row
    /// below it (top: 16-23, bottom: 24-31). A half with no selected definition is blank.
    /// </summary>
    /// <param name="topRecord">The selected top definition's record, or null.</param>
    /// <param name="bottomRecord">The selected bottom definition's record, or null.</param>
    public SpriteArtwork BodyArtwork(int? topRecord, int? bottomRecord)
    {
        int top = topRecord is { } upper ? definitionOrdinals[upper] : -1;
        int bottom = bottomRecord is { } lower ? definitionOrdinals[lower] : -1;
        return bodyArtwork.GetOrAdd(new SamusBodyArtworkKey(top, bottom), key =>
        {
            var characters = new byte[BodyCharacterCount * SpriteArtwork.CharacterByteCount];
            if (topRecord is { } upperRecord)
                CopyHalves(DefinitionAt(upperRecord), characters, 0);
            if (bottomRecord is { } lowerRecord)
                CopyHalves(DefinitionAt(lowerRecord), characters, HalfRowCharacters);
            return new SpriteArtwork(new SpriteArtworkId(SpriteArtworkKind.SamusBody, (key.Top + 1) << 16 | (key.Bottom + 1)), characters);
        });

        static void CopyHalves(SamusBodyTileDefinition definition, byte[] characters, int firstCharacter)
        {
            ReadOnlySpan<byte> planar = definition.Planar.Span;
            planar[..definition.FirstSize].CopyTo(characters.AsSpan(firstCharacter * SpriteArtwork.CharacterByteCount));
            planar[definition.FirstSize..].CopyTo(characters.AsSpan(
                (firstCharacter + SpriteArtwork.CharactersPerRow) * SpriteArtwork.CharacterByteCount));
        }
    }

    /// <summary>Resolve a saved definition record after a debugger-state rebind.</summary>
    public SamusBodyTileDefinition DefinitionAt(int record)
    {
        if (!definitionsByRecord.TryGetValue(record, out SamusBodyTileDefinition? definition))
            throw new InvalidDataException($"Samus body definition record {record} is absent from installed art.");
        return definition;
    }

    /// <summary>Indexes each tile definition at its physical record.</summary>
    private void IndexDefinitions(SamusBodyHalf half, SamusBodyTileDefinition[][] groups)
    {
        for (int set = 0; set < groups.Length; set++)
        for (int position = 0; position < groups[set].Length; position++)
        {
            int record = SetStart(half == SamusBodyHalf.Top, set) + position;
            if (!definitionsByRecord.TryAdd(record, groups[set][position]))
                throw new InvalidDataException($"Samus body definition record {record} is duplicated.");
            definitionOrdinals.Add(record, definitionOrdinals.Count);
        }
    }

    /// <summary>Clones authored groups and validates each transfer slot against native capacity.</summary>
    private static SamusBodyTileDefinition[][] CloneAndValidate(
        int[] starts, SamusBodyTileDefinition[][] groups)
    {
        var result = new SamusBodyTileDefinition[groups.Length][];
        for (int set = 0; set < groups.Length; set++)
        {
            SamusBodyTileDefinition[] source = groups[set] ??
                throw new InvalidDataException($"Samus body definition set {set} is missing.");
            result[set] = (SamusBodyTileDefinition[])source.Clone();
            if (source.Length == 0 || starts[set] < 0)
                throw new InvalidDataException($"Samus body definition set {set} is malformed.");
            foreach (SamusBodyTileDefinition definition in source)
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

    /// <summary>Hashes selected body bytes, selectors, and bundled Samus presentation catalogs.</summary>
    private string CreateContentIdentity() => SelectedPresentationHash.Create("samus-body-v2", content =>
    {
        foreach (int start in TopSetStarts) content.Append("top set start", start);
        foreach (int start in BottomSetStarts) content.Append("bottom set start", start);
        foreach (int firstFrame in PoseFirstFrames) content.Append("pose first frame", firstFrame);
        content.Append("graphics y offsets", GraphicsYOffsets.SelectToArray(value => unchecked((byte)value)));
        content.AppendWords("landing y offsets", LandingYOffsets);
        content.Append("posture y offsets", PostureYOffsets.SelectToArray(value => unchecked((byte)value)));
        content.Append("drained y offsets", DrainedYOffsets.SelectToArray(value => unchecked((byte)value)));
        foreach (SamusBodyFrameSelection frame in Frames)
        {
            content.Append("top set", frame.TopSet);
            content.Append("top position", frame.TopPosition);
            content.Append("bottom set", frame.BottomSet);
            content.Append("bottom position", frame.BottomPosition);
        }
        foreach ((int record, SamusBodyTileDefinition definition) in definitionsByRecord.OrderBy(pair => pair.Key))
        {
            content.Append("definition record", record);
            content.Append("source address", definition.SourceAddress);
            content.Append("first transfer size", definition.FirstSize);
            content.Append("second transfer size", definition.SecondSize);
            content.Append("characters", definition.Planar.Span);
        }
        content.Append("spritemaps", Convert.FromHexString(Spritemaps.ContentIdentity));
        content.Append("atmosphere", Convert.FromHexString(Atmosphere.ContentIdentity));
        content.Append("death palettes", Convert.FromHexString(DeathPalettes.ContentIdentity));
        content.Append("death tiles", Convert.FromHexString(DeathTiles.ContentIdentity));
        content.Append("arm cannon", Convert.FromHexString(ArmCannon.ContentIdentity));
    });
}
