using SuperMetroid.Core.Input;
using SuperMetroid.Core.Areas.Ceres;
using SuperMetroid.Core.Areas.Escape;
using SuperMetroid.Core.Areas.Maridia;
using SuperMetroid.Core.Areas.Tourian;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.WreckedShip;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Gameplay;

namespace SuperMetroid.Core.Rooms;

/// <summary>The collaborators a <see cref="RoomLoader"/> is constructed with, built once by the owner that creates it (#621).</summary>
internal sealed record RoomLoaderDependencies
{
    /// <summary>Artwork.</summary>
    public required RuntimeArtwork Artwork { get; init; }

    /// <summary>Background phase.</summary>
    public required BackgroundScrollPhase BackgroundPhase { get; init; }

    /// <summary>Background scroll.</summary>
    public required BackgroundScrollState BackgroundScroll { get; init; }

    /// <summary>Bomb projectiles.</summary>
    public required SamusBombProjectileSystem BombProjectiles { get; init; }

    /// <summary>Ceres elevator shaft.</summary>
    public required CeresElevatorShaftRoomMainState CeresElevatorShaft { get; init; }

    /// <summary>Ceres haze.</summary>
    public required CeresHazeState CeresHaze { get; init; }

    /// <summary>Cgram.</summary>
    public required SnesCgram Cgram { get; init; }

    /// <summary>Controller1.</summary>
    public required ControllerInputState Controller1 { get; init; }

    /// <summary>Display.</summary>
    public required GameplayDisplayState Display { get; init; }

    /// <summary>Door transition.</summary>
    public required DoorTransitionRunner DoorTransition { get; init; }

    /// <summary>Enemies.</summary>
    public required RoomEnemySystem Enemies { get; init; }

    /// <summary>The session's BG tilemap staging, which library-background programs fill.</summary>
    public required Bg2TilemapStaging Bg2TilemapStaging { get; init; }

    /// <summary>Enemy room effects.</summary>
    public required EnemyRoomEffects EnemyRoomEffects { get; init; }

    /// <summary>Escape effects.</summary>
    public required EscapeRoomEffects EscapeEffects { get; init; }

    /// <summary>Game time.</summary>
    public required GameTimeState GameTime { get; init; }

    /// <summary>Hud.</summary>
    public required HudState Hud { get; init; }

    /// <summary>Hud setup.</summary>
    public required GameplayHudSetup HudSetup { get; init; }

    /// <summary>Maridia elevatube.</summary>
    public required MaridiaElevatubeRoomMainState MaridiaElevatube { get; init; }

    /// <summary>Nmi.</summary>
    public required FrameClock Clock { get; init; }

    /// <summary>Options.</summary>
    public required GameplayOptions Options { get; init; }

    /// <summary>Player.</summary>
    public required SamusFrameState Player { get; init; }

    /// <summary>Plms.</summary>
    public required RoomPlmSystem Plms { get; init; }

    /// <summary>Projectiles.</summary>
    public required SamusProjectileSystem Projectiles { get; init; }

    /// <summary>Room.</summary>
    public required ActiveRoomState Room { get; init; }

    /// <summary>Room layer3 fx.</summary>
    public required RoomLayer3FxState RoomLayer3Fx { get; init; }

    /// <summary>Room palette fx.</summary>
    public required RoomPaletteFxSystem RoomPaletteFx { get; init; }

    /// <summary>Room spikes.</summary>
    public required RoomSpikeAnimatedTilesState RoomSpikes { get; init; }

    /// <summary>Room treadmills.</summary>
    public required RoomTreadmillAnimatedTilesState RoomTreadmills { get; init; }

    /// <summary>Sand animated tiles.</summary>
    public required RoomSandAnimatedTilesState SandAnimatedTiles { get; init; }

    /// <summary>System.</summary>
    public required Bank80SystemState System { get; init; }

    /// <summary>Tester.</summary>
    public required TesterTools Tester { get; init; }

    /// <summary>Tourian statues.</summary>
    public required TourianStatueSequence TourianStatues { get; init; }

    /// <summary>Vram.</summary>
    public required SnesVram Vram { get; init; }

    /// <summary>Vram writes.</summary>
    public required VramWriteQueue VramWrites { get; init; }

    /// <summary>Wrecked ship treadmill.</summary>
    public required WreckedShipTreadmillAnimatedTilesState WreckedShipTreadmill { get; init; }

    /// <summary>Address space.</summary>
    public required ISnesAddressSpace AddressSpace { get; init; }
}
