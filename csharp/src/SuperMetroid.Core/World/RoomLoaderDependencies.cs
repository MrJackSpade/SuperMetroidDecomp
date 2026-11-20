using SuperMetroid.Core.Bosses.Artwork;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.Rooms.Scrolling;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Gameplay;

namespace SuperMetroid.Core.World;

/// <summary>The collaborators a <see cref="RoomLoader"/> is constructed with, built once by the owner that creates it (#621).</summary>
[ArchitectureReview("wide-dependencies", "The room composer: a room load builds every room-lifetime owner (level, " +
    "camera, FX, colour effects, backgrounds, projectiles, room objects, enemies, Tourian statues) over the session owners each one " +
    "outlives, as the room load initializes them in one pass; each member is one of those session owners.")]
internal sealed record RoomLoaderDependencies
{
    /// <summary>The installed artwork the entered room draws and colours from.</summary>
    public required RuntimeArtwork Artwork { get; init; }

    /// <summary>Where the foreground and background layers stand, which a load places for the entered room.</summary>
    public required BackgroundScrollState BackgroundScroll { get; init; }

    /// <summary>The player's gameplay actions as the frame boundary latched them.</summary>
    public required GameplayActionInput PlayerActions { get; init; }

    /// <summary>The runtime's audio requests, which every room system and every Samus built here queue theirs into.</summary>
    public required GameplayAudioRequests AudioRequests { get; init; }

    /// <summary>What the gameplay display shows, which a load sets up for the entered room.</summary>
    public required GameplayDisplayState Display { get; init; }

    /// <summary>The door transition, whose door setup a load runs once the room's objects exist.</summary>
    public required DoorTransitionRunner DoorTransition { get; init; }

    /// <summary>The enemy state and installed enemy artwork that outlive each room; each room's enemies are built over it.</summary>
    public required EnemySession EnemySession { get; init; }

    /// <summary>The session's installed boss artwork, which each room's bosses read.</summary>
    public required BossArtworkBinding BossArtwork { get; init; }

    /// <summary>The room effects the entered room's enemies request as they load.</summary>
    public required EnemyRoomEffects EnemyRoomEffects { get; init; }

    /// <summary>The game's clock, which loading a saved game restores.</summary>
    public required GameTimeState GameTime { get; init; }

    /// <summary>The HUD, whose values and minimap a load carries into the entered room.</summary>
    public required HudState Hud { get; init; }

    /// <summary>The game's frame counters, which time enemies, animations and blinking.</summary>
    public required FrameClock Clock { get; init; }

    /// <summary>How the door transition's scroll counts frames on the short frame counter.</summary>
    public required DoorTransitionFrameCounting DoorFrameCounting { get; init; }

    /// <summary>The in-game options, which loading a saved game restores.</summary>
    public required GameplayOptions Options { get; init; }

    /// <summary>Samus, whom a load places in the entered room and shows as she enters.</summary>
    public required SamusFrameState Player { get; init; }

    /// <summary>The host's installed room-object artwork each room's room-object system draws with.</summary>
    public required RoomObjectVisuals RoomObjectVisuals { get; init; }

    /// <summary>The held beam charge, flare and charge colouring, which each room's projectile system shares.</summary>
    public required BeamChargeState BeamCharge { get; init; }

    /// <summary>The loaded room, which a load replaces with the room it builds.</summary>
    public required ActiveRoomState Room { get; init; }

    /// <summary>Whether the colour effects run, and the heat phases, which each room's colour effects read.</summary>
    public required ColourEffectSessionState ColourEffectSession { get; init; }

    /// <summary>The session owners each room's Tourian statue sequence is built over.</summary>
    public required TourianStatueSequenceDependencies TourianStatueDependencies { get; init; }

    /// <summary>The session's shared game state: the events and defeated bosses that choose each room's state.</summary>
    public required SharedGameState System { get; init; }

    /// <summary>The runtime's assists, which grant the loadout and reset saved encounters on a load.</summary>
    public required ActiveAssists Assists { get; init; }

    /// <summary>The installed services reproducing Samus's glitch reads, handed to every Samus and projectile system a load builds.</summary>
    public required SamusOverreadServices SamusOverreads { get; init; }

    /// <summary>The installed cells each explicit scroll layout's load writes past the room's screens, which every room load reads.</summary>
    public required ScrollCellsPastRoomScreens<RoomScrollState> ScrollCellsPastRoomScreens { get; init; }

    /// <summary>
    /// The session's unauthored Speed Booster cadence, handed to every Samus a load builds and to every room's enemies.
    /// </summary>
    public required UnauthoredBoostCadence UnauthoredBoostCadence { get; init; }

    /// <summary>The session's camera fractions kept across games, which the runtime's first room's camera starts from.</summary>
    public required CameraFractionsKeptAcrossGames CameraFractions { get; init; }

    /// <summary>The runtime's shared fade step count, handed to every room's enemies.</summary>
    public required SharedFadeStepCount FadeStepCount { get; init; }

    /// <summary>The runtime's camera measure a kicked Yard flies off with, handed to every room's enemies.</summary>
    public required YardKickTrackingDistance YardKickDistance { get; init; }

    /// <summary>Initialises each new Samus the loader sets up.</summary>
    public required SamusInitialization SamusInitialization { get; init; }
}
