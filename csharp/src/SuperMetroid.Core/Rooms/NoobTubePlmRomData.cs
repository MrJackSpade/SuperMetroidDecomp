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
