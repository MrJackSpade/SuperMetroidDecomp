using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Rooms;

/// <summary>Cartridge constants used exclusively by Maridia's n00b-tube PLM program.</summary>
public static class NoobTubePlmRomData
{
    /// <summary>Native controller-new-input mask used by <c>$84:D4BF</c>.</summary>
    public const ushort AcceptedWakeInputMask = 0xc3c0;

    /// <summary>Solid collision type eight plus resident projectile-trigger BTS $44.</summary>
    public const ushort SolidProjectileTriggerLevelWord = 0x8044;

    /// <summary>Story event $0B set after the tube has finished breaking.</summary>
    public const EventNumber BrokenEvent = EventNumber.MaridiaNoobTubeBroken;

    /// <summary>Sound-library-two ID queued for an ineffective shot.</summary>
    public const byte IneffectiveShotSound = 0x57;

    /// <summary>Sound-library-two ID queued when the glass breaks.</summary>
    public const byte BreakSound = 0x1a;

    /// <summary>Native earthquake type written by instruction $D536.</summary>
    public const ushort EarthquakeType = 0x000b;

    /// <summary>Native earthquake duration written by instruction $D536.</summary>
    public const ushort EarthquakeTimer = 0x0040;
}

/// <summary>Bank-$84 pre-instructions the n00b-tube PLM installs.</summary>
public enum NoobTubePlmPreInstruction : ushort
{
    /// <summary><c>$84:D4BF</c>: wake the linked list on a newly pressed face button or horizontal direction.</summary>
    WakeOnAcceptedInput = 0xd4bf,

    /// <summary><c>$84:BD26</c>: select the linked list only after a power-bomb hit.</summary>
    WakeOnPowerBomb = 0xbd26,

    /// <summary><c>$84:86D0</c>: inert return installed by ClearPreInstruction.</summary>
    Inactive = 0x86d0,
}

/// <summary>Bank-$86 enemy-projectile definitions the n00b-tube PLM spawns.</summary>
public enum NoobTubeProjectileDefinition : ushort
{
    /// <summary>Bank-$86 n00b-tube crack projectile definition.</summary>
    Crack = 0xd904,

    /// <summary>Bank-$86 n00b-tube shard projectile definition.</summary>
    Shard = 0xd912,

    /// <summary>Bank-$86 released-air-bubble projectile definition.</summary>
    ReleasedAirBubble = 0xd920,
}

/// <summary>One native room-graphics enemy-projectile spawn emitted by PLM $D70C.</summary>
public readonly record struct NoobTubeProjectileRequest(
    NoobTubeProjectileDefinition DefinitionPointer,
    ushort Parameter,
    int PlmBlockIndex);
