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
    private static TourianStatueAnimatedTileProgramDefinition? SelectObject(ushort objectPointer) =>
        objectPointer switch
    {
        AnimatedTileObjectPointers.TourianStatuePhantoon => new(
            objectPointer: AnimatedTileObjectPointers.TourianStatuePhantoon,
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
        AnimatedTileObjectPointers.TourianStatueRidley => new(
            objectPointer: AnimatedTileObjectPointers.TourianStatueRidley,
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
        AnimatedTileObjectPointers.TourianStatueKraid => new(
            objectPointer: AnimatedTileObjectPointers.TourianStatueKraid,
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
        AnimatedTileObjectPointers.TourianStatueDraygon => new(
            objectPointer: AnimatedTileObjectPointers.TourianStatueDraygon,
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
        _ => null,
    };

    /// <summary>The four boss-statue programs in cartridge header order, without a stored roster.</summary>
    public static IEnumerable<TourianStatueAnimatedTileProgramDefinition> All
    {
        get
        {
            yield return SelectObject(AnimatedTileObjectPointers.TourianStatuePhantoon)!;
            yield return SelectObject(AnimatedTileObjectPointers.TourianStatueRidley)!;
            yield return SelectObject(AnimatedTileObjectPointers.TourianStatueKraid)!;
            yield return SelectObject(AnimatedTileObjectPointers.TourianStatueDraygon)!;
        }
    }

    /// <summary>Resolves one stock bank-$87 statue animated-tile object header.</summary>
    public static bool TryResolveObjectHeader(
        ushort objectPointer,
        out TourianStatueAnimatedTileProgramDefinition definition)
    {
        definition = SelectObject(objectPointer)!;
        return definition is not null;
    }
}

/// <summary>One Tourian boss statue's complete mechanics program.</summary>
public sealed class TourianStatueAnimatedTileProgramDefinition
{
    /// <summary>Instruction offsets for the nine artwork operands, calculated from this program's start address.</summary>
    private readonly IReadOnlyList<ushort> sourceOperandPointers;

    /// <summary>Creates the mechanics description associated with one bank-$87 statue header.</summary>
    /// <param name="objectPointer">Address of the animated-tile object header.</param>
    /// <param name="programStart">Address of the first instruction in the statue's program.</param>
    /// <param name="transferByteCount">Number of artwork bytes uploaded by a timed frame.</param>
    /// <param name="encodedVramDestination">Hardware-encoded VRAM destination stored in the header.</param>
    /// <param name="statueStateBit">Bit used by this statue's animation-state instructions.</param>
    /// <param name="greyEventNumber">Event raised when the statue reaches its grey state.</param>
    /// <param name="firstFrameDuration">Duration of the first oscillation frame.</param>
    /// <param name="packedBossTest">Packed area and boss-mask operand used by the program's boss test.</param>
    /// <param name="clearPaletteByteIndex">Palette byte offset at which the program clears three colors.</param>
    /// <param name="unlockEffectParameter">Statue selector passed to the eye-glow and soul effect spawners.</param>
    /// <param name="paletteFxDefinition">Bank-$8D palette-effect definition spawned during release.</param>
    /// <param name="targetPaletteByteIndex">Palette byte offset receiving the shared grey target colors.</param>
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
        sourceOperandPointers = new CalculatedSourceOperands(programStart);
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
    /// <summary>
    /// The nine artwork operands: five four-byte oscillation frames, two four-byte
    /// release frames, then the eye-glow and soul waits. Positions are calculated
    /// from the shared instruction layout, with ushort wrapping and list bounds.
    /// </summary>
    public IReadOnlyList<ushort> SourceOperandPointers => sourceOperandPointers;

    /// <summary>
    /// Provides the nine source-operand addresses without allocating or storing a separate address array.
    /// Each address is derived from the program start and the fixed instruction layout.
    /// </summary>
    /// <param name="programStart">Address of the first instruction in the statue's program.</param>
    private sealed class CalculatedSourceOperands(ushort programStart) : IReadOnlyList<ushort>
    {
        /// <summary>The number of artwork operands in the statue program: five oscillation frames, two release frames, and two waits.</summary>
        public int Count => 9;

        /// <summary>Gets the source address for one artwork operand in program order.</summary>
        /// <param name="index">Zero-based operand position, from zero through <see cref="Count"/> minus one.</param>
        /// <returns>The operand's bank-$87 address, calculated with 16-bit address wrapping.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is outside the operand list.</exception>
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

        /// <summary>Enumerates the calculated operand addresses in the order they occur in the program.</summary>
        /// <returns>An enumerator over the nine source addresses.</returns>
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

    /// <summary>Adds an instruction-relative byte offset to this program's start address.</summary>
    /// <param name="relativeOffset">Byte offset from <see cref="ProgramStart"/>.</param>
    /// <returns>The resulting bank-$87 address, with 16-bit address wrapping.</returns>
    private ushort At(int relativeOffset) =>
        unchecked((ushort)(ProgramStart + relativeOffset));
}
