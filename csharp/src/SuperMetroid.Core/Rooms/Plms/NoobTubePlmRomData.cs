using SuperMetroid.Core.Input;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>Cartridge constants used exclusively by Maridia's n00b-tube PLM program.</summary>
public static class NoobTubePlmRomData
{
    /// <summary>Native controller-new-input mask <c>$C3C0</c> used by <c>$84:D4BF</c>: B, Y, Left, Right, A and X.</summary>
    public const SnesButton AcceptedWakeInputMask =
        SnesButton.B | SnesButton.Y | SnesButton.Left | SnesButton.Right | SnesButton.A | SnesButton.X;

    /// <summary>Solid collision type eight plus resident projectile-trigger BTS $44.</summary>
    public const ushort SolidProjectileTriggerLevelWord = 0x8044;

    /// <summary>Story event $0B set after the tube has finished breaking.</summary>
    public const EventNumber BrokenEvent = EventNumber.MaridiaNoobTubeBroken;

    /// <summary>Sound-library-two ID queued for an ineffective shot.</summary>
    public static readonly SoundEffectId IneffectiveShotSound = Library2Sound.DudShotOnDoor;

    /// <summary>Sound-library-two ID queued when the glass breaks.</summary>
    public static readonly SoundEffectId BreakSound = Library2Sound.GlassTubeShatter;

    /// <summary>Native earthquake type written by instruction $D536.</summary>
    public const ushort EarthquakeType = 0x000b;

    /// <summary>Native earthquake duration written by instruction $D536.</summary>
    public const ushort EarthquakeTimer = 0x0040;

    /// <summary>Glass shards the break spawns (parameters 0, 2, .. 18).</summary>
    public const int ShardCount = 10;

    /// <summary>Released-air bubbles the break spawns (parameters 0, 2, .. 10).</summary>
    public const int BubbleCount = 6;
}
