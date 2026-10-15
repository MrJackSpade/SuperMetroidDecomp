using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Samus.Drawing;

namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>Editable bank-$92 Samus body characters and their native frame/half selectors.</summary>
/// <remarks>
/// These records affect presentation only. Pose transitions, animation delays, collision,
/// and projectile timing remain in the compiled simulation. Definition addresses are kept
/// so the debugger can still identify the original seven-byte DMA record.
/// </remarks>
public sealed class SamusBodyArtworkCatalog
{
    /// <summary>SNES address of the first four-byte Samus frame selector.</summary>
    public const int FirstFrameAddress = 0x92DB48;
    /// <summary>Exclusive SNES address ending the extracted frame-selector interval.</summary>
    public const int FrameEndExclusive = 0x92ED24;
    /// <summary>Bank-$92 offset of the first four-byte frame selector.</summary>
    public const int FirstFrameOffset = 0xDB48;
    /// <summary>Exclusive bank-$92 offset ending the frame-selector interval.</summary>
    public const int FrameEndOffset = 0xED24;
    /// <summary>Number of four-byte frame selectors in the contiguous interval.</summary>
    public const int FrameCount = (FrameEndExclusive - FirstFrameAddress) / 4;
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

    /// <summary>Edited upper-body set pointers keyed by native set index.</summary>
    internal readonly Dictionary<int, ushort> topPointers;
    /// <summary>Edited lower-body set pointers keyed by native set index.</summary>
    internal readonly Dictionary<int, ushort> bottomPointers;
    /// <summary>Edited per-pose frame-list pointers, with stock pointers calculated on demand.</summary>
    internal readonly Dictionary<int, ushort> posePointers;
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
    /// <summary>Definition lookup by physical SNES address, including cross-group selections.</summary>
    private readonly Dictionary<int, SamusBodyTileDefinition> definitionsByAddress = [];
    /// <summary>Owned ordinal of each definition in authored order (top sets, then bottom sets); the body artwork identity uses it.</summary>
    private readonly Dictionary<int, int> definitionOrdinals = [];
    /// <summary>Composed body characters per selected pair, built once.</summary>
    private readonly System.Collections.Concurrent.ConcurrentDictionary<(int Top, int Bottom), SpriteArtwork> bodyArtwork = new();

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
        Ensure.NotNull(parts.TopPointers);
        Ensure.NotNull(parts.BottomPointers);
        Ensure.NotNull(parts.PosePointers);
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
        if (parts.TopPointers.Length != TopSetCount || parts.BottomPointers.Length != BottomSetCount ||
            parts.PosePointers.Length != PoseCount || parts.GraphicsYOffsets.Length != PoseCount ||
            parts.LandingYOffsets.Length != SamusRenderingRomData.Body.LandingVerticalOffsetByteCount ||
            parts.PostureYOffsets.Length != SamusRenderingRomData.Body.PostureTransitionVerticalOffsetByteCount ||
            parts.DrainedYOffsets.Length != SamusRenderingRomData.Body.DrainedVerticalOffsetByteCount ||
            parts.Frames.Length != FrameCount ||
            parts.Top.Length != TopSetCount || parts.Bottom.Length != BottomSetCount)
            throw new InvalidDataException("Samus body selector tables have an invalid length.");

        this.topPointers = Enumerable.Range(0, parts.TopPointers.Length)
            .Where(index => parts.TopPointers[index] != SamusBodyDefinitionLayout.DefaultTopPointer(index))
            .ToDictionary(index => index, index => parts.TopPointers[index]);
        this.bottomPointers = Enumerable.Range(0, parts.BottomPointers.Length)
            .Where(index => parts.BottomPointers[index] != SamusBodyDefinitionLayout.DefaultBottomPointer(index))
            .ToDictionary(index => index, index => parts.BottomPointers[index]);
        this.posePointers = Enumerable.Range(0, parts.PosePointers.Length)
            .Where(pose => parts.PosePointers[pose] != SamusBodyPoseDefinitions.DefaultFrameList((SamusPoseId)pose))
            .ToDictionary(pose => pose, pose => parts.PosePointers[pose]);
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

        byte[] components = parts.Frames.SelectMany(frame => new byte[] { frame.TopSet, frame.TopPosition, frame.BottomSet, frame.BottomPosition }).ToArray();
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
        this.top = CloneAndValidate(parts.TopPointers, parts.Top);
        this.bottom = CloneAndValidate(parts.BottomPointers, parts.Bottom);
        SamusBodyDefinitionLayout.ValidateCompleteGroups(parts.TopPointers, parts.BottomPointers, this.top, this.bottom);
        IndexDefinitions(true, this.top);
        IndexDefinitions(false, this.bottom);
        this.postureYOffsets = Enumerable.Range(0, parts.PostureYOffsets.Length)
            .Where(index => SamusBodyPlacementDefinitions.PostureSourceIndex(index) == index
                ? !SamusBodyPlacementDefinitions.TryDefaultPostureByte(this, index, out sbyte calculated) || parts.PostureYOffsets[index] != calculated
                : parts.PostureYOffsets[index] != parts.PostureYOffsets[SamusBodyPlacementDefinitions.PostureSourceIndex(index)])
            .ToDictionary(index => index, index => parts.PostureYOffsets[index]);
        this.drainedYOffsets = Enumerable.Range(0, parts.DrainedYOffsets.Length)
            .Where(index => !SamusBodyPlacementDefinitions.TryDefaultDrainedByte(this, index, out sbyte calculated) || parts.DrainedYOffsets[index] != calculated)
            .ToDictionary(index => index, index => parts.DrainedYOffsets[index]);
        foreach (ushort pointer in parts.PosePointers)
            if (pointer < FirstFrameOffset ||
                pointer >= FrameEndOffset ||
                (pointer - FirstFrameOffset) % 4 != 0)
                throw new InvalidDataException($"Samus pose selects invalid frame list ${pointer:X4}.");
        // The contiguous bank image also contains bytes reached only by a frame
        // counter running past its authored list. Validate a selection when it is
        // actually used, not every four-byte word in the backing ROM interval.
        BindTransfers(true, this.top);
        BindTransfers(false, this.bottom);
        definitionsByAddress.Clear();
        definitionOrdinals.Clear();
        IndexDefinitions(true, this.top);
        IndexDefinitions(false, this.bottom);
    }

    /// <summary>Resolves each definition's native transfer geometry against the selected frame tables.</summary>
    private void BindTransfers(bool upper, SamusBodyTileDefinition[][] groups)
    {
        ReadOnlySpan<ushort> pointers = PosePointers;
        ReadOnlySpan<SamusBodyFrameSelection> frameSnapshot = Frames;
        for (int set = 0; set < groups.Length; set++)
        for (int position = 0; position < groups[set].Length; position++)
            groups[set][position] = groups[set][position].WithTransferGeometry(this, upper, set, position, pointers, frameSnapshot);
    }

    /// <summary>SHA-256 of selected body art, all visual selectors and every bundled Samus catalog.</summary>
    public string ContentIdentity => CreateContentIdentity();

    /// <summary>Gets the resolved upper-body definition-group pointers.</summary>
    public ReadOnlySpan<ushort> TopSetPointers => Enumerable.Range(0, TopSetCount).Select(index => SetPointer(true, index)).ToArray();
    /// <summary>Gets the resolved lower-body definition-group pointers.</summary>
    public ReadOnlySpan<ushort> BottomSetPointers => Enumerable.Range(0, BottomSetCount).Select(index => SetPointer(false, index)).ToArray();
    /// <summary>Gets the resolved per-pose frame-list pointers.</summary>
    public ReadOnlySpan<ushort> PosePointers => Enumerable.Range(0, PoseCount).Select(pose => PosePointer((SamusPoseId)pose)).ToArray();
    /// <summary>Gets the resolved signed art-origin offset for every pose.</summary>
    public ReadOnlySpan<sbyte> GraphicsYOffsets => Enumerable.Range(0, PoseCount).Select(index => GraphicsYOffset((SamusPoseId)index)).ToArray();
    /// <summary>Native landing table, including the one adjacent byte read by an unaligned word.</summary>
    public ReadOnlySpan<ushort> LandingYOffsets => Enumerable.Range(0, SamusRenderingRomData.Body.LandingVerticalOffsetByteCount).Select(LandingByte).ToArray();
    /// <summary>Gets the resolved posture-transition vertical-offset table.</summary>
    public ReadOnlySpan<sbyte> PostureYOffsets => Enumerable.Range(0, SamusRenderingRomData.Body.PostureTransitionVerticalOffsetByteCount).Select(PostureByte).ToArray();
    /// <summary>Gets the resolved drained-state vertical-offset table.</summary>
    public ReadOnlySpan<sbyte> DrainedYOffsets => Enumerable.Range(0, SamusRenderingRomData.Body.DrainedVerticalOffsetByteCount).Select(DrainedByte).ToArray();
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
        if ((uint)index >= SamusRenderingRomData.Body.LandingVerticalOffsetByteCount - 1)
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
        if ((uint)index >= SamusRenderingRomData.Body.PostureTransitionVerticalOffsetByteCount) { value = 0; return false; }
        value = PostureByte(index);
        return true;
    }
    /// <summary>Attempts to read a signed drained-state art offset.</summary>
    /// <param name="index">The table byte index.</param>
    /// <param name="value">Receives the signed vertical offset.</param>
    /// <returns><see langword="true"/> when the index is in range.</returns>
    public bool TryDrainedYOffset(int index, out sbyte value)
    {
        if ((uint)index >= SamusRenderingRomData.Body.DrainedVerticalOffsetByteCount) { value = 0; return false; }
        value = DrainedByte(index);
        return true;
    }
    /// <summary>Signed pose art origin; changing it never changes a physical projectile origin.</summary>
    public sbyte GraphicsYOffset(SamusPoseId pose) =>
        (int)pose < PoseCount ? graphicsYOffsets.TryGetValue((int)pose, out sbyte value) ? value : SamusBodyPlacementDefinitions.DefaultGraphicsYOffset((SamusPoseId)pose) :
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

    /// <summary>Resolves an edited pose frame-list pointer or the compiled stock pointer.</summary>
    private ushort PosePointer(SamusPoseId pose) => posePointers.TryGetValue((int)pose, out ushort value)
        ? value : SamusBodyPoseDefinitions.DefaultFrameList(pose);

    /// <summary>Resolve the cartridge's pose pointer plus four bytes per animation frame.</summary>
    public SamusBodyFrameSelection Frame(SamusPoseId pose, ushort animationFrame)
    {
        if ((int)pose >= PoseCount)
            throw new InvalidDataException($"Pose ${(int)pose:X2} has no authored Samus body frame list.");
        ushort address = unchecked((ushort)(PosePointer(pose) + animationFrame * 4));
        if (address < FirstFrameOffset || address >= FrameEndOffset ||
            (address - FirstFrameOffset) % 4 != 0)
            throw new InvalidDataException($"Samus frame ${address:X4} is outside extracted visual selectors.");
        SamusBodyFrameSelection frame = FrameAt((address - FirstFrameOffset) / 4);
        GetDefinition(true, frame.TopSet, frame.TopPosition);
        if (frame.BottomSet != SamusRenderingRomData.TileTransfers.NoBottomTransferSet)
            GetDefinition(false, frame.BottomSet, frame.BottomPosition);
        return frame;
    }

    /// <summary>Resolves the selected upper or lower definition-group pointer.</summary>
    private ushort SetPointer(bool upperHalf, int set)
    {
        Dictionary<int, ushort> overrides = upperHalf ? topPointers : bottomPointers;
        return overrides.TryGetValue(set, out ushort value) ? value : upperHalf
            ? SamusBodyDefinitionLayout.DefaultTopPointer(set) : SamusBodyDefinitionLayout.DefaultBottomPointer(set);
    }
    /// <summary>Resolves a native set and position to its physical body tile definition.</summary>
    public SamusBodyTileDefinition GetDefinition(bool upperHalf, byte set, byte position)
    {
        if (set >= (upperHalf ? TopSetCount : BottomSetCount))
            throw new InvalidDataException(
                $"Samus {(upperHalf ? "top" : "bottom")} definition {set:X2}/{position:X2} is absent.");
        // The native selector adds position*7 to the chosen set pointer without a
        // per-set bounds check. Several authored frames intentionally land in the
        // next definition group; resolve the physical address, not a C# jagged index.
        int address = SamusBodyDefinitionLayout.BankBase | unchecked((ushort)(SetPointer(upperHalf, set) +
            position * SamusRenderingRomData.TileTransfers.DefinitionByteCount));
        return DefinitionAt(address);
    }

    /// <summary>Returns the SNES address selected by a native body set and position.</summary>
    public int DefinitionAddress(bool upperHalf, byte set, byte position)
    {
        _ = GetDefinition(upperHalf, set, position);
        return SamusBodyDefinitionLayout.BankBase | unchecked((ushort)(SetPointer(upperHalf, set) +
            position * SamusRenderingRomData.TileTransfers.DefinitionByteCount));
    }

    /// <summary>
    /// Samus's 32 body characters as the selected definitions make them visible: each definition's first transfer
    /// fills one eight-character half row (top: characters 0-7, bottom: 8-15) and its second transfer the half row
    /// below it (top: 16-23, bottom: 24-31). A half with no selected definition is blank.
    /// </summary>
    /// <param name="topAddress">The selected top definition's record address, or null.</param>
    /// <param name="bottomAddress">The selected bottom definition's record address, or null.</param>
    public SpriteArtwork BodyArtwork(int? topAddress, int? bottomAddress)
    {
        int top = topAddress is { } upper ? definitionOrdinals[upper] : -1;
        int bottom = bottomAddress is { } lower ? definitionOrdinals[lower] : -1;
        return bodyArtwork.GetOrAdd((top, bottom), key =>
        {
            var characters = new byte[BodyCharacterCount * SpriteArtwork.CharacterByteCount];
            if (topAddress is { } topRecord)
                CopyHalves(DefinitionAt(topRecord), characters, 0);
            if (bottomAddress is { } bottomRecord)
                CopyHalves(DefinitionAt(bottomRecord), characters, HalfRowCharacters);
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

    /// <summary>Resolve a saved native definition pointer after a debugger-state rebind.</summary>
    public SamusBodyTileDefinition DefinitionAt(int nativeAddress)
    {
        if (!definitionsByAddress.TryGetValue(nativeAddress, out SamusBodyTileDefinition? definition))
            throw new InvalidDataException($"Samus body definition ${nativeAddress:X6} is absent from installed art.");
        return definition;
    }

    /// <summary>Indexes each tile definition at its physical seven-byte bank-$92 record address.</summary>
    private void IndexDefinitions(bool upperHalf, SamusBodyTileDefinition[][] groups)
    {
        for (int set = 0; set < groups.Length; set++)
        for (int position = 0; position < groups[set].Length; position++)
        {
            int address = SamusBodyDefinitionLayout.BankBase | unchecked((ushort)(SetPointer(upperHalf, set) +
                position * SamusRenderingRomData.TileTransfers.DefinitionByteCount));
            if (!definitionsByAddress.TryAdd(address, groups[set][position]))
                throw new InvalidDataException($"Samus body definition ${address:X6} is duplicated.");
            definitionOrdinals.Add(address, definitionOrdinals.Count);
        }
    }

    /// <summary>Clones authored groups and validates each transfer slot against native capacity.</summary>
    private static SamusBodyTileDefinition[][] CloneAndValidate(
        ushort[] pointers, SamusBodyTileDefinition[][] groups)
    {
        var result = new SamusBodyTileDefinition[groups.Length][];
        for (int set = 0; set < groups.Length; set++)
        {
            SamusBodyTileDefinition[] source = groups[set] ??
                throw new InvalidDataException($"Samus body definition set {set} is missing.");
            result[set] = (SamusBodyTileDefinition[])source.Clone();
            if (source.Length == 0 || pointers[set] < SamusBodyDefinitionLayout.MinimumSetOffset)
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
    private string CreateContentIdentity() => SelectedPresentationHash.Create(nameof(SamusBodyArtworkCatalog), content =>
    {
        content.AppendWords("top pointers", TopSetPointers);
        content.AppendWords("bottom pointers", BottomSetPointers);
        content.AppendWords("pose pointers", PosePointers);
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
        foreach ((int address, SamusBodyTileDefinition definition) in definitionsByAddress.OrderBy(pair => pair.Key))
        {
            content.Append("definition address", address);
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
