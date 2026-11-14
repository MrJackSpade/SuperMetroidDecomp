using SuperMetroid.Core.Input;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Bosses.Torizo;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Gameplay;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.World;

/// <summary>
/// The room-object phase of a gameplay frame and the door-transition room-object handler.
/// </summary>
internal sealed class RoomObjectFramePhase
{
    /// <summary>Current and newly pressed buttons passed to room-object and message handlers.</summary>
    private readonly ControllerInputState _controller1;
    /// <summary>Applies gate projectiles requested during the room-object pass.</summary>
    private readonly EnemyRoomEffects _enemyRoomEffects;
    /// <summary>Suppresses ordinary room-object updates while gameplay time is frozen.</summary>
    private readonly GameplayTimeFreeze _freeze;
    /// <summary>Owns the active synchronous item or station message.</summary>
    private readonly GameplayMessageBoxState _messageBox;
    /// <summary>Retains pending save-station and suit-pickup handoffs.</summary>
    private readonly MessageBoxFlow _messageBoxes;
    /// <summary>Supplies Samus and the X-ray room-object-suspension flag.</summary>
    private readonly SamusFrameState _player;
    /// <summary>Supplies active level data, camera, and scroll state.</summary>
    private readonly ActiveRoomState _room;
    /// <summary>Prevents a second suit transformation from replacing an active one.</summary>
    private readonly SamusSuitPickupState _suitPickup;


    /// <summary>Binds the shared room-object handler to live runtime state and its presentation effects.</summary>
    internal RoomObjectFramePhase(RoomObjectFramePhaseDependencies dependencies)
    {
        _controller1 = dependencies.Controller1;
        _enemyRoomEffects = dependencies.EnemyRoomEffects;
        _freeze = dependencies.Freeze;
        _messageBox = dependencies.MessageBox;
        _messageBoxes = dependencies.MessageBoxes;
        _player = dependencies.Player;
        _room = dependencies.Room;
        _suitPickup = dependencies.SuitPickup;

    }
    /// <summary>
    /// Runs the room objects once for the door transition, on the door scroll's final update and
    /// before the following frame boundary.
    /// </summary>
    /// <remarks>
    /// The ordinary frame step intentionally suppresses room objects while the shared
    /// door/elevator freeze flag is set. This call is the game's one exception: it
    /// advances the freshly loaded room objects' sequences once before the destination fades in.
    /// </remarks>
    internal void RunDoorTransitionObjectHandler(RoomObjectSystem roomObjects, SamusBombProjectileSystem bombProjectiles, RoomScope room)
    {
        // The explicit transition-time call still enters the common handler,
        // whose first step returns while the room-object handler is disabled.
        // Indirect G-Mode deliberately carries that disabled flag across the door.
        if (_player.Samus?.Xray.AreObjectsSuspended == true)
            return;

        RunObjectHandlerCore(controllerNewInput: 0, roomObjects, bombProjectiles, room);

        // Setup-time room objects do not have Samus contact/input available. If a future
        // change publishes a player-facing event here, fail at the exact ownership
        // boundary instead of silently discarding it.
        if (roomObjects.CollectibleObjects.CollectiblePickupEvents.Count != 0 || roomObjects.Stations.StationActivationEvents.Count != 0)
        {
            throw new InvalidDataException(
                "A door-transition room-object handler unexpectedly published a player interaction.");
        }
        foreach (MotherBrainGlassProjectileRequest request in roomObjects.MotherBrainGlass.MotherBrainGlassProjectileRequests)
            room.Enemies.RoomObjectSpawns.SpawnMotherBrainGlassProjectile(request);
        foreach (BombTorizoStatueProjectileRequest request in roomObjects.BombTorizoHand.BombTorizoStatueProjectileRequests)
            room.Enemies.RoomObjectSpawns.SpawnBombTorizoStatueBreakingProjectile(request);
    }

    /// <summary>
    /// Runs the shared handler body used by ordinary gameplay and the explicit
    /// transition-time call. Player-facing events remain with each caller because the
    /// transition path cannot legitimately collect an item or activate a station.
    /// </summary>
    private void RunObjectHandlerCore(ControllerButton controllerNewInput, RoomObjectSystem roomObjects, SamusBombProjectileSystem bombProjectiles, RoomScope room)
    {
        if (_room.LevelData is null || _room.Camera is null || _player.Samus is null)
        {
            throw new InvalidOperationException(
                "The room-object handler requires an active room, camera, and Samus.");
        }

        roomObjects.Step(_room.LevelData, new RoomObjectFrameInputs { Scrolls = _room.Camera.Scrolls, EnemyDeaths = room.Enemies.RoomStatus.EnemiesKilled, EnemyDeathQuota = room.Enemies.PopulationLoader.DeathQuota, ControllerNewInput = controllerNewInput, CollectedItems = _player.Samus.CollectedItems, PowerBombExplosionActive = bombProjectiles.PowerBombExplosion.IsActive });
        _enemyRoomEffects.ApplyPendingDownwardGateProjectileRequests(roomObjects, room.Enemies);
    }

    /// <summary>Runs the ordinary room-object phase when gameplay permits it, then applies pickups, station messages, and spawned projectiles.</summary>
    internal void RunObjectPhase(bool deathOwnsSamus, SamusState samus, RoomObjectSystem roomObjects, SamusBombProjectileSystem bombProjectiles, RoomScope room)
    {
        // Main gameplay reaches the room-object update after Samus's new-state and
        // enemy-projectile passes but before the main scrolling. A grapple block
        // acquired above therefore consumes its initial timer and draws its first frame in this
        // same frame; when it later becomes air, anchor validation has already run and
        // sees that mutation on the following Samus frame.
        if (!deathOwnsSamus && !_freeze.TimeIsFrozen && !samus.Xray.AreObjectsSuspended)
        {
            RunObjectHandlerCore(_controller1.NewlyPressed, roomObjects, bombProjectiles, room);

            // Item room objects publish acquisition only after their trigger and
            // handler pass. Apply their colour consequences at that same seam:
            // beam pickups show the newly equipped combination's colours, and
            // suit changes defer their visible palette reveal until stage
            // three after the message. Resource tanks and other equipment bits are
            // consumed directly by the HUD/movement owners.
            foreach (CollectiblePickupEvent pickup in roomObjects.CollectibleObjects.CollectiblePickupEvents)
            {
                if (pickup.Kind is
                    InWorldCollectibleKind.ChargeBeam or
                    InWorldCollectibleKind.IceBeam or
                    InWorldCollectibleKind.WaveBeam or
                    InWorldCollectibleKind.SpazerBeam or
                    InWorldCollectibleKind.PlasmaBeam)
                {
                    SamusProjectileSystem.LoadBeamPalette(_player.Colours, samus.EquippedBeams);
                }
                else if (pickup.Kind is
                         InWorldCollectibleKind.VariaSuit or
                         InWorldCollectibleKind.GravitySuit)
                {
                    if (_messageBoxes.HasPendingSuitPickup || _suitPickup.IsActive)
                    {
                        throw new InvalidDataException(
                            "A second suit pickup attempted to replace an active transformation.");
                    }
                    _messageBoxes.AwaitSuitPickup(pickup.Kind == InWorldCollectibleKind.VariaSuit
                        ? SamusSuitPickupKind.Varia
                        : SamusSuitPickupKind.Gravity);
                }

                if (_messageBox.IsActive)
                {
                    throw new InvalidDataException(
                        "Multiple permanent items attempted to enter the synchronous " +
                        "message box during one room-object pass.");
                }
                _messageBox.Begin(pickup.Message, _controller1.Current);
            }
            foreach (StationActivationEvent station in roomObjects.Stations.StationActivationEvents)
            {
                if (station.Kind == StationKind.Save)
                {
                    if (station.Message == GameplayMessageId.SaveConfirmation)
                    {
                        if (_messageBoxes.HasPendingSaveStationWork ||
                            _messageBox.IsActive)
                        {
                            throw new InvalidDataException(
                                "A save station attempted to replace an active room-object message owner.");
                        }
                        _messageBoxes.AwaitSaveConfirmation(station);
                        _messageBox.Begin(GameplayMessageId.SaveConfirmation,
                            _controller1.Current);
                        continue;
                    }
                    if (station.Message == GameplayMessageId.SaveCompleted)
                    {
                        if (_messageBoxes.HasPendingSaveStationWork ||
                            _messageBox.IsActive)
                        {
                            throw new InvalidDataException(
                                "A completed save station attempted to replace an active message owner.");
                        }
                        _messageBoxes.AwaitSaveCompletion(station);
                        _messageBox.Begin(GameplayMessageId.SaveCompleted, _controller1.Current);
                        continue;
                    }
                    throw new InvalidDataException(
                        $"Save station published unexpected message {station.Message}.");
                }
                if (_messageBox.IsActive)
                {
                    throw new InvalidDataException(
                        "Multiple room objects attempted to open the synchronous message box " +
                        "during one handler pass.");
                }
                _messageBox.Begin(station.Message, _controller1.Current);
            }
            foreach (MotherBrainGlassProjectileRequest request in
                     roomObjects.MotherBrainGlass.MotherBrainGlassProjectileRequests)
            {
                room.Enemies.RoomObjectSpawns.SpawnMotherBrainGlassProjectile(request);
            }
            foreach (BombTorizoStatueProjectileRequest request in
                     roomObjects.BombTorizoHand.BombTorizoStatueProjectileRequests)
            {
                room.Enemies.RoomObjectSpawns.SpawnBombTorizoStatueBreakingProjectile(request);
            }
        }
    }
}
