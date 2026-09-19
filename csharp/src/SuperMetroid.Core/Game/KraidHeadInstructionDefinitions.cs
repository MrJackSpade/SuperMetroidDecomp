namespace SuperMetroid.Core.Game;

using SuperMetroid.Core.Hardware;

/// <summary>
/// Compiled control metadata for Kraid's private bank-$A7 head programs. The selected
/// tilemaps remain cartridge presentation assets; durations, collision-shape selection,
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

    private static readonly KraidHeadInstructionDefinition[] Definitions =
    [
        Frame(0x96d2, 0x000a, 0x97c8, 0x9788, 0xffff),
        Frame(0x96da, 0x000a, 0x9ac8, 0x9790, 0x97b0),
        Frame(0x96e2, 0x000a, 0x9dc8, 0x9798, 0x97b8),
        Sound(0x96ea, KraidHeadInstructionKind.RoarSound, 0x002d),
        Frame(0x96ec, 0x0040, OpenMouthTilemap, 0x97a0, 0x97c0),
        Frame(0x96f4, 0x000a, 0x9dc8, 0x9798, 0x97b8),
        Frame(0x96fc, 0x000a, 0x9ac8, 0x9790, 0x97b0),
        Frame(0x9704, 0x000a, 0x97c8, 0x9788, 0xffff),
        End(0x970c),

        Frame(0x970e, 0x0014, 0x97c8, 0x9788, 0xffff),
        Frame(0x9716, 0x0014, 0x9ac8, 0x9790, 0x97b0),
        Frame(0x971e, 0x0014, 0x9dc8, 0x9798, 0x97b8),
        Sound(0x9726, KraidHeadInstructionKind.RoarSound, 0x002d),
        Frame(0x9728, 0x00c0, OpenMouthTilemap, 0x97a0, 0x97c0),
        Frame(0x9730, 0x0014, 0x9dc8, 0x9798, 0x97b8),
        Frame(0x9738, 0x0014, 0x9ac8, 0x9790, 0x97b0),
        Frame(0x9740, 0x0014, 0x97c8, 0x9788, 0xffff),
        End(0x9748),

        Frame(0x974a, 0x0005, 0x97c8, 0x9788, 0xffff),
        Frame(0x9752, 0x000a, 0x9ac8, 0x9790, 0x97b0),
        Frame(0x975a, 0x0005, 0x97c8, 0x9788, 0xffff),
        End(0x9762),

        Frame(0x9764, 0x0019, 0x97c8, 0x9788, 0xffff),
        Frame(0x976c, 0x0019, 0x9ac8, 0x9790, 0x97b0),
        Frame(0x9774, 0x0019, 0x9dc8, 0x9798, 0x97b8),
        Sound(0x977c, KraidHeadInstructionKind.DyingSound, 0x002e),
        Frame(0x977e, 0x0040, OpenMouthTilemap, 0x97a0, 0x97c0),
        End(0x9786),
    ];

    /// <summary>All 28 aligned command records in native address order.</summary>
    public static ReadOnlySpan<KraidHeadInstructionDefinition> All => Definitions;

    /// <summary>Resolves one exact private-program cursor and rejects adjacent data/code.</summary>
    public static KraidHeadInstructionDefinition Resolve(ushort pointer)
    {
        foreach (KraidHeadInstructionDefinition definition in Definitions)
        {
            if (definition.Pointer == pointer)
                return definition;
        }

        throw new InvalidDataException(
            $"Kraid head instruction $A7:{pointer:X4} is outside the compiled private programs.");
    }

    /// <summary>
    /// Resolves the tilemap word at offset two of a timed frame. Authored upper-ROM
    /// records use the compiled catalog. A restored low-half pointer still aliases live
    /// SNES memory exactly as the cartridge does; the three possible bytes where that
    /// low-half record crosses $7FFF are retained from the bank-$A7 LoROM boundary.
    /// </summary>
    public static ushort ResolveFrameTilemap(ISnesAddressSpace bus, ushort pointer)
    {
        ArgumentNullException.ThrowIfNull(bus);
        foreach (KraidHeadInstructionDefinition definition in Definitions)
        {
            if (definition.Pointer != pointer)
                continue;
            if (definition.Kind != KraidHeadInstructionKind.Frame)
            {
                throw new InvalidDataException(
                    $"Kraid head instruction $A7:{pointer:X4} is not a timed frame.");
            }
            return definition.Tilemap;
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
    /// Ports $A7:AC0B's tilemap-dependent resume selection used when Kraid crosses the
    /// one-eighth-health growth boundary. The returned cursor names the next command,
    /// while the timer retains the current displayed head frame.
    /// </summary>
    public static KraidHeadResumeDefinition GrowthResume(ushort currentTilemap) =>
        currentTilemap switch
        {
            0x97c8 => new(0x970c, RoarEntryTimer),
            0x9ac8 => new(0x9704, RoarEntryTimer),
            0x9dc8 => new(0x96fc, RoarEntryTimer),
            _ => new(0x96f4, 64),
        };

    private static KraidHeadInstructionDefinition Frame(
        ushort pointer,
        ushort duration,
        ushort tilemap,
        ushort vulnerableHitbox,
        ushort invulnerableHitbox) =>
        new(pointer, KraidHeadInstructionKind.Frame, duration, tilemap,
            vulnerableHitbox, invulnerableHitbox, 0);

    private static KraidHeadInstructionDefinition Sound(
        ushort pointer,
        KraidHeadInstructionKind kind,
        ushort soundId) =>
        new(pointer, kind, 0, 0, 0, 0, soundId);

    private static KraidHeadInstructionDefinition End(ushort pointer) =>
        new(pointer, KraidHeadInstructionKind.Terminate, 0, 0, 0, 0, 0);

    private static byte ReadLiveByte(ISnesAddressSpace bus, ushort pointer)
    {
        if (pointer < 0x8000)
            return bus.ReadByte(KraidBackgroundRomData.NativeBank | pointer);

        int boundaryIndex = pointer - 0x8000;
        ReadOnlySpan<byte> boundary = KraidMouthHitboxes.LowHalfBoundaryBytes;
        if ((uint)boundaryIndex < 3)
            return boundary[boundaryIndex];

        throw new InvalidDataException(
            $"Kraid low-half head frame crossed into uncompiled cartridge address $A7:{pointer:X4}.");
    }
}

/// <summary>Mutually exclusive command forms used by Kraid's private head interpreter.</summary>
internal enum KraidHeadInstructionKind
{
    Frame,
    RoarSound,
    DyingSound,
    Terminate,
}

/// <summary>One compiled command from Kraid's four private bank-$A7 head programs.</summary>
internal readonly record struct KraidHeadInstructionDefinition(
    ushort Pointer,
    KraidHeadInstructionKind Kind,
    ushort Duration,
    ushort Tilemap,
    ushort VulnerableHitbox,
    ushort InvulnerableHitbox,
    ushort SoundId);

/// <summary>Kraid head cursor/timer pair selected when the first phase ends.</summary>
internal readonly record struct KraidHeadResumeDefinition(ushort Pointer, ushort Timer);
