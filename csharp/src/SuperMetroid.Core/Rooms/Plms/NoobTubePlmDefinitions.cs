using SuperMetroid.Core.Input;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>What Maridia's n00b tube does: what wakes it, the block it occupies, its sounds, its earthquake and its debris counts.</summary>
public static class NoobTubePlmDefinitions
{
    /// <summary>The newly pressed buttons that wake the tube once it is cracked: B, Y, Left, Right, A and X.</summary>
    public const ControllerButton AcceptedWakeInputMask =
        ControllerButton.B | ControllerButton.Y | ControllerButton.Left | ControllerButton.Right | ControllerButton.A | ControllerButton.X;

    /// <summary>The tube's origin block: a solid block showing visual block $44.</summary>
    public static RoomLevelWord SolidProjectileTriggerLevelWord { get; } =
        RoomLevelWord.Create(0x44, LevelBlockFlipFlags.None, RoomCollisionType.SolidBlock);

    /// <summary>The story event set after the tube has finished breaking.</summary>
    public const EventNumber BrokenEvent = EventNumber.MaridiaNoobTubeBroken;

    /// <summary>The sound queued for an ineffective shot.</summary>
    public static readonly SoundEffectId IneffectiveShotSound = Library2Sound.DudShotOnDoor;

    /// <summary>The sound queued when the glass breaks.</summary>
    public static readonly SoundEffectId BreakSound = Library2Sound.GlassTubeShatter;

    /// <summary>The kind of earthquake the break starts.</summary>
    public const ushort EarthquakeType = 11;

    /// <summary>How long the break's earthquake lasts, in frames.</summary>
    public const ushort EarthquakeTimer = 64;

    /// <summary>Glass shards the break spawns (parameters 0, 2, .. 18).</summary>
    public const int ShardCount = 10;

    /// <summary>Released-air bubbles the break spawns (parameters 0, 2, .. 10).</summary>
    public const int BubbleCount = 6;
}
