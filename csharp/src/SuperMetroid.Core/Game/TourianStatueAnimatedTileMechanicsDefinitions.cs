namespace SuperMetroid.Core.Game;

/// <summary>
/// Immutable engine-owned headers and control programs for the four Tourian entrance
/// statue animated-tile objects in bank <c>$87</c>.
/// </summary>
/// <remarks>
/// The nine frame-source operands in each program are presentation references in
/// <see cref="SuperMetroid.Core.Assets.TourianStatueAnimatedTileArtworkDefinitions"/>.
/// This catalog owns the instruction graph, timing, event routing, boss-bit
/// tests, palette destinations, and effect parameters that make the sequence operate.
/// </remarks>
public static class TourianStatueAnimatedTileMechanicsDefinitions
{
    /// <summary>First timed source operand, $87:83B8 relative to Phantoon's $83AC entry.</summary>
    internal const int FirstOscillatingSource = 0x0c;
    /// <summary>First post-palette-clear source operand, $87:83E6 relative to $83AC.</summary>
    internal const int FirstReleaseSource = 0x3a;
    /// <summary>Source operand for the eye-glow wait, $87:83F2 relative to $83AC.</summary>
    internal const int EyeGlowSource = 0x46;
    /// <summary>Source operand for the soul/palette-FX wait, $87:83FE relative to $83AC.</summary>
    internal const int SoulSource = 0x52;
    /// <summary>
    /// Native boss identity selects its animation program, transfer geometry, event,
    /// boss test and palette/effect destinations. These are semantic object cases,
    /// not sampled numeric curves. The six-byte headers are $87:854C..8563 and their
    /// 104-byte programs are $87:83AC..854B; unsupported ushort identities return null.
    /// </summary>
    private static TourianStatueAnimatedTileProgramDefinition? SelectObject(AnimatedTileObject objectPointer) =>
        objectPointer switch
    {
        AnimatedTileObject.TourianStatuePhantoon => new(new TourianStatueAnimatedTileProgramParts { ObjectPointer = AnimatedTileObject.TourianStatuePhantoon, ProgramStart = 0x83ac, TransferByteCount = 0x0080, EncodedVramDestination = 0x7800, StatueStateBit = 0x0001, GreyEventNumber = 0x0006, FirstFrameDuration = 0x0006, PackedBossTest = 0x0301, ClearPaletteByteIndex = 0x0158, UnlockEffectParameter = 0x0000, PaletteFxDefinition = 0xf755, TargetPaletteByteIndex = 0x0140 }),
        AnimatedTileObject.TourianStatueRidley => new(new TourianStatueAnimatedTileProgramParts { ObjectPointer = AnimatedTileObject.TourianStatueRidley, ProgramStart = 0x8414, TransferByteCount = 0x0040, EncodedVramDestination = 0x7220, StatueStateBit = 0x0002, GreyEventNumber = 0x0007, FirstFrameDuration = 0x000a, PackedBossTest = 0x0201, ClearPaletteByteIndex = 0x0132, UnlockEffectParameter = 0x0002, PaletteFxDefinition = 0xf751, TargetPaletteByteIndex = 0x0120 }),
        AnimatedTileObject.TourianStatueKraid => new(new TourianStatueAnimatedTileProgramParts { ObjectPointer = AnimatedTileObject.TourianStatueKraid, ProgramStart = 0x847c, TransferByteCount = 0x0040, EncodedVramDestination = 0x0b40, StatueStateBit = 0x0004, GreyEventNumber = 0x0009, FirstFrameDuration = 0x0004, PackedBossTest = 0x0101, ClearPaletteByteIndex = 0x00f8, UnlockEffectParameter = 0x0006, PaletteFxDefinition = 0xf74d, TargetPaletteByteIndex = 0x00e0 }),
        AnimatedTileObject.TourianStatueDraygon => new(new TourianStatueAnimatedTileProgramParts { ObjectPointer = AnimatedTileObject.TourianStatueDraygon, ProgramStart = 0x84e4, TransferByteCount = 0x0080, EncodedVramDestination = 0x0ca0, StatueStateBit = 0x0008, GreyEventNumber = 0x0008, FirstFrameDuration = 0x0008, PackedBossTest = 0x0401, ClearPaletteByteIndex = 0x00d2, UnlockEffectParameter = 0x0004, PaletteFxDefinition = 0xf749, TargetPaletteByteIndex = 0x00c0 }),
        AnimatedTileObject.None or AnimatedTileObject.Empty or AnimatedTileObject.HorizontalSpikes or
            AnimatedTileObject.VerticalSpikes or AnimatedTileObject.CrateriaLake or
            AnimatedTileObject.UnusedCrateriaLava or AnimatedTileObject.BrinstarPlant or
            AnimatedTileObject.WreckedShipScreen or AnimatedTileObject.MaridiaSandCeiling or
            AnimatedTileObject.MaridiaSandFalling or AnimatedTileObject.WreckedShipTreadmillRightwards or
            AnimatedTileObject.WreckedShipTreadmillLeftwards or AnimatedTileObject.Lava or
            AnimatedTileObject.Acid or AnimatedTileObject.Rain or AnimatedTileObject.Spores => null,
        _ => throw new InvalidOperationException($"Undefined AnimatedTileObject {objectPointer}."),
    };

    /// <summary>The four boss-statue programs in cartridge header order, without a stored roster.</summary>
    public static IEnumerable<TourianStatueAnimatedTileProgramDefinition> All
    {
        get
        {
            yield return SelectObject(AnimatedTileObject.TourianStatuePhantoon)!;
            yield return SelectObject(AnimatedTileObject.TourianStatueRidley)!;
            yield return SelectObject(AnimatedTileObject.TourianStatueKraid)!;
            yield return SelectObject(AnimatedTileObject.TourianStatueDraygon)!;
        }
    }

    /// <summary>Resolves one stock bank-$87 statue animated-tile object header.</summary>
    public static bool TryResolveObjectHeader(
        AnimatedTileObject objectPointer,
        out TourianStatueAnimatedTileProgramDefinition definition)
    {
        definition = SelectObject(objectPointer)!;
        return definition is not null;
    }
}

/// <summary>One Tourian boss statue's complete mechanics program.</summary>
public sealed class TourianStatueAnimatedTileProgramDefinition
{
    internal TourianStatueAnimatedTileProgramDefinition(TourianStatueAnimatedTileProgramParts parts)
    {
        ObjectPointer = parts.ObjectPointer;
        ProgramStart = parts.ProgramStart;
        TransferByteCount = parts.TransferByteCount;
        EncodedVramDestination = parts.EncodedVramDestination;
        StatueStateBit = parts.StatueStateBit;
        GreyEventNumber = parts.GreyEventNumber;
        FirstFrameDuration = parts.FirstFrameDuration;
        PackedBossTest = parts.PackedBossTest;
        ClearPaletteByteIndex = parts.ClearPaletteByteIndex;
        UnlockEffectParameter = parts.UnlockEffectParameter;
        PaletteFxDefinition = parts.PaletteFxDefinition;
        TargetPaletteByteIndex = parts.TargetPaletteByteIndex;
        SourceOperandPointers = new CalculatedSourceOperands(parts.ProgramStart);
    }

    /// <summary>The animated-tile object header address in bank $87.</summary>
    public AnimatedTileObject ObjectPointer { get; }
    /// <summary>The first instruction of the object's fixed 104-byte program.</summary>
    public ushort ProgramStart { get; }
    /// <summary>The byte count uploaded by each timed frame.</summary>
    public ushort TransferByteCount { get; }
    /// <summary>The hardware-encoded VRAM destination for each timed frame.</summary>
    public ushort EncodedVramDestination { get; }
    /// <summary>The queued-DMA destination <see cref="EncodedVramDestination"/> decodes to.</summary>
    [PpuLayoutBoundary("codec: Tourian statue animated-tile program's native VRAM destination")]
    public VramDmaDestination Destination => VramDmaDestination.FromNative(EncodedVramDestination);
    /// <summary>The statue-specific bit manipulated by $87:8349/$8352.</summary>
    public ushort StatueStateBit { get; }
    /// <summary>The event set once this statue has turned grey.</summary>
    public ushort GreyEventNumber { get; }
    /// <summary>The statue-specific first-frame duration.</summary>
    public ushort FirstFrameDuration { get; }
    /// <summary>The area byte and boss mask consumed by $87:8303.</summary>
    public ushort PackedBossTest { get; }
    /// <summary>The byte offset of the first of three colors cleared by $87:835B.</summary>
    public ushort ClearPaletteByteIndex { get; }
    /// <summary>The even statue selector passed to the eye-glow and soul spawners.</summary>
    public ushort UnlockEffectParameter { get; }
    /// <summary>The bank-$8D palette-FX definition spawned during release.</summary>
    public ushort PaletteFxDefinition { get; }
    /// <summary>The byte offset receiving the eight common grey target colors.</summary>
    public ushort TargetPaletteByteIndex { get; }
    /// <summary>
    /// The nine artwork operands: five four-byte oscillation frames, two four-byte
    /// release frames, then the eye-glow and soul waits. Positions are calculated
    /// from the shared instruction layout, with ushort wrapping and list bounds.
    /// </summary>
    public IReadOnlyList<ushort> SourceOperandPointers { get; }

    private sealed class CalculatedSourceOperands(ushort programStart) : IReadOnlyList<ushort>
    {
        public int Count => 9;
        public ushort this[int index]
        {
            get
            {
                if ((uint)index >= Count) throw new ArgumentOutOfRangeException(nameof(index));
                int offset = index switch
                {
                    <= 4 => TourianStatueAnimatedTileMechanicsDefinitions.FirstOscillatingSource + 4 * index,
                    <= 6 => TourianStatueAnimatedTileMechanicsDefinitions.FirstReleaseSource + 4 * (index - 5),
                    7 => TourianStatueAnimatedTileMechanicsDefinitions.EyeGlowSource,
                    _ => TourianStatueAnimatedTileMechanicsDefinitions.SoulSource,
                };
                return unchecked((ushort)(programStart + offset));
            }
        }

        public IEnumerator<ushort> GetEnumerator()
        {
            for (int index = 0; index < Count; index++) yield return this[index];
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }

    /// <summary>
    /// Reads the three header words and the shared native instruction layout at
    /// $87:83AC/8414/847C/84E4. Named opcode cases preserve program order; branch
    /// destinations add their relative byte offsets with ushort wrapping. Timed
    /// waits select idle, release, eye-glow and soul phases, and reset masks combine
    /// the statue bit with Busy. Artwork operands, odd and unrelated addresses
    /// return false/zero. Original cartridge words independently verify each mapping.
    /// </summary>
    public bool TryReadMechanicsWord(ushort pointer, out ushort value)
    {
        if (pointer == (ushort)ObjectPointer)
            value = ProgramStart;
        else if (pointer == unchecked((ushort)(ObjectPointer + 2)))
            value = TransferByteCount;
        else if (pointer == unchecked((ushort)(ObjectPointer + 4)))
            value = EncodedVramDestination;
        else
        {
            int offset = pointer - ProgramStart;
            ushort? compiled = offset switch
            {
                0x00 => (ushort)AnimatedTileInstruction.SetTourianStatueAnimationState,
                0x02 => StatueStateBit,
                0x04 => (ushort)AnimatedTileInstruction.GotoIfEventSet,
                0x06 => GreyEventNumber,
                0x08 => At(0x5e),
                0x0a => FirstFrameDuration,
                0x0e or 0x12 or 0x16 => 0x000c,
                0x1a => 0x0010,
                0x1e => (ushort)AnimatedTileInstruction.GotoIfAnyBossBitsSetForArea,
                0x20 => PackedBossTest,
                0x22 => At(0x2c),
                0x24 => (ushort)AnimatedTileInstruction.ResetTourianStatueAnimationState,
                0x26 => StatueStateBit,
                0x28 => (ushort)AnimatedTileInstruction.Goto,
                0x2a => At(0x0e),
                0x2c => (ushort)AnimatedTileInstruction.GotoIfTourianStatueBusy,
                0x2e => At(0x0e),
                0x30 => (ushort)AnimatedTileInstruction.SetTourianStatueAnimationState,
                0x32 => TourianStatueRomData.Busy,
                0x34 => (ushort)AnimatedTileInstruction.ClearThreePaletteColors,
                0x36 => ClearPaletteByteIndex,
                0x38 => 0x0010,
                0x3c => 0x0010,
                0x40 => (ushort)AnimatedTileInstruction.SpawnTourianStatueEyeGlow,
                0x42 => UnlockEffectParameter,
                0x44 => 0x00c0,
                0x48 => (ushort)AnimatedTileInstruction.SpawnTourianStatueSoul,
                0x4a => UnlockEffectParameter,
                0x4c => (ushort)AnimatedTileInstruction.SpawnPaletteFxObject,
                0x4e => PaletteFxDefinition,
                0x50 => 0x0080,
                0x54 => (ushort)AnimatedTileInstruction.SetEvent,
                0x56 => GreyEventNumber,
                0x58 => (ushort)AnimatedTileInstruction.ResetTourianStatueAnimationState,
                0x5a => unchecked((ushort)(TourianStatueRomData.Busy | StatueStateBit)),
                0x5c => (ushort)AnimatedTileInstruction.Delete,
                0x5e => (ushort)AnimatedTileInstruction.ResetTourianStatueAnimationState,
                0x60 => unchecked((ushort)(TourianStatueRomData.Busy | StatueStateBit)),
                0x62 => (ushort)AnimatedTileInstruction.WriteEightTargetPaletteColors,
                0x64 => TargetPaletteByteIndex,
                0x66 => (ushort)AnimatedTileInstruction.Delete,
                _ => null,
            };
            if (offset is < 0 or > 0x66)
            {
                value = 0;
                return false;
            }
            if ((offset & 1) != 0)
            {
                value = 0;
                return false;
            }
            if (!compiled.HasValue)
            {
                value = 0;
                return false;
            }

            value = compiled.Value;
        }

        return true;
    }

    private ushort At(int relativeOffset) =>
        unchecked((ushort)(ProgramStart + relativeOffset));
}
