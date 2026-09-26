using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Assets;

/// <summary>Editable bank-$92 Samus body characters and their native frame/half selectors.</summary>
/// <remarks>
/// These records affect presentation only. Pose transitions, animation delays, collision,
/// and projectile timing remain in the compiled simulation. Definition addresses are kept
/// so the debugger can still identify the original seven-byte DMA record.
/// </remarks>
public sealed class SamusBodyArtworkCatalog
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

    private readonly ushort[] topPointers;
    private readonly ushort[] bottomPointers;
    private readonly ushort[] posePointers;
    private readonly sbyte[] graphicsYOffsets;
    private readonly ushort[] landingYOffsets;
    private readonly sbyte[] postureYOffsets;
    private readonly sbyte[] drainedYOffsets;
    private readonly SamusBodyFrameSelection[] frames;
    private readonly SamusBodyTileDefinition[][] top;
    private readonly SamusBodyTileDefinition[][] bottom;
    private readonly Dictionary<int, SamusBodyTileDefinition> definitionsByAddress = [];

    /// <summary>Editable bank-$92 OAM composition for these body frames.</summary>
    public SamusSpritemapArtworkCatalog Spritemaps { get; }

    /// <summary>Editable direct small-OBJ attributes for Samus's atmospheric effects.</summary>
    public SamusAtmosphericArtworkCatalog Atmosphere { get; }

    public SamusBodyArtworkCatalog(ushort[] topPointers, ushort[] bottomPointers,
        ushort[] posePointers, sbyte[] graphicsYOffsets,
        SamusBodyFrameSelection[] frames,
        SamusBodyTileDefinition[][] top, SamusBodyTileDefinition[][] bottom,
        SamusSpritemapArtworkCatalog spritemaps, SamusAtmosphericArtworkCatalog atmosphere,
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

        this.topPointers = (ushort[])topPointers.Clone();
        this.bottomPointers = (ushort[])bottomPointers.Clone();
        this.posePointers = (ushort[])posePointers.Clone();
        this.graphicsYOffsets = (sbyte[])graphicsYOffsets.Clone();
        this.landingYOffsets = (ushort[])landingYOffsets.Clone();
        if (this.landingYOffsets.Any(value => value > byte.MaxValue))
            throw new InvalidDataException("Samus landing visual bytes must fit in one byte.");
        this.postureYOffsets = (sbyte[])postureYOffsets.Clone();
        this.drainedYOffsets = (sbyte[])drainedYOffsets.Clone();
        this.frames = (SamusBodyFrameSelection[])frames.Clone();
        Spritemaps = spritemaps;
        Atmosphere = atmosphere;
        this.top = CloneAndValidate(topPointers, top);
        this.bottom = CloneAndValidate(bottomPointers, bottom);
        IndexDefinitions(true, this.top);
        IndexDefinitions(false, this.bottom);
        foreach (ushort pointer in this.posePointers)
            if (pointer < FirstFrameOffset ||
                pointer >= FrameEndOffset ||
                (pointer - FirstFrameOffset) % 4 != 0)
                throw new InvalidDataException($"Samus pose selects invalid frame list ${pointer:X4}.");
        // The contiguous bank image also contains bytes reached only by a frame
        // counter running past its authored list. Validate a selection when it is
        // actually used, not every four-byte word in the backing ROM interval.
    }

    public ReadOnlySpan<ushort> TopSetPointers => topPointers;
    public ReadOnlySpan<ushort> BottomSetPointers => bottomPointers;
    public ReadOnlySpan<ushort> PosePointers => posePointers;
    public ReadOnlySpan<sbyte> GraphicsYOffsets => graphicsYOffsets;
    /// <summary>Native landing table, including the one adjacent byte read by an unaligned word.</summary>
    public ReadOnlySpan<ushort> LandingYOffsets => landingYOffsets;
    public ReadOnlySpan<sbyte> PostureYOffsets => postureYOffsets;
    public ReadOnlySpan<sbyte> DrainedYOffsets => drainedYOffsets;
    public bool TryLandingYOffset(int index, out ushort value)
    {
        if ((uint)index >= landingYOffsets.Length - 1)
        {
            value = 0;
            return false;
        }
        value = (ushort)(landingYOffsets[index] | landingYOffsets[index + 1] << 8);
        return true;
    }
    public bool TryPostureYOffset(int index, out sbyte value)
    {
        if ((uint)index >= postureYOffsets.Length) { value = 0; return false; }
        value = postureYOffsets[index];
        return true;
    }
    public bool TryDrainedYOffset(int index, out sbyte value)
    {
        if ((uint)index >= drainedYOffsets.Length) { value = 0; return false; }
        value = drainedYOffsets[index];
        return true;
    }
    /// <summary>Signed pose art origin; changing it never changes a physical projectile origin.</summary>
    public sbyte GraphicsYOffset(byte pose) =>
        pose < PoseCount ? graphicsYOffsets[pose] :
            throw new InvalidDataException($"Pose ${pose:X2} has no authored graphics Y offset.");
    public ReadOnlySpan<SamusBodyFrameSelection> Frames => frames;
    public IReadOnlyList<SamusBodyTileDefinition> TopSet(int set) => top[set];
    public IReadOnlyList<SamusBodyTileDefinition> BottomSet(int set) => bottom[set];

    /// <summary>Resolve the cartridge's pose pointer plus four bytes per animation frame.</summary>
    public SamusBodyFrameSelection Frame(byte pose, ushort animationFrame)
    {
        if (pose >= PoseCount)
            throw new InvalidDataException($"Pose ${pose:X2} has no authored Samus body frame list.");
        ushort address = unchecked((ushort)(posePointers[pose] + animationFrame * 4));
        if (address < FirstFrameOffset || address >= FrameEndOffset ||
            (address - FirstFrameOffset) % 4 != 0)
            throw new InvalidDataException($"Samus frame ${address:X4} is outside extracted visual selectors.");
        SamusBodyFrameSelection frame = frames[(address - FirstFrameOffset) / 4];
        GetDefinition(true, frame.TopSet, frame.TopPosition);
        if (frame.BottomSet != 0xFF)
            GetDefinition(false, frame.BottomSet, frame.BottomPosition);
        return frame;
    }

    public SamusBodyTileDefinition GetDefinition(bool upperHalf, byte set, byte position)
    {
        ushort[] pointers = upperHalf ? topPointers : bottomPointers;
        if (set >= pointers.Length)
            throw new InvalidDataException(
                $"Samus {(upperHalf ? "top" : "bottom")} definition {set:X2}/{position:X2} is absent.");
        // The native selector adds position*7 to the chosen set pointer without a
        // per-set bounds check. Several authored frames intentionally land in the
        // next definition group; resolve the physical address, not a C# jagged index.
        int address = 0x920000 | unchecked((ushort)(pointers[set] + position * 7));
        return DefinitionAt(upperHalf, address);
    }

    public int DefinitionAddress(bool upperHalf, byte set, byte position)
    {
        _ = GetDefinition(upperHalf, set, position);
        return 0x920000 | unchecked((ushort)((upperHalf ? topPointers : bottomPointers)[set] + position * 7));
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
        ushort[] pointers = upperHalf ? topPointers : bottomPointers;
        for (int set = 0; set < groups.Length; set++)
        for (int position = 0; position < groups[set].Length; position++)
        {
            int address = 0x920000 | unchecked((ushort)(pointers[set] + position * 7));
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
            if (source.Length == 0 || pointers[set] < 0x8000)
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
    byte TopSet, byte TopPosition, byte BottomSet, byte BottomPosition);

/// <summary>One native seven-byte transfer compiled from palette-indexed PNG tiles.</summary>
public sealed class SamusBodyTileDefinition
{
    private readonly byte[] planar;

    public SamusBodyTileDefinition(int sourceAddress, ushort firstSize, ushort secondSize,
        byte[] planar)
    {
        ArgumentNullException.ThrowIfNull(planar);
        SourceAddress = sourceAddress;
        FirstSize = firstSize;
        SecondSize = secondSize;
        this.planar = (byte[])planar.Clone();
    }

    public int SourceAddress { get; }
    public ushort FirstSize { get; }
    public ushort SecondSize { get; }
    public ReadOnlyMemory<byte> Planar => planar;
}
