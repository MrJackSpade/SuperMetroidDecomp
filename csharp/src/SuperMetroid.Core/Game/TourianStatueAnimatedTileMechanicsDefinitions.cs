namespace SuperMetroid.Core.Game;

/// <summary>
/// Immutable engine-owned headers and control programs for the four Tourian entrance
/// statue animated-tile objects in bank <c>$87</c>.
/// </summary>
/// <remarks>
/// The nine frame-source operands in each program remain cartridge-backed presentation
/// references. This catalog owns the instruction graph, timing, event routing, boss-bit
/// tests, palette destinations, and effect parameters that make the sequence operate.
/// </remarks>
public static class TourianStatueAnimatedTileMechanicsDefinitions
{
    private static readonly TourianStatueAnimatedTileProgramDefinition[] Definitions =
    [
        new(
            objectPointer: 0x854c,
            programStart: 0x83ac,
            transferByteCount: 0x0080,
            encodedVramDestination: 0x7800,
            statueStateBit: 0x0001,
            greyEventNumber: 0x0006,
            firstFrameDuration: 0x0006,
            packedBossTest: 0x0301,
            clearPaletteByteIndex: 0x0158,
            unlockEffectParameter: 0x0000,
            paletteFxDefinition: 0xf755,
            targetPaletteByteIndex: 0x0140),
        new(
            objectPointer: 0x8552,
            programStart: 0x8414,
            transferByteCount: 0x0040,
            encodedVramDestination: 0x7220,
            statueStateBit: 0x0002,
            greyEventNumber: 0x0007,
            firstFrameDuration: 0x000a,
            packedBossTest: 0x0201,
            clearPaletteByteIndex: 0x0132,
            unlockEffectParameter: 0x0002,
            paletteFxDefinition: 0xf751,
            targetPaletteByteIndex: 0x0120),
        new(
            objectPointer: 0x8558,
            programStart: 0x847c,
            transferByteCount: 0x0040,
            encodedVramDestination: 0x0b40,
            statueStateBit: 0x0004,
            greyEventNumber: 0x0009,
            firstFrameDuration: 0x0004,
            packedBossTest: 0x0101,
            clearPaletteByteIndex: 0x00f8,
            unlockEffectParameter: 0x0006,
            paletteFxDefinition: 0xf74d,
            targetPaletteByteIndex: 0x00e0),
        new(
            objectPointer: 0x855e,
            programStart: 0x84e4,
            transferByteCount: 0x0080,
            encodedVramDestination: 0x0ca0,
            statueStateBit: 0x0008,
            greyEventNumber: 0x0008,
            firstFrameDuration: 0x0008,
            packedBossTest: 0x0401,
            clearPaletteByteIndex: 0x00d2,
            unlockEffectParameter: 0x0004,
            paletteFxDefinition: 0xf749,
            targetPaletteByteIndex: 0x00c0),
    ];
    private static readonly IReadOnlyList<TourianStatueAnimatedTileProgramDefinition>
        ReadOnlyDefinitions = Array.AsReadOnly(Definitions);

    /// <summary>The four boss-statue programs in cartridge header order.</summary>
    public static IReadOnlyList<TourianStatueAnimatedTileProgramDefinition> All =>
        ReadOnlyDefinitions;

    /// <summary>Resolves one stock bank-$87 statue animated-tile object header.</summary>
    public static bool TryResolveObjectHeader(
        ushort objectPointer,
        out TourianStatueAnimatedTileProgramDefinition definition)
    {
        foreach (TourianStatueAnimatedTileProgramDefinition candidate in Definitions)
        {
            if (candidate.ObjectPointer != objectPointer)
                continue;

            definition = candidate;
            return true;
        }

        definition = null!;
        return false;
    }
}

/// <summary>One Tourian boss statue's complete mechanics program.</summary>
public sealed class TourianStatueAnimatedTileProgramDefinition
{
    private static readonly ushort[] SourceOperandOffsets =
        [0x0c, 0x10, 0x14, 0x18, 0x1c, 0x3a, 0x3e, 0x46, 0x52];
    private readonly ushort[] sourceOperandPointers;
    private readonly IReadOnlyList<ushort> readOnlySourceOperandPointers;

    internal TourianStatueAnimatedTileProgramDefinition(
        ushort objectPointer,
        ushort programStart,
        ushort transferByteCount,
        ushort encodedVramDestination,
        ushort statueStateBit,
        ushort greyEventNumber,
        ushort firstFrameDuration,
        ushort packedBossTest,
        ushort clearPaletteByteIndex,
        ushort unlockEffectParameter,
        ushort paletteFxDefinition,
        ushort targetPaletteByteIndex)
    {
        ObjectPointer = objectPointer;
        ProgramStart = programStart;
        TransferByteCount = transferByteCount;
        EncodedVramDestination = encodedVramDestination;
        StatueStateBit = statueStateBit;
        GreyEventNumber = greyEventNumber;
        FirstFrameDuration = firstFrameDuration;
        PackedBossTest = packedBossTest;
        ClearPaletteByteIndex = clearPaletteByteIndex;
        UnlockEffectParameter = unlockEffectParameter;
        PaletteFxDefinition = paletteFxDefinition;
        TargetPaletteByteIndex = targetPaletteByteIndex;
        sourceOperandPointers = SourceOperandOffsets
            .Select(offset => unchecked((ushort)(programStart + offset)))
            .ToArray();
        readOnlySourceOperandPointers = Array.AsReadOnly(sourceOperandPointers);
    }

    /// <summary>The animated-tile object header address in bank $87.</summary>
    public ushort ObjectPointer { get; }
    /// <summary>The first instruction of the object's fixed 104-byte program.</summary>
    public ushort ProgramStart { get; }
    /// <summary>The byte count uploaded by each timed frame.</summary>
    public ushort TransferByteCount { get; }
    /// <summary>The hardware-encoded VRAM destination for each timed frame.</summary>
    public ushort EncodedVramDestination { get; }
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
    /// <summary>The nine live artwork-pointer operands excluded from mechanics ownership.</summary>
    public IReadOnlyList<ushort> SourceOperandPointers => readOnlySourceOperandPointers;

    /// <summary>Reads one immutable mechanics word, excluding frame source operands.</summary>
    public bool TryReadMechanicsWord(ushort pointer, out ushort value)
    {
        if (pointer == ObjectPointer)
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
                0x00 => AnimatedTileInstructionCodes.SetTourianStatueAnimationState,
                0x02 => StatueStateBit,
                0x04 => AnimatedTileInstructionCodes.GotoIfEventSet,
                0x06 => GreyEventNumber,
                0x08 => At(0x5e),
                0x0a => FirstFrameDuration,
                0x0e or 0x12 or 0x16 => 0x000c,
                0x1a => 0x0010,
                0x1e => AnimatedTileInstructionCodes.GotoIfAnyBossBitsSetForArea,
                0x20 => PackedBossTest,
                0x22 => At(0x2c),
                0x24 => AnimatedTileInstructionCodes.ResetTourianStatueAnimationState,
                0x26 => StatueStateBit,
                0x28 => AnimatedTileInstructionCodes.Goto,
                0x2a => At(0x0e),
                0x2c => AnimatedTileInstructionCodes.GotoIfTourianStatueBusy,
                0x2e => At(0x0e),
                0x30 => AnimatedTileInstructionCodes.SetTourianStatueAnimationState,
                0x32 => TourianStatueRomData.Busy,
                0x34 => AnimatedTileInstructionCodes.ClearThreePaletteColors,
                0x36 => ClearPaletteByteIndex,
                0x38 => 0x0010,
                0x3c => 0x0010,
                0x40 => AnimatedTileInstructionCodes.SpawnTourianStatueEyeGlow,
                0x42 => UnlockEffectParameter,
                0x44 => 0x00c0,
                0x48 => AnimatedTileInstructionCodes.SpawnTourianStatueSoul,
                0x4a => UnlockEffectParameter,
                0x4c => AnimatedTileInstructionCodes.SpawnPaletteFxObject,
                0x4e => PaletteFxDefinition,
                0x50 => 0x0080,
                0x54 => AnimatedTileInstructionCodes.SetEvent,
                0x56 => GreyEventNumber,
                0x58 => AnimatedTileInstructionCodes.ResetTourianStatueAnimationState,
                0x5a => unchecked((ushort)(TourianStatueRomData.Busy | StatueStateBit)),
                0x5c => AnimatedTileInstructionCodes.Delete,
                0x5e => AnimatedTileInstructionCodes.ResetTourianStatueAnimationState,
                0x60 => unchecked((ushort)(TourianStatueRomData.Busy | StatueStateBit)),
                0x62 => AnimatedTileInstructionCodes.WriteEightTargetPaletteColors,
                0x64 => TargetPaletteByteIndex,
                0x66 => AnimatedTileInstructionCodes.Delete,
                _ => null,
            };
            if (offset is < 0 or > 0x66 || SourceOperandOffsets.Contains((ushort)offset))
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
