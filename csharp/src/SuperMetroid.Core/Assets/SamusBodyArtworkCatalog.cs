using System.Text.Json.Serialization;
using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Assets;

/// <summary>Editable bank-$92 Samus body characters and their native frame/half selectors.</summary>
/// <remarks>
/// These records affect presentation only. Pose transitions, animation delays, collision,
/// and projectile timing remain in the compiled simulation. Definition addresses are kept
/// so the debugger can still identify the original seven-byte DMA record.
/// </remarks>
public sealed partial class SamusBodyArtworkCatalog
{
    public const int FirstFrameAddress = 0x92DB48;
    public const int FrameEndExclusive = 0x92ED24;
    public const int FirstFrameOffset = 0xDB48;
    public const int FrameEndOffset = 0xED24;
    public const int FrameCount = (FrameEndExclusive - FirstFrameAddress) / 4;
    public const int PoseCount = 253;
    public const int TopSetCount = 13;
    public const int BottomSetCount = 11;
    public const int TilesPerDefinition = 16;
    public const int BytesPerDefinitionSlot = TilesPerDefinition * 32;

    private readonly Dictionary<int, ushort> topPointers;
    private readonly Dictionary<int, ushort> bottomPointers;
    private readonly Dictionary<int, ushort> posePointers;
    private readonly Dictionary<int, sbyte> graphicsYOffsets;
    private readonly Dictionary<int, ushort> landingYOffsets;
    private readonly Dictionary<int, sbyte> postureYOffsets;
    private readonly Dictionary<int, sbyte> drainedYOffsets;
    private readonly Dictionary<int, byte> frames;
    private readonly SamusBodyTileDefinition[][] top;
    private readonly SamusBodyTileDefinition[][] bottom;
    private readonly Dictionary<int, SamusBodyTileDefinition> definitionsByAddress = [];

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

    public SamusBodyArtworkCatalog(ushort[] topPointers, ushort[] bottomPointers,
        ushort[] posePointers, sbyte[] graphicsYOffsets,
        SamusBodyFrameSelection[] frames,
        SamusBodyTileDefinition[][] top, SamusBodyTileDefinition[][] bottom,
        SamusSpritemapArtworkCatalog spritemaps, SamusAtmosphericArtworkCatalog atmosphere,
        SamusDeathPaletteArtworkCatalog deathPalettes,
        SamusDeathTileAtlas deathTiles,
        SamusArmCannonArtworkCatalog armCannon,
        ushort[] landingYOffsets,
        sbyte[] postureYOffsets, sbyte[] drainedYOffsets)
    {
        ArgumentNullException.ThrowIfNull(topPointers);
        ArgumentNullException.ThrowIfNull(bottomPointers);
        ArgumentNullException.ThrowIfNull(posePointers);
        ArgumentNullException.ThrowIfNull(graphicsYOffsets);
        ArgumentNullException.ThrowIfNull(frames);
        ArgumentNullException.ThrowIfNull(top);
        ArgumentNullException.ThrowIfNull(bottom);
        ArgumentNullException.ThrowIfNull(spritemaps);
        ArgumentNullException.ThrowIfNull(atmosphere);
        ArgumentNullException.ThrowIfNull(deathPalettes);
        ArgumentNullException.ThrowIfNull(deathTiles);
        ArgumentNullException.ThrowIfNull(armCannon);
        ArgumentNullException.ThrowIfNull(landingYOffsets);
        ArgumentNullException.ThrowIfNull(postureYOffsets);
        ArgumentNullException.ThrowIfNull(drainedYOffsets);
        if (topPointers.Length != TopSetCount || bottomPointers.Length != BottomSetCount ||
            posePointers.Length != PoseCount || graphicsYOffsets.Length != PoseCount ||
            landingYOffsets.Length != SamusRenderingRomData.Body.LandingVerticalOffsetByteCount ||
            postureYOffsets.Length != SamusRenderingRomData.Body.PostureTransitionVerticalOffsetByteCount ||
            drainedYOffsets.Length != SamusRenderingRomData.Body.DrainedVerticalOffsetByteCount ||
            frames.Length != FrameCount ||
            top.Length != TopSetCount || bottom.Length != BottomSetCount)
            throw new InvalidDataException("Samus body selector tables have an invalid length.");

        this.topPointers = Enumerable.Range(0, topPointers.Length)
            .Where(index => topPointers[index] != SamusBodyDefinitionLayout.DefaultTopPointer(index))
            .ToDictionary(index => index, index => topPointers[index]);
        this.bottomPointers = Enumerable.Range(0, bottomPointers.Length)
            .Where(index => bottomPointers[index] != SamusBodyDefinitionLayout.DefaultBottomPointer(index))
            .ToDictionary(index => index, index => bottomPointers[index]);
        this.posePointers = Enumerable.Range(0, posePointers.Length)
            .Where(pose => posePointers[pose] != SamusBodyPoseDefinitions.DefaultFrameList((byte)pose))
            .ToDictionary(pose => pose, pose => posePointers[pose]);
        this.graphicsYOffsets = Enumerable.Range(0, graphicsYOffsets.Length)
            .Where(index => graphicsYOffsets[index] != SamusBodyPlacementDefinitions.DefaultGraphicsYOffset((byte)index))
            .ToDictionary(index => index, index => graphicsYOffsets[index]);
        if (landingYOffsets.Any(value => value > byte.MaxValue))
            throw new InvalidDataException("Samus landing visual bytes must fit in one byte.");
        this.landingYOffsets = Enumerable.Range(0, landingYOffsets.Length)
            .Where(index => landingYOffsets[index] != (SamusBodyPlacementDefinitions.LandingSourceIndex(index) == index
                ? SamusBodyPlacementDefinitions.DefaultLandingByte(index)
                : landingYOffsets[SamusBodyPlacementDefinitions.LandingSourceIndex(index)]))
            .ToDictionary(index => index, index => landingYOffsets[index]);

        byte[] components = frames.SelectMany(frame => new byte[] { frame.TopSet, frame.TopPosition, frame.BottomSet, frame.BottomPosition }).ToArray();
        this.frames = Enumerable.Range(0, components.Length)
            .Where(index => SamusBodyFrameDefinitions.SourceComponent(index) != index
                ? components[index] != components[SamusBodyFrameDefinitions.SourceComponent(index)]
                : !SamusBodyFrameDefinitions.TryComponent(index, out byte calculated) || components[index] != calculated)
            .ToDictionary(index => index, index => components[index]);
        Spritemaps = spritemaps;
        Atmosphere = atmosphere;
        DeathPalettes = deathPalettes;
        DeathTiles = deathTiles;
        ArmCannon = armCannon.WithBodyGeometry(this);
        this.top = CloneAndValidate(topPointers, top);
        this.bottom = CloneAndValidate(bottomPointers, bottom);
        SamusBodyDefinitionLayout.ValidateCompleteGroups(topPointers, bottomPointers, this.top, this.bottom);
        IndexDefinitions(true, this.top);
        IndexDefinitions(false, this.bottom);
        this.postureYOffsets = Enumerable.Range(0, postureYOffsets.Length)
            .Where(index => SamusBodyPlacementDefinitions.PostureSourceIndex(index) == index
                ? !SamusBodyPlacementDefinitions.TryDefaultPostureByte(this, index, out sbyte calculated) || postureYOffsets[index] != calculated
                : postureYOffsets[index] != postureYOffsets[SamusBodyPlacementDefinitions.PostureSourceIndex(index)])
            .ToDictionary(index => index, index => postureYOffsets[index]);
        this.drainedYOffsets = Enumerable.Range(0, drainedYOffsets.Length)
            .Where(index => !SamusBodyPlacementDefinitions.TryDefaultDrainedByte(this, index, out sbyte calculated) || drainedYOffsets[index] != calculated)
            .ToDictionary(index => index, index => drainedYOffsets[index]);
        foreach (ushort pointer in posePointers)
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
        IndexDefinitions(true, this.top);
        IndexDefinitions(false, this.bottom);
    }

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

    public ReadOnlySpan<ushort> TopSetPointers => Enumerable.Range(0, TopSetCount).Select(index => SetPointer(true, index)).ToArray();
    public ReadOnlySpan<ushort> BottomSetPointers => Enumerable.Range(0, BottomSetCount).Select(index => SetPointer(false, index)).ToArray();
    public ReadOnlySpan<ushort> PosePointers => Enumerable.Range(0, PoseCount).Select(pose => PosePointer((byte)pose)).ToArray();
    public ReadOnlySpan<sbyte> GraphicsYOffsets => Enumerable.Range(0, PoseCount).Select(index => GraphicsYOffset((byte)index)).ToArray();
    /// <summary>Native landing table, including the one adjacent byte read by an unaligned word.</summary>
    public ReadOnlySpan<ushort> LandingYOffsets => Enumerable.Range(0, SamusRenderingRomData.Body.LandingVerticalOffsetByteCount).Select(LandingByte).ToArray();
    public ReadOnlySpan<sbyte> PostureYOffsets => Enumerable.Range(0, SamusRenderingRomData.Body.PostureTransitionVerticalOffsetByteCount).Select(PostureByte).ToArray();
    public ReadOnlySpan<sbyte> DrainedYOffsets => Enumerable.Range(0, SamusRenderingRomData.Body.DrainedVerticalOffsetByteCount).Select(DrainedByte).ToArray();
    private ushort LandingByte(int index) => landingYOffsets.TryGetValue(index, out ushort value)
        ? value : SamusBodyPlacementDefinitions.LandingSourceIndex(index) == index
            ? SamusBodyPlacementDefinitions.DefaultLandingByte(index) : LandingByte(SamusBodyPlacementDefinitions.LandingSourceIndex(index));
    private sbyte PostureByte(int index) => postureYOffsets.TryGetValue(index, out sbyte value)
        ? value : SamusBodyPlacementDefinitions.PostureSourceIndex(index) != index
            ? PostureByte(SamusBodyPlacementDefinitions.PostureSourceIndex(index))
            : SamusBodyPlacementDefinitions.TryDefaultPostureByte(this, index, out sbyte calculated)
                ? calculated : throw new InvalidDataException("Selected posture geometry no longer supplies its installed offset.");

    private sbyte DrainedByte(int index) => drainedYOffsets.TryGetValue(index, out sbyte value)
        ? value : SamusBodyPlacementDefinitions.TryDefaultDrainedByte(this, index, out sbyte calculated)
            ? calculated : throw new InvalidDataException("Selected drained geometry no longer supplies its installed offset.");
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
    public bool TryPostureYOffset(int index, out sbyte value)
    {
        if ((uint)index >= SamusRenderingRomData.Body.PostureTransitionVerticalOffsetByteCount) { value = 0; return false; }
        value = PostureByte(index);
        return true;
    }
    public bool TryDrainedYOffset(int index, out sbyte value)
    {
        if ((uint)index >= SamusRenderingRomData.Body.DrainedVerticalOffsetByteCount) { value = 0; return false; }
        value = DrainedByte(index);
        return true;
    }
    /// <summary>Signed pose art origin; changing it never changes a physical projectile origin.</summary>
    public sbyte GraphicsYOffset(byte pose) =>
        pose < PoseCount ? graphicsYOffsets.TryGetValue(pose, out sbyte value) ? value : SamusBodyPlacementDefinitions.DefaultGraphicsYOffset(pose) :
            throw new InvalidDataException($"Pose ${pose:X2} has no authored graphics Y offset.");
    public ReadOnlySpan<SamusBodyFrameSelection> Frames => Enumerable.Range(0, FrameCount).Select(FrameAt).ToArray();
    private byte FrameComponent(int index) => frames.TryGetValue(index, out byte value)
        ? value : SamusBodyFrameDefinitions.SourceComponent(index) != index
            ? FrameComponent(SamusBodyFrameDefinitions.SourceComponent(index))
            : SamusBodyFrameDefinitions.TryComponent(index, out byte calculated) ? calculated
                : throw new InvalidDataException("Installed body frame has no selected component.");
    private SamusBodyFrameSelection FrameAt(int index) => new(FrameComponent(index * 4), FrameComponent(index * 4 + 1),
        FrameComponent(index * 4 + 2), FrameComponent(index * 4 + 3));
    public IReadOnlyList<SamusBodyTileDefinition> TopSet(int set) => top[set];
    public IReadOnlyList<SamusBodyTileDefinition> BottomSet(int set) => bottom[set];

    private ushort PosePointer(byte pose) => posePointers.TryGetValue(pose, out ushort value)
        ? value : SamusBodyPoseDefinitions.DefaultFrameList(pose);

    /// <summary>Resolve the cartridge's pose pointer plus four bytes per animation frame.</summary>
    public SamusBodyFrameSelection Frame(byte pose, ushort animationFrame)
    {
        if (pose >= PoseCount)
            throw new InvalidDataException($"Pose ${pose:X2} has no authored Samus body frame list.");
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

    private ushort SetPointer(bool upperHalf, int set)
    {
        Dictionary<int, ushort> overrides = upperHalf ? topPointers : bottomPointers;
        return overrides.TryGetValue(set, out ushort value) ? value : upperHalf
            ? SamusBodyDefinitionLayout.DefaultTopPointer(set) : SamusBodyDefinitionLayout.DefaultBottomPointer(set);
    }
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
        return DefinitionAt(upperHalf, address);
    }

    public int DefinitionAddress(bool upperHalf, byte set, byte position)
    {
        _ = GetDefinition(upperHalf, set, position);
        return SamusBodyDefinitionLayout.BankBase | unchecked((ushort)(SetPointer(upperHalf, set) +
            position * SamusRenderingRomData.TileTransfers.DefinitionByteCount));
    }

    /// <summary>Resolve a saved native definition pointer after a debugger-state rebind.</summary>
    public SamusBodyTileDefinition DefinitionAt(bool upperHalf, int nativeAddress)
    {
        if (!definitionsByAddress.TryGetValue(nativeAddress, out SamusBodyTileDefinition? definition))
            throw new InvalidDataException($"Samus body definition ${nativeAddress:X6} is absent from installed art.");
        return definition;
    }

    private void IndexDefinitions(bool upperHalf, SamusBodyTileDefinition[][] groups)
    {
        for (int set = 0; set < groups.Length; set++)
        for (int position = 0; position < groups[set].Length; position++)
        {
            int address = SamusBodyDefinitionLayout.BankBase | unchecked((ushort)(SetPointer(upperHalf, set) +
                position * SamusRenderingRomData.TileTransfers.DefinitionByteCount));
            if (!definitionsByAddress.TryAdd(address, groups[set][position]))
                throw new InvalidDataException($"Samus body definition ${address:X6} is duplicated.");
        }
    }

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
}

/// <summary>Four-byte native selector: upper set/position, then lower set/position.</summary>
public readonly record struct SamusBodyFrameSelection(
    [property: JsonRequired] byte TopSet,
    [property: JsonRequired] byte TopPosition,
    [property: JsonRequired] byte BottomSet,
    [property: JsonRequired] byte BottomPosition);

/// <summary>One native seven-byte transfer compiled from palette-indexed PNG tiles.</summary>
public sealed class SamusBodyTileDefinition
{
    private readonly byte[]? standalonePlanar;
    private readonly Dictionary<int, byte>? pixelInputs;
    private readonly Dictionary<int, byte>? contourEdits;
    internal int PayloadLength { get; }
    private readonly int? sourceAddressOverride;
    private readonly ushort? firstSizeOverride;
    // The body's pointers and frames are fixed once it is bound, so the calculated first
    // transfer size is resolved at binding instead of rescanning every pose on each read.
    private readonly ushort? calculatedFirstSize;
    private readonly ushort standaloneSecondSize;
    private readonly SamusBodyArtworkCatalog? body;
    private readonly bool upper;
    private readonly int set;
    private readonly int position;

    public SamusBodyTileDefinition(int sourceAddress, ushort firstSize, ushort secondSize,
        byte[] planar)
    {
        ArgumentNullException.ThrowIfNull(planar);
        sourceAddressOverride = sourceAddress;
        firstSizeOverride = firstSize;
        standaloneSecondSize = secondSize;
        standalonePlanar = (byte[])planar.Clone();
        PayloadLength = planar.Length;
    }

    private SamusBodyTileDefinition(SamusBodyTileDefinition supplied, SamusBodyArtworkCatalog body,
        bool upper, int set, int position, ReadOnlySpan<ushort> pointers, ReadOnlySpan<SamusBodyFrameSelection> frames)
    {
        this.body = body;
        this.upper = upper;
        this.set = set;
        this.position = position;
        PayloadLength = supplied.PayloadLength;
        // Collect the sparse edits first so each dictionary is created at its final size;
        // growing one entry at a time reallocated every tile's tables several times over.
        int[] scratchIndexes = System.Buffers.ArrayPool<int>.Shared.Rent(PayloadLength * 2);
        byte[] scratchValues = System.Buffers.ArrayPool<byte>.Shared.Rent(PayloadLength * 2);
        try
        {
            int pixelCount = 0, contourCount = 0;
            for (int index = 0; index < PayloadLength; index++)
            {
                byte value = supplied.ReadPlanarByte(index);
                byte mask = SamusBodyPixelDefinitions.ContourMask(upper, set, position, index);
                byte outside = (byte)(value & ~mask);
                if (outside != 0)
                {
                    scratchIndexes[PayloadLength + contourCount] = index;
                    scratchValues[PayloadLength + contourCount++] = outside;
                }
                value &= mask;
                if (!TryPixelDefault(index, out byte pixel) || value != pixel)
                {
                    scratchIndexes[pixelCount] = index;
                    scratchValues[pixelCount++] = value;
                }
            }
            pixelInputs = new Dictionary<int, byte>(pixelCount);
            for (int entry = 0; entry < pixelCount; entry++)
                pixelInputs.Add(scratchIndexes[entry], scratchValues[entry]);
            contourEdits = new Dictionary<int, byte>(contourCount);
            for (int entry = 0; entry < contourCount; entry++)
                contourEdits.Add(scratchIndexes[PayloadLength + entry], scratchValues[PayloadLength + entry]);
        }
        finally
        {
            System.Buffers.ArrayPool<int>.Shared.Return(scratchIndexes);
            System.Buffers.ArrayPool<byte>.Shared.Return(scratchValues);
        }
        int source = supplied.SourceAddress;
        ushort first = supplied.FirstSize;
        sourceAddressOverride = source == SamusBodyTransferDefinitions.SourceAddress(body, upper, set, position) ? null : source;
        if (SamusBodyTransferDefinitions.TryFirstSize(body, upper, set, position, PayloadLength, pointers, frames, out ushort calculated))
            calculatedFirstSize = calculated;
        firstSizeOverride = calculatedFirstSize == first ? null : first;
    }

    internal SamusBodyTileDefinition WithTransferGeometry(SamusBodyArtworkCatalog body, bool upper, int set, int position,
        ReadOnlySpan<ushort> pointers, ReadOnlySpan<SamusBodyFrameSelection> frames) =>
        new(this, body, upper, set, position, pointers, frames);

    public int SourceAddress => sourceAddressOverride ?? SamusBodyTransferDefinitions.SourceAddress(body!, upper, set, position);
    public ushort FirstSize => firstSizeOverride ?? calculatedFirstSize ??
        throw new InvalidDataException("Installed body composition no longer supplies its transfer row.");
    public ushort SecondSize => body is null ? standaloneSecondSize : (ushort)(PayloadLength - FirstSize);
    /// <summary>Canonical planar snapshot; native padding and shared angle patches calculate.</summary>
    public ReadOnlyMemory<byte> Planar
    {
        get
        {
            if (standalonePlanar is not null) return standalonePlanar;
            var result = new byte[PayloadLength];
            for (int index = 0; index < result.Length; index++) result[index] = ReadPlanarByte(index);
            return result;
        }
    }

    internal byte ReadPlanarByte(int index)
    {
        if ((uint)index >= PayloadLength) throw new ArgumentOutOfRangeException(nameof(index));
        if (standalonePlanar is not null) return standalonePlanar[index];
        if (pixelInputs!.TryGetValue(index, out byte value))
            return (byte)((value & SamusBodyPixelDefinitions.ContourMask(upper, set, position, index)) | contourEdits!.GetValueOrDefault(index));
        return TryPixelDefault(index, out byte calculated) ? calculated
            : throw new InvalidDataException("Installed body artwork lost an independent pixel input.");
    }

    private bool TryPixelDefault(int index, out byte value)
    {
        if (SamusBodyPixelDefinitions.TryDiagnosticByte(upper, set, position, index, out value)) return true;
        int tile = index / SamusBodyPixelDefinitions.TileBytes;
        if (SamusBodyPixelDefinitions.IsBlank(upper, set, position, tile))
        {
            value = 0;
            return true;
        }
        if (SamusBodyPixelDefinitions.TrySourceByte(upper, set, position, index, out int sourcePosition, out int sourceIndex))
        {
            SamusBodyTileDefinition source = (upper ? body!.TopSet(set) : body!.BottomSet(set))[sourcePosition];
            if ((uint)sourceIndex < source.PayloadLength)
            {
                value = source.ReadPlanarByte(sourceIndex);
                return true;
            }
        }
        value = 0;
        return false;
    }
}
