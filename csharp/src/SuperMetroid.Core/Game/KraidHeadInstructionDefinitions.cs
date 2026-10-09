namespace SuperMetroid.Core.Game;

using SuperMetroid.Core.Hardware;

/// <summary>
/// Compiled control metadata for Kraid's private bank-$A7 head programs. The selected
/// tilemaps come from installed presentation assets; durations, collision-shape selection,
/// sound callbacks, and program flow are immutable gameplay definitions.
/// </summary>
internal static class KraidHeadInstructionDefinitions
{
    /// <summary>$A7:96D2, first timed frame of <c>InstList_Kraid_Roar</c>.</summary>
    public const ushort RoarInitial = 0x96d2;

    /// <summary>$A7:96DA, continuation installed after the roar entry frame.</summary>
    public const ushort RoarContinuation = 0x96da;

    /// <summary>$A7:974A, first timed frame of <c>InstList_Kraid_EyeGlowing</c>.</summary>
    public const ushort EyeGlowInitial = 0x974a;

    /// <summary>$A7:9752, continuation installed after the eye-glow entry frame.</summary>
    public const ushort EyeGlowContinuation = 0x9752;

    /// <summary>$A7:9764, first timed frame of <c>InstList_Kraid_Dying</c>.</summary>
    public const ushort DeathInitial = 0x9764;

    /// <summary>$A7:976C, continuation installed after the dying entry frame.</summary>
    public const ushort DeathContinuation = 0x976c;

    /// <summary>$A7:A0C8, the fully open mouth tilemap used by the rock emitter.</summary>
    public const ushort OpenMouthTilemap = 0xa0c8;

    /// <summary>Timer from the first roar record at $A7:96D2.</summary>
    public const ushort RoarEntryTimer = 10;

    /// <summary>Timer from the first eye-glow record at $A7:974A.</summary>
    public const ushort EyeGlowEntryTimer = 5;

    /// <summary>Timer from the first dying record at $A7:9764.</summary>
    public const ushort DeathEntryTimer = 25;

    /// <summary>$A7:970E, InstList_Kraid_DyingRoar_0, the slower complete roar.</summary>
    private const ushort DyingRoarInitial = 0x970e;
    /// <summary>$A7:97C8, Tilemap_KraidHead_0; successive mouth stages occupy $300 bytes.</summary>
    private const ushort ClosedMouthTilemap = 0x97c8;
    /// <summary>$A7:9788, Hitbox_KraidMouth_0; four eight-byte vulnerable mouth shapes.</summary>
    private const ushort VulnerableHitboxStart = 0x9788;
    /// <summary>$A7:97B0, Hitbox_KraidMouth_5; first open-stage invulnerable shape.</summary>
    private const ushort InvulnerableHitboxStart = 0x97b0;

    /// <summary>All 28 command records, calculated in native address order.</summary>
    public static KraidHeadCommandSequence All => new(28);

    /// <summary>Calculates one command from its roar, glow, or death program role.</summary>
    /// <remarks>
    /// Independently checked for #1165 against pinned bank_A7.asm and every original
    /// NTSC J/U v1.0 command. Roars open through stages 0..3 and close through 2..0;
    /// death stops after opening, while glow uses stages 0,1,0. A two-byte sound
    /// command precedes the fully open frame. Timed frames occupy eight bytes.
    /// Tilemaps advance by $300 per stage and hitboxes by eight bytes; the closed
    /// mouth has no invulnerable hitbox. No generated command cache is stored.
    /// </remarks>
    internal static KraidHeadInstructionDefinition Command(int index)
    {
        if ((uint)index >= 28) throw new IndexOutOfRangeException();
        if (index < 18)
        {
            bool dyingRoar = index >= 9;
            return OpeningCommand(dyingRoar ? DyingRoarInitial : RoarInitial,
                index % 9, false, (ushort)(dyingRoar ? 20 : RoarEntryTimer),
                (ushort)(dyingRoar ? 192 : 64));
        }
        if (index < 22)
        {
            int step = index - 18;
            ushort pointer = (ushort)(EyeGlowInitial + 8 * step);
            if (step == 3) return End(pointer);
            int stage = 1 - Math.Abs(step - 1);
            return MouthFrame(pointer, (ushort)(EyeGlowEntryTimer * (stage + 1)), stage);
        }
        return OpeningCommand(DeathInitial, index - 22, true, DeathEntryTimer, 64);
    }

    /// <summary>Builds a timed mouth frame, roar sound, or terminator from one command position in a roar/death program.</summary>
    /// <param name="start">Address of the program's first timed frame.</param>
    /// <param name="step">Zero-based command step, including the compact two-byte sound slot.</param>
    /// <param name="death">Selects dying-roar timing, sound, and program termination rules.</param>
    /// <param name="frameTimer">Duration assigned to ordinary mouth frames.</param>
    /// <param name="openTimer">Duration assigned to the fully open mouth frame.</param>
    /// <returns>The compiled instruction at the address computed for this step.</returns>
    private static KraidHeadInstructionDefinition OpeningCommand(
        ushort start, int step, bool death, ushort frameTimer, ushort openTimer)
    {
        // The sound occupies one command slot but only two bytes, unlike a frame.
        ushort pointer = (ushort)(start + 8 * step - (step > 3 ? 6 : 0));
        if (step == (death ? 5 : 8)) return End(pointer);
        if (step == 3)
            return Sound(pointer, death ? KraidHeadInstructionKind.DyingSound
                : KraidHeadInstructionKind.RoarSound, (ushort)(death ? 0x2e : 0x2d));
        int frame = step < 3 ? step : step - 1;
        int stage = frame <= 3 ? frame : 6 - frame;
        return MouthFrame(pointer, stage == 3 ? openTimer : frameTimer, stage);
    }

    /// <summary>Constructs one mouth-frame command from its stage's tilemap and collision shapes.</summary>
    /// <param name="pointer">Address of the timed frame record in bank $A7.</param>
    /// <param name="duration">Number of interpreter updates for which the frame remains active.</param>
    /// <param name="stage">Zero-based mouth opening stage.</param>
    /// <returns>A frame definition with vulnerable and, for open stages, invulnerable hitboxes.</returns>
    private static KraidHeadInstructionDefinition MouthFrame(ushort pointer, ushort duration, int stage) =>
        Frame(pointer, duration, (ushort)(ClosedMouthTilemap + 0x300 * stage),
            (ushort)(VulnerableHitboxStart + 8 * stage),
            stage == 0 ? ushort.MaxValue : (ushort)(InvulnerableHitboxStart + 8 * (stage - 1)));

    /// <summary>Resolves one exact private-program cursor and rejects adjacent data/code.</summary>
    public static KraidHeadInstructionDefinition Resolve(ushort pointer)
    {
        foreach (KraidHeadInstructionDefinition definition in All)
        {
            if (definition.Pointer == pointer)
                return definition;
        }

        throw new InvalidDataException(
            $"Kraid head instruction $A7:{pointer:X4} is outside the compiled private programs.");
    }

    /// <summary>
    /// $A7:AFB7-AFBF / B19C-B1A4: collision reads the hitbox word four or two
    /// bytes before the live next-instruction cursor, even when no frame ran yet.
    /// </summary>
    public static ushort ReadCollisionHitbox(ISnesAddressSpace bus, ushort nextInstruction, bool innerMouth)
    {
        ushort address = unchecked((ushort)(nextInstruction - (innerMouth ? 2 : 4)));
        foreach (KraidHeadInstructionDefinition frame in All)
        {
            if (frame.Kind != KraidHeadInstructionKind.Frame)
                continue;
            if (address == frame.Pointer + 4) return frame.VulnerableHitbox;
            if (address == frame.Pointer + 6) return frame.InvulnerableHitbox;
        }
        if (address < 0x8000)
            return (ushort)(ReadLiveByte(bus, address) |
                ReadLiveByte(bus, unchecked((ushort)(address + 1))) << 8);
        throw new InvalidDataException($"Kraid collision selector $A7:{address:X4} is outside the compiled head frames.");
    }

    /// <summary>
    /// Reads the word at cursor + 2 used by HandleKraidPhase1 at $A7:C026.
    /// This is a tilemap only for timed frames; sound/terminal cursors read the next
    /// record's first word without executing it. Authored upper-ROM records use the
    /// compiled catalog. A restored low-half pointer still aliases live
    /// SNES memory exactly as the cartridge does; the three possible bytes where that
    /// low-half record crosses $7FFF are retained from the bank-$A7 LoROM boundary.
    /// </summary>
    public static ushort ReadGrowthSelectionWord(ISnesAddressSpace bus, ushort pointer)
    {
        ArgumentNullException.ThrowIfNull(bus);
        foreach (KraidHeadInstructionDefinition definition in All)
        {
            if (definition.Pointer != pointer)
                continue;
            if (definition.Kind == KraidHeadInstructionKind.Frame)
                return definition.Tilemap;

            // The final terminator borders the mouth geometry; all other sound
            // and terminal records border a timed frame. Preserve the raw read,
            // including the duration which triggers the native quick-kill delay.
            ushort following = unchecked((ushort)(pointer + 2));
            return KraidMouthHitboxes.IsDefined(following)
                ? unchecked((ushort)KraidMouthHitboxes.Resolve(following).Left)
                : Resolve(following).Duration;
        }

        if (pointer >= 0x8000)
        {
            throw new InvalidDataException(
                $"Kraid head frame $A7:{pointer:X4} is outside the compiled private programs " +
                "and is not a live bank-$A7 low-half alias.");
        }

        ushort tilemapPointer = unchecked((ushort)(pointer + 2));
        return unchecked((ushort)(
            ReadLiveByte(bus, tilemapPointer) |
            ReadLiveByte(bus, unchecked((ushort)(tilemapPointer + 1))) << 8));
    }

    /// <summary>
    /// Ports $A7:C029-$C04F's raw-word-dependent resume selection when Kraid crosses the
    /// seven-eighths-health growth boundary. The returned cursor names the next command,
    /// while the timer retains the current displayed head frame.
    /// </summary>
    public static KraidHeadResumeDefinition GrowthResume(ushort selectionWord) =>
        selectionWord switch
        {
            0x97c8 => new(0x970c, RoarEntryTimer),
            0x9ac8 => new(0x9704, RoarEntryTimer),
            0x9dc8 => new(0x96fc, RoarEntryTimer),
            _ => new(0x96f4, 64),
        };

    /// <summary>Creates a timed frame record for the compiled instruction catalog.</summary>
    /// <param name="pointer">Native bank-$A7 address of the record.</param>
    /// <param name="duration">Interpreter duration for the displayed frame.</param>
    /// <param name="tilemap">ROM tilemap pointer selected for the mouth stage.</param>
    /// <param name="vulnerableHitbox">Hitbox active while Kraid's mouth can be damaged.</param>
    /// <param name="invulnerableHitbox">Hitbox active while the mouth is not vulnerable, or the native sentinel for no such shape.</param>
    /// <returns>A frame-kind instruction definition.</returns>
    private static KraidHeadInstructionDefinition Frame(
        ushort pointer,
        ushort duration,
        ushort tilemap,
        ushort vulnerableHitbox,
        ushort invulnerableHitbox) =>
        new(pointer, KraidHeadInstructionKind.Frame, duration, tilemap,
            vulnerableHitbox, invulnerableHitbox, 0);

    /// <summary>Creates the compact sound command placed between timed mouth frames.</summary>
    /// <param name="pointer">Native bank-$A7 address of the two-byte command.</param>
    /// <param name="kind">Whether the sound is the ordinary roar or dying roar.</param>
    /// <param name="soundId">Sound effect identifier consumed by the Kraid interpreter.</param>
    /// <returns>A sound-kind definition with no frame timing or graphics data.</returns>
    private static KraidHeadInstructionDefinition Sound(
        ushort pointer,
        KraidHeadInstructionKind kind,
        ushort soundId) =>
        new(pointer, kind, 0, 0, 0, 0, soundId);

    /// <summary>Creates the terminal command that ends a private head instruction program.</summary>
    /// <param name="pointer">Native bank-$A7 address of the terminator.</param>
    /// <returns>A terminate-kind definition with no frame or sound payload.</returns>
    private static KraidHeadInstructionDefinition End(ushort pointer) =>
        new(pointer, KraidHeadInstructionKind.Terminate, 0, 0, 0, 0, 0);

    /// <summary>Reads an aliased low-half byte from WRAM or the preserved bytes at the bank-$A7 mirror boundary.</summary>
    /// <param name="bus">Address space providing mutable WRAM access for mirrored addresses.</param>
    /// <param name="pointer">CPU address in the low half of bank $A7, including its three mapped boundary bytes.</param>
    /// <returns>The byte exposed at that aliased address.</returns>
    /// <exception cref="InvalidOperationException">The bus does not expose WRAM access for a low-half alias.</exception>
    /// <exception cref="InvalidDataException">The address lies beyond the supported mirror boundary.</exception>
    private static byte ReadLiveByte(ISnesAddressSpace bus, ushort pointer)
    {
        if (pointer < 0x8000)
        {
            // Bank $A7's low system window mirrors WRAM, not cartridge data. The
            // typed reader also rejects the unmapped expansion range instead of
            // accepting a synthetic value from an overly permissive fake bus.
            ISnesMutableMemory memory = bus as ISnesMutableMemory ??
                throw new InvalidOperationException(
                    "Kraid's live low-half head alias requires WRAM access.");
            return memory.ReadWorkRamByte(KraidBackgroundRomData.NativeBank | pointer);
        }

        int boundaryIndex = pointer - 0x8000;
        if ((uint)boundaryIndex < 3)
            return KraidMouthHitboxes.LowHalfBoundaryByte(boundaryIndex);

        throw new InvalidDataException(
            $"Kraid low-half head frame crossed into uncompiled cartridge address $A7:{pointer:X4}.");
    }
}

/// <summary>Mutually exclusive command forms used by Kraid's private head interpreter.</summary>
internal enum KraidHeadInstructionKind
{
    /// <summary>A timed mouth image with collision-shape metadata.</summary>
    Frame,
    /// <summary>The short sound command used during Kraid's ordinary roar.</summary>
    RoarSound,
    /// <summary>The short sound command used during Kraid's dying roar.</summary>
    DyingSound,
    /// <summary>A program terminator that stops head instruction processing.</summary>
    Terminate,
}

/// <summary>One compiled command from Kraid's four private bank-$A7 head programs.</summary>
/// <param name="Pointer">Native address identifying the command and its position in the program.</param>
/// <param name="Kind">Interpretation of the payload, such as a timed frame, sound, or terminator.</param>
/// <param name="Duration">Frame lifetime in AI updates; sound and terminal commands use zero.</param>
/// <param name="Tilemap">Mouth graphics source for frame commands; other command kinds use zero.</param>
/// <param name="VulnerableHitbox">Damageable mouth shape for frame commands.</param>
/// <param name="InvulnerableHitbox">Non-damageable mouth shape, or <see cref="ushort.MaxValue"/> when absent.</param>
/// <param name="SoundId">Sound effect identifier for sound commands; other kinds use zero.</param>
internal readonly record struct KraidHeadInstructionDefinition(
    ushort Pointer,
    KraidHeadInstructionKind Kind,
    ushort Duration,
    ushort Tilemap,
    ushort VulnerableHitbox,
    ushort InvulnerableHitbox,
    ushort SoundId);

/// <summary>Kraid head cursor/timer pair selected when the first phase ends.</summary>
/// <param name="Pointer">Next instruction cursor installed when the first phase resumes.</param>
/// <param name="Timer">Delay retained for the currently displayed head frame.</param>
internal readonly record struct KraidHeadResumeDefinition(ushort Pointer, ushort Timer);

/// <summary>Calculated native head commands with no stored lookup or startup cache.</summary>
/// <param name="Length">Number of command records addressable in native program order.</param>
internal readonly record struct KraidHeadCommandSequence(int Length)
{
    /// <summary>Gets the calculated command at a zero-based native-order index.</summary>
    /// <param name="index">Index in the combined roar, dying-roar, glow, and death command sequence.</param>
    /// <returns>The command calculated for that position.</returns>
    /// <exception cref="IndexOutOfRangeException">The index is outside the sequence.</exception>
    public KraidHeadInstructionDefinition this[int index] => KraidHeadInstructionDefinitions.Command(index);

    /// <summary>Materializes the calculated commands in native address order.</summary>
    /// <returns>A new array containing each command in the sequence.</returns>
    public KraidHeadInstructionDefinition[] ToArray()
    {
        var result = new KraidHeadInstructionDefinition[Length];
        for (int index = 0; index < result.Length; index++) result[index] = this[index];
        return result;
    }
    /// <summary>Creates a value enumerator that calculates commands as they are visited.</summary>
    /// <returns>An enumerator starting before the first command.</returns>
    public Enumerator GetEnumerator() => new(this);

    /// <summary>Iterates the calculated command sequence without storing a command cache.</summary>
    /// <param name="sequence">The sequence whose length and indexed command calculation drive iteration.</param>
    public struct Enumerator(KraidHeadCommandSequence sequence)
    {
        /// <summary>Index of the next command to expose, advanced by <see cref="MoveNext"/>.</summary>
        private int next;

        /// <summary>Advances to the next command when the sequence has not ended.</summary>
        /// <returns><see langword="true"/> when <see cref="Current"/> names a valid command.</returns>
        public bool MoveNext() => next++ < sequence.Length;

        /// <summary>Gets the command selected by the most recent successful <see cref="MoveNext"/> call.</summary>
        public KraidHeadInstructionDefinition Current => KraidHeadInstructionDefinitions.Command(next - 1);
    }
}
