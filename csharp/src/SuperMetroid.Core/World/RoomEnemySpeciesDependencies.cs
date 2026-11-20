using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Bosses;
using SuperMetroid.Core.Bosses.Artwork;
using SuperMetroid.Core.Compatibility;
using SuperMetroid.Core.Enemies.Beetom;
using SuperMetroid.Core.Enemies.Bull;
using SuperMetroid.Core.Enemies.EffectActors;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms.Scrolling;
using SuperMetroid.Core.Samus.Movement;

namespace SuperMetroid.Core.World;

/// <summary>
/// The room's shared enemy services the composition of its species (<see cref="RoomEnemySpecies"/>) builds every owner from
/// (#2571), handed to it by the room's enemy system once those services exist, so the composition reaches no enemy system.
/// </summary>
[ArchitectureReview("wide-dependencies", "The composition seam for every species of the room: each owner is built from a " +
    "different subset of the room's shared enemy services, and the composition is the one place that names the species.")]
[RoomLifetime]
internal sealed record RoomEnemySpeciesDependencies
{
    /// <summary>The enemy state and installed artwork that outlive each room.</summary>
    public required EnemySession Session { get; init; }

    /// <summary>The room's loaded definitions, random numbers and area.</summary>
    public required EnemyRoomBinding Room { get; init; }

    /// <summary>The room's enemy places.</summary>
    public required EnemyPlaces Places { get; init; }

    /// <summary>Runs each placed or spawned enemy's setup.</summary>
    public required EnemyLifecycle Lifecycle { get; init; }

    /// <summary>The sounds the enemies request.</summary>
    public required EnemyAudioQueue Audio { get; init; }

    /// <summary>The room's enemy projectiles.</summary>
    public required EnemyProjectileEngine Projectiles { get; init; }

    /// <summary>The room's effect actors.</summary>
    public required RoomEffectActors EffectActors { get; init; }

    /// <summary>The enemies' contact with Samus and her weapons.</summary>
    public required EnemyCollision Collision { get; init; }

    /// <summary>The enemies' deaths and the drops they leave.</summary>
    public required EnemyDeathsAndDrops DeathsAndDrops { get; init; }

    /// <summary>The room's blocks the enemies read and change.</summary>
    public required EnemyTerrain Terrain { get; init; }

    /// <summary>The room's earthquake.</summary>
    public required RoomEarthquake Earthquake { get; init; }

    /// <summary>The events the enemy pass publishes.</summary>
    public required EnemyPassEvents PassEvents { get; init; }

    /// <summary>The frame policy that can force off-screen enemies into the pass.</summary>
    public required EnemyFrameContext FrameContext { get; init; }

    /// <summary>The surfaces the enemies draw on.</summary>
    public required EnemyPresentation Presentation { get; init; }

    /// <summary>What the room's bosses draw with.</summary>
    public required BossPresentation BossPresentation { get; init; }

    /// <summary>The bosses' installed artwork.</summary>
    public required BossArtworkBinding BossArtworkBinding { get; init; }

    /// <summary>The room's boss encounters, which the bosses join at their setup.</summary>
    public required RoomBossEncounters BossEncounters { get; init; }

    /// <summary>The room's status the enemies record: kills and the area boss.</summary>
    public required EnemyRoomStatus RoomStatus { get; init; }

    /// <summary>The room's effects the enemies start.</summary>
    public required EnemyRoomFx RoomFx { get; init; }

    /// <summary>The cap an enemy puts on the screen's scrolling speed.</summary>
    public required SamusScrollCap ScrollCap { get; init; }

    /// <summary>The distance the camera keeps from Samus, which an enemy may change.</summary>
    public required RoomCameraDistance CameraDistance { get; init; }

    /// <summary>The lowest the camera goes, which an enemy may change.</summary>
    public required RoomCameraFloor CameraFloor { get; init; }

    /// <summary>The Puyo's ceiling turn, read from the last draw.</summary>
    public required PuyoCeilingTurnFromLastDraw PuyoCeilingTurn { get; init; }

    /// <summary>Samus's passage through a door, which some enemies watch.</summary>
    public required DoorPassage DoorPassage { get; init; }

    /// <summary>The room's scenery its enemies act on.</summary>
    public required RoomEnemyScenery Scenery { get; init; }

    /// <summary>What a Bull takes over from the place's previous occupant.</summary>
    public required PlaceInheritance<BullEnemyState> BullInheritance { get; init; }

    /// <summary>What a Beetom takes over from the place's previous occupant.</summary>
    public required PlaceInheritance<BeetomEnemyState> BeetomInheritance { get; init; }

    /// <summary>The fade step count every fade shares.</summary>
    public required SharedFadeStepCount FadeStepCount { get; init; }

    /// <summary>The session's unauthored Speed Booster cadence.</summary>
    public required UnauthoredBoostCadence UnauthoredBoostCadence { get; init; }

    /// <summary>The Yard's kick camera measure.</summary>
    public required YardKickTrackingDistance YardKickDistance { get; init; }

    /// <summary>The falling sparks' floor probe.</summary>
    public required FallingSparkFloorProbe FallingSparkFloorProbe { get; init; }
}
