using SuperMetroid.Core.Samus.Drawing;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms.Scrolling;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.World;

using SuperMetroid.Core.World.RoomStates;

namespace SuperMetroid.Core.Runtime.Attract;

/// <summary>
/// Drives the attract demo: sets up its room and Samus, then substitutes the scripted input each frame and
/// restores the player's controller afterwards.
/// </summary>
internal sealed class AttractDemoDriver : IAttractSamusInput
{
    /// <summary>Owns the scripted input and the temporary controller substitution scope.</summary>
    private readonly AttractDemoSession _attractSession;
    /// <summary>Player controller replaced by demo input only during Samus updates.</summary>
    private readonly GameplayActionInput _playerActions;
    /// <summary>Builds the initial HUD from the demo record's health and ammunition.</summary>
    private readonly GameplayHudSetup _hudSetup;
    /// <summary>Publishes initial HUD graphics before loading the demo room.</summary>
    private readonly AcceptedFrameBoundary _frameBoundary;
    /// <summary>Receives Samus state and the selected demo pose or shinespark launch.</summary>
    private readonly SamusFrameState _player;
    /// <summary>Holds demo room viewport, camera scrolls, and load-station state.</summary>
    private readonly ActiveRoomState _room;
    /// <summary>Initialises the demo's Samus and loads the recorded room and door independently using the demo camera position.</summary>
    private readonly RoomLoader _roomLoader;
    /// <summary>Sets the demo's progression without reading or writing a player save.</summary>
    private readonly SharedGameState _system;

    /// <summary>Retains the runtime owners used to initialize and advance attract scenes.</summary>
    internal AttractDemoDriver(AttractDemoDriverDependencies dependencies)
    {
        _attractSession = dependencies.AttractSession;
        _playerActions = dependencies.PlayerActions;
        _hudSetup = dependencies.HudSetup;
        _frameBoundary = dependencies.FrameBoundary;
        _player = dependencies.Player;
        _room = dependencies.Room;
        _roomLoader = dependencies.RoomLoader;
        _system = dependencies.System;
    }
    /// <summary>Reflects whether the current session is substituting an attract demo for gameplay.</summary>
    public bool IsAttractDemo => _attractSession.IsAttractDemo;

    /// <summary>Advances the recorded input and temporarily substitutes its held and newly pressed actions.</summary>
    public void BeginAttractSamusInput()
    {
        if (_attractSession.Input is null) return;
        _player.Samus.InputSnapshot.PressesSeenLastUpdate = _attractSession.Input.Script.NewlyPressed;
        _attractSession.Input.Step();
        _attractSession.InputRestore = _playerActions.UseDemoInput(
            _attractSession.Input.Script.Held, _attractSession.Input.Script.NewlyPressed);
    }

    /// <summary>Disposes the demo input scope so the player's controller is restored after Samus updates.</summary>
    internal void RestoreAttractPlayerInput()
    {
        _attractSession.InputRestore?.Dispose();
        _attractSession.InputRestore = null;
    }

    /// <summary>
    /// The progress every attract demo plays with: every item location collected, no door opened, no event or boss,
    /// every area's map acquired and every station used, and nothing explored.
    /// </summary>
    private static PersistentProgress DemoProgress()
    {
        var progress = new PersistentProgress();
        progress.CollectedItems.AddAll();
        progress.UsedStations.AddAll();
        foreach (AreaId area in AreaIds.All)
            progress.SetAreaMap(area, acquired: true);
        return progress;
    }

    /// <summary>
    /// Loads the demo scene without consulting or writing a player save.
    /// Uses the ordinary room, room-object, enemy, FX, and graphics loaders with demo progression.
    /// </summary>
    /// <returns>The runtime's loaded room, the demo's.</returns>
    internal RoomScope InitializeAttractDemo(AttractDemoScene scene)
    {
        Ensure.NotNull(scene);
        _system.Progress.CopyFrom(DemoProgress());
        // The demo load clears the Zebes areas' stored explored maps and leaves the live map as the session left it.
        StoredExploredMapLoads.ClearForDemo(_system.StoredExploredMaps);

        SamusState samus = _roomLoader.InitializeSamus();
        samus.Position = FixedPosition<RoomPixels>.FromWhole(scene.Samus.Position);
        samus.EquippedItems = scene.Items;
        samus.CollectedItems = scene.Items;
        samus.EquippedBeams = scene.EquippedBeams;
        samus.CollectedBeams = scene.CollectedBeams;
        samus.Health = scene.Health;
        samus.MaxHealth = scene.Health;
        samus.Missiles = scene.Missiles;
        samus.MaxMissiles = scene.Missiles;
        samus.SuperMissiles = scene.SuperMissiles;
        samus.MaxSuperMissiles = scene.SuperMissiles;
        samus.PowerBombs = scene.PowerBombs;
        samus.MaxPowerBombs = scene.PowerBombs;
        _player.Samus = samus;
        scene.Samus.Setup.Apply(samus);
        samus.PreviousHealthForHurtCheck = samus.Health;
        _hudSetup.InitializeHud(new HudSnapshot(samus.Health, samus.MaxHealth,
            samus.Missiles, samus.MaxMissiles, samus.SuperMissiles, samus.MaxSuperMissiles,
            samus.PowerBombs, samus.MaxPowerBombs, samus.EquippedItems, HudItem.Beam, 0, ReserveTankMode.None));
        _frameBoundary.BeginAcceptedFrame(ControllerButton.None);
        DoorDefinition door = scene.Door;
        // The demo record independently supplies room and door. Do not replace room
        // selection with the door destination or derive placement from a load station.
        var room = _roomLoader.LoadSelectedRoom(scene.Room);
        // The scene's room setup acts on the demo room: the Kraid demo's phase wait on Kraid, composed with the load so it runs
        // as his body sets up (#2554), the others on the room the load built. A scene without one leaves the room as loaded.
        AttractDemoRoomWork work = RoomWorkOf(scene.RoomSetup);
        RoomScope loaded = _roomLoader.LoadRoom(door, room, scene.CameraPosition,
            RoomViewportLoadMode.DisplayInitialViewport, work.Entry);
        work.AfterLoad?.Invoke(loaded);
        SuperMetroid.Core.Samus.Colours.SamusSuitShade.Show(_player.Colours, samus.EquippedItems);
        SamusPoseMetadata.RefreshCollisionRadii(samus);
        SamusPresentation.InitializeAnimation(samus);
        _player.GroundedSamusMovementEnabled = true;
        _attractSession.Input = new AttractDemoInput(scene);
        return loaded;
    }

    /// <summary>
    /// What <paramref name="roomSetup"/> does to the scene's room: the scroll and defeated-boss setups act on the room its load
    /// built; the Kraid timer is composed with the load, over Kraid's setup.
    /// </summary>
    private AttractDemoRoomWork RoomWorkOf(AttractDemoRoomSetup? roomSetup) => roomSetup switch
    {
        null => new AttractDemoRoomWork(default, null),
        AttractDemoRoomSetup.ChargeBeamScroll => new AttractDemoRoomWork(default,
            room => room.Camera.Scrolls.SetRoomScreen(AttractDemoDefinitions.ChargeBeamClosedScreen, RoomScrollState.Closed)),
        AttractDemoRoomSetup.KraidTimer => new AttractDemoRoomWork(
            new RoomEntry { AttractDemoEnemySetup = hooks => hooks.StartKraidPhaseWaitAsHeSetsUp(AttractDemoDefinitions.KraidPhaseWaitUpdates) },
            null),
        AttractDemoRoomSetup.DefeatedKraid => new AttractDemoRoomWork(default,
            _ => _system.RecordBossesDefeated(AreaId.Brinstar, DefeatedBosses.AreaBoss)),
        _ => throw new InvalidOperationException($"Undefined AttractDemoRoomSetup {roomSetup}."),
    };

    /// <summary>What a scene's room setup does: how the room loads, and what acts on it once loaded, if anything.</summary>
    /// <param name="Entry">How the room loads.</param>
    /// <param name="AfterLoad">What acts on the room once loaded, or null when nothing does.</param>
    [RoomScopeBoundary("A per-call value, built for one room load and never kept.")]
    private readonly record struct AttractDemoRoomWork(RoomEntry Entry, Action<RoomScope>? AfterLoad);
}
