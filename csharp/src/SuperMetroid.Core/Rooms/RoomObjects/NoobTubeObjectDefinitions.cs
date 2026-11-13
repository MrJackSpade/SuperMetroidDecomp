using SuperMetroid.Core.Input;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Earthquakes;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>What Maridia's n00b tube does: what wakes it, the block it occupies, its sounds, its earthquake and its debris counts.</summary>
public static class NoobTubeObjectDefinitions
{
    /// <summary>The newly pressed buttons that wake the tube once it is cracked: B, Y, Left, Right, A and X.</summary>
    public const ControllerButton AcceptedWakeInputMask =
        ControllerButton.B | ControllerButton.Y | ControllerButton.Left | ControllerButton.Right | ControllerButton.A | ControllerButton.X;

    /// <summary>The collision kind the tube's setup gives its origin block, which keeps what it shows.</summary>
    public const RoomCollisionType OriginCollision = RoomCollisionType.SolidBlock;

    /// <summary>The story event set after the tube has finished breaking.</summary>
    public const StoryEvent BrokenEvent = StoryEvent.MaridiaNoobTubeBroken;

    /// <summary>The sound queued for an ineffective shot.</summary>
    public static readonly SoundEffectId IneffectiveShotSound = EnemySound.DudShotOnDoor;

    /// <summary>The sound queued when the glass breaks.</summary>
    public static readonly SoundEffectId BreakSound = EnemySound.GlassTubeShatter;

    /// <summary>The light diagonal earthquake, shaking both layers, the break starts.</summary>
    public static readonly EarthquakeShake Earthquake =
        new EarthquakeShake(EarthquakeStrength.Light, EarthquakeDirection.Diagonal, EarthquakeReach.ForegroundAndBackground);

    /// <summary>How long the break's earthquake lasts, in frames.</summary>
    public const int EarthquakeFrames = 64;

    /// <summary>Glass shards the break spawns.</summary>
    public const int ShardCount = 10;

    /// <summary>Released-air bubbles the break spawns.</summary>
    public const int BubbleCount = 6;

    /// <summary>How long the first cracked image shows before the tube opens.</summary>
    internal const int CrackedDuration = 48;

    /// <summary>How long the shattered, open tube shows before the room's water is restored.</summary>
    internal const int OpenedDuration = 96;

    /// <summary>The break sound's request priority.</summary>
    internal static readonly SoundRequestPriority BreakSoundPriority = SoundRequestPriority.Normal;

    /// <summary>The intact tube, shown on its first update.</summary>
    internal const NoobTubeDraw IntactImage = NoobTubeDraw.Intact;

    /// <summary>The tube's cracking, image by image, before it shatters.</summary>
    internal static IReadOnlyList<TerrainChangeFrame> Cracking { get; } =
    [
        new(RoomObjectDraw.Of(NoobTubeDraw.Damaged), CrackedDuration),
        new(RoomObjectDraw.Of(NoobTubeDraw.OpenedRows), 1),
        new(RoomObjectDraw.Of(NoobTubeDraw.BrokenFull), 1),
    ];

    /// <summary>The shattered, open tube, shown with the break sound, the debris and the earthquake.</summary>
    internal static TerrainChangeFrame Shattered { get; } = new(RoomObjectDraw.Of(NoobTubeDraw.Opened), OpenedDuration);
}
