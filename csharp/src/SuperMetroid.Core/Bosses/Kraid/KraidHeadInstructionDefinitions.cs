namespace SuperMetroid.Core.Bosses.Kraid;

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

    /// <summary>Calculates one opening or closing roar frame, embedded sound command, or terminal cursor.</summary>
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

    /// <summary>Derives a mouth stage's tilemap and vulnerable/invulnerable hitbox pointers from native strides.</summary>
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
    public static ushort ReadCollisionHitbox(ushort nextInstruction, bool innerMouth)
    {
        ushort address = unchecked((ushort)(nextInstruction - (innerMouth ? 2 : 4)));
        foreach (KraidHeadInstructionDefinition frame in All)
        {
            if (frame.Kind != KraidHeadInstructionKind.Frame)
                continue;
            if (address == frame.Pointer + 4) return frame.VulnerableHitbox;
            if (address == frame.Pointer + 6) return frame.InvulnerableHitbox;
        }
        throw new InvalidDataException($"Kraid collision selector $A7:{address:X4} is outside the compiled head frames.");
    }

    /// <summary>
    /// Reads the word at cursor + 2 used by HandleKraidPhase1 at $A7:C026.
    /// This is a tilemap only for timed frames; sound/terminal cursors read the next
    /// record's first word without executing it. Every authored record is compiled; any
    /// other cursor is corrupt state.
    /// </summary>
    public static ushort ReadGrowthSelectionWord(ISnesAddressSpace bus, ushort pointer)
    {
        Ensure.NotNull(bus);
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
            return KraidMouthHitboxDefinitions.IsDefined(following)
                ? unchecked((ushort)KraidMouthHitboxDefinitions.Resolve(following).Left)
                : Resolve(following).Duration;
        }

        throw new InvalidDataException(
            $"Kraid head frame $A7:{pointer:X4} is outside the compiled private programs.");
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

    /// <summary>Packages one timed head frame with its selected tilemap and two collision shapes.</summary>
    private static KraidHeadInstructionDefinition Frame(
        ushort pointer,
        ushort duration,
        ushort tilemap,
        ushort vulnerableHitbox,
        ushort invulnerableHitbox) =>
        new(pointer, KraidHeadInstructionKind.Frame, duration, tilemap,
            vulnerableHitbox, invulnerableHitbox, 0);

    /// <summary>Packages a two-byte roar command without frame duration or collision operands.</summary>
    private static KraidHeadInstructionDefinition Sound(
        ushort pointer,
        KraidHeadInstructionKind kind,
        ushort soundId) =>
        new(pointer, kind, 0, 0, 0, 0, soundId);

    /// <summary>Marks the end cursor of a private head program.</summary>
    private static KraidHeadInstructionDefinition End(ushort pointer) =>
        new(pointer, KraidHeadInstructionKind.Terminate, 0, 0, 0, 0, 0);


}
