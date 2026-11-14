using SuperMetroid.Core.Bosses.Artwork;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.Backgrounds;
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
    /// <summary>Artwork.</summary>
    public required RuntimeArtwork Artwork { get; init; }


    /// <summary>Background scroll.</summary>
    public required BackgroundScrollState BackgroundScroll { get; init; }





    /// <summary>Controller1.</summary>
    public required ControllerInputState Controller1 { get; init; }

    /// <summary>The runtime's audio requests, which every room system and every Samus built here queue theirs into.</summary>
    public required GameplayAudioRequests AudioRequests { get; init; }

    /// <summary>Display.</summary>
    public required GameplayDisplayState Display { get; init; }

    /// <summary>Door transition.</summary>
    public required DoorTransitionRunner DoorTransition { get; init; }

    /// <summary>The enemy state and installed enemy artwork that outlive each room; each room's enemies are built over it.</summary>
    public required EnemySession EnemySession { get; init; }

    /// <summary>The session's installed boss artwork, which each room's bosses read.</summary>
    public required BossArtworkBinding BossArtwork { get; init; }

    /// <summary>Enemy room effects.</summary>
    public required EnemyRoomEffects EnemyRoomEffects { get; init; }

    /// <summary>Escape effects.</summary>
    public required EscapeRoomEffects EscapeEffects { get; init; }

    /// <summary>Game time.</summary>
    public required GameTimeState GameTime { get; init; }

    /// <summary>Hud.</summary>
    public required HudState Hud { get; init; }



    /// <summary>The frame clock.</summary>
    public required FrameClock Clock { get; init; }

    /// <summary>How the door transition's scroll counts frames on the short frame counter.</summary>
    public required DoorTransitionFrameCounting DoorFrameCounting { get; init; }

    /// <summary>Options.</summary>
    public required GameplayOptions Options { get; init; }

    /// <summary>Player.</summary>
    public required SamusFrameState Player { get; init; }

    /// <summary>The host's installed room-object artwork each room's room-object system draws with.</summary>
    public required RoomObjectVisuals ShotBlockVisuals { get; init; }

    /// <summary>The held beam charge, flare and charge palette, which each room's projectile system shares.</summary>
    public required BeamChargeState BeamCharge { get; init; }

    /// <summary>Room.</summary>
    public required ActiveRoomState Room { get; init; }

    /// <summary>The Fireflea state, the room's fixed colour and installed FX artwork each room's effect overlay FX reads.</summary>
    public required RoomFxSession RoomFx { get; init; }

    /// <summary>Whether the colour effects run, and the heat phases, which each room's colour effects read.</summary>
    public required ColourEffectSessionState ColourEffectSession { get; init; }

    /// <summary>The session owners each room's Tourian statue sequence is built over.</summary>
    public required TourianStatueSequenceDependencies TourianStatueDependencies { get; init; }





    /// <summary>System.</summary>
    public required SharedGameState System { get; init; }

    /// <summary>The runtime's assists, which grant the loadout and reset saved encounters on a load.</summary>
    public required ActiveAssists Assists { get; init; }

    /// <summary>The installed services reproducing Samus's glitch reads, handed to every Samus and projectile system a load builds.</summary>
    public required SamusOverreadServices SamusOverreads { get; init; }

    /// <summary>
    /// The session's unauthored Speed Booster cadence, handed to every Samus a load builds and to every room's enemies.
    /// </summary>
    public required UnauthoredBoostCadence UnauthoredBoostCadence { get; init; }

    /// <summary>The session's camera fractions kept across games, which the runtime's first room's camera starts from.</summary>
    public required CameraFractionsKeptAcrossGames CameraFractions { get; init; }

    /// <summary>The runtime's shared fade step count, handed to every room's enemies.</summary>
    public required SharedFadeStepCount FadeStepCount { get; init; }




}
