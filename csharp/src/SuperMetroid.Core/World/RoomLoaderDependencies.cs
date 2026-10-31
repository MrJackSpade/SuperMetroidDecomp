using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Bosses.Artwork;
using SuperMetroid.Core.Areas;
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
    "camera, FX, palette FX, backgrounds, projectiles, PLMs, enemies, Tourian statues) over the session owners each one " +
    "outlives, as $82:E7D3 initializes them in one pass; each member is one of those session owners.")]
internal sealed record RoomLoaderDependencies
{
    /// <summary>Artwork.</summary>
    public required RuntimeArtwork Artwork { get; init; }


    /// <summary>Background scroll.</summary>
    public required BackgroundScrollState BackgroundScroll { get; init; }




    /// <summary>Gameplay's colours.</summary>
    public required GameplayColours Colours { get; init; }

    /// <summary>Controller1.</summary>
    public required ControllerInputState Controller1 { get; init; }

    /// <summary>Display.</summary>
    public required GameplayDisplayState Display { get; init; }

    /// <summary>Door transition.</summary>
    public required DoorTransitionRunner DoorTransition { get; init; }

    /// <summary>The enemy words and installed enemy artwork that outlive each room; each room's enemies are built over it.</summary>
    public required EnemySession EnemySession { get; init; }

    /// <summary>The session's installed boss artwork, which each room's bosses read.</summary>
    public required BossArtworkBinding BossArtwork { get; init; }

    /// <summary>The session's installed area artwork, which each room's area actors read.</summary>
    public required AreaArtworkBinding AreaArtwork { get; init; }

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

    /// <summary>The host's installed PLM artwork each room's PLM system draws with.</summary>
    public required PlmShotBlockVisuals ShotBlockVisuals { get; init; }

    /// <summary>The held beam charge, flare and charge palette, which each room's projectile system shares.</summary>
    public required BeamChargeState BeamCharge { get; init; }

    /// <summary>Room.</summary>
    public required ActiveRoomState Room { get; init; }

    /// <summary>The Fireflea state, the room's fixed colour and installed FX artwork each room's layer-3 FX reads.</summary>
    public required RoomFxSession RoomFx { get; init; }

    /// <summary>The palette-FX handler state each room's palette FX reads.</summary>
    public required PaletteFxSessionState PaletteFxSession { get; init; }

    /// <summary>The session owners each room's Tourian statue sequence is built over.</summary>
    public required TourianStatueSequenceDependencies TourianStatueDependencies { get; init; }





    /// <summary>System.</summary>
    public required SharedGameState System { get; init; }

    /// <summary>Tester.</summary>
    public required TesterTools Tester { get; init; }

    /// <summary>The installed services reproducing Samus's glitch reads, handed to every Samus and projectile system a load builds.</summary>
    public required SamusOverreadServices SamusOverreads { get; init; }




}
