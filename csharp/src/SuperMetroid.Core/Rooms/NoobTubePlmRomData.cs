using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Rooms;

/// <summary>Cartridge constants used exclusively by Maridia's n00b-tube PLM program.</summary>
public static class NoobTubePlmRomData
{
    /// <summary><c>$84:D4BF</c>: wake the linked list on a newly pressed face button or horizontal direction.</summary>
    public const ushort WakeOnAcceptedInputPreInstruction = 0xd4bf;

    /// <summary><c>$84:BD26</c>: select the linked list only after a power-bomb hit.</summary>
    public const ushort WakeOnPowerBombPreInstruction = 0xbd26;

    /// <summary><c>$84:86D0</c>: inert return installed by ClearPreInstruction.</summary>
    public const ushort InactivePreInstruction = 0x86d0;

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

    /// <summary>FX option bit cleared by instruction $D525 to restore water physics.</summary>
    public const ushort WaterPhysicsDisabledMask = 0x0004;

    /// <summary>Bank-$86 n00b-tube crack projectile definition.</summary>
    public const ushort CrackProjectile = 0xd904;

    /// <summary>Bank-$86 n00b-tube shard projectile definition.</summary>
    public const ushort ShardProjectile = 0xd912;

    /// <summary>Bank-$86 released-air-bubble projectile definition.</summary>
    public const ushort ReleasedAirBubbleProjectile = 0xd920;
}

/// <summary>One native room-graphics enemy-projectile spawn emitted by PLM $D70C.</summary>
public readonly record struct NoobTubeProjectileRequest(
    ushort DefinitionPointer,
    ushort Parameter,
    int PlmBlockIndex);
