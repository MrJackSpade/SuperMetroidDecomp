using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Runtime;

/// <summary>
/// The PLM phase of a gameplay frame and the door-transition PLM handler.
/// </summary>
internal sealed class PlmFramePhase
{
    /// <summary>Stores artwork.</summary>
    private readonly RuntimeArtwork _artwork;
    /// <summary>Stores background scroll.</summary>
    private readonly BackgroundScrollState _backgroundScroll;
    /// <summary>Stores bomb projectiles.</summary>
    private readonly SamusBombProjectileSystem _bombProjectiles;
    /// <summary>Stores cgram.</summary>
    private readonly SnesCgram _cgram;
    /// <summary>Stores controller1.</summary>
    private readonly ControllerInputState _controller1;
    /// <summary>Stores enemies.</summary>
    private readonly RoomEnemySystem _enemies;
    /// <summary>Stores enemy room effects.</summary>
    private readonly EnemyRoomEffects _enemyRoomEffects;
    /// <summary>Stores freeze.</summary>
    private readonly GameplayTimeFreeze _freeze;
    /// <summary>Stores message box.</summary>
    private readonly GameplayMessageBoxState _messageBox;
    /// <summary>Stores message boxes.</summary>
    private readonly MessageBoxFlow _messageBoxes;
    /// <summary>Stores player.</summary>
    private readonly SamusFrameState _player;
    /// <summary>Stores plms.</summary>
    private readonly RoomPlmSystem _plms;
    /// <summary>Stores room.</summary>
    private readonly ActiveRoomState _room;
    /// <summary>Stores suit pickup.</summary>
    private readonly SamusSuitPickupState _suitPickup;
    /// <summary>Stores vram.</summary>
    private readonly SnesVram _vram;
    /// <summary>Stores vram writes.</summary>
    private readonly VramWriteQueue _vramWrites;
    /// <summary>Stores address space.</summary>
    private readonly ISnesAddressSpace _addressSpace;

    /// <summary>Initializes a new plm frame phase instance.</summary>
    internal PlmFramePhase(PlmFramePhaseDependencies dependencies)
    {
        _artwork = dependencies.Artwork;
        _backgroundScroll = dependencies.BackgroundScroll;
        _bombProjectiles = dependencies.BombProjectiles;
        _cgram = dependencies.Cgram;
        _controller1 = dependencies.Controller1;
        _enemies = dependencies.Enemies;
        _enemyRoomEffects = dependencies.EnemyRoomEffects;
        _freeze = dependencies.Freeze;
        _messageBox = dependencies.MessageBox;
        _messageBoxes = dependencies.MessageBoxes;
        _player = dependencies.Player;
        _plms = dependencies.Plms;
        _room = dependencies.Room;
        _suitPickup = dependencies.SuitPickup;
        _vram = dependencies.Vram;
        _vramWrites = dependencies.VramWrites;
        _addressSpace = dependencies.AddressSpace;
    }
    /// <summary>
    /// Executes the explicit <c>PLM_Handler</c> call made by the door coroutine at
    /// <c>$82:E53C</c>, after its final scrolling IRQ and before the following NMI.
    /// </summary>
    /// <remarks>
    /// The ordinary frame dispatcher intentionally suppresses PLMs while the shared
    /// door/elevator freeze flag is set. This call is the cartridge's one exception: it
    /// advances freshly loaded room setup lists once before the destination fades in.
    /// </remarks>
    internal void RunDoorTransitionPlmHandler()
    {
        // The explicit transition-time call still enters the common bank-$84 handler,
        // whose first instruction returns while native PLM word `$1C23` is disabled.
        // Indirect G-Mode deliberately carries that disabled word across the door.
        if (_player.Samus?.Xray.ArePlmsSuspended == true)
            return;

        RunPlmHandlerCore(controllerNewInput: 0);

        // Setup-time PLMs do not have Samus contact/input available. If a future
        // translation publishes a player-facing event here, fail at the exact ownership
        // boundary instead of silently discarding it.
        if (_plms.CollectiblePlms.CollectiblePickupEvents.Count != 0 || _plms.Stations.StationActivationEvents.Count != 0)
        {
            throw new InvalidDataException(
                "A door-transition PLM handler unexpectedly published a player interaction.");
        }
        foreach (MotherBrainGlassProjectileRequest request in _plms.MotherBrainGlass.MotherBrainGlassProjectileRequests)
            _enemies.MotherBrainBehavior.SpawnMotherBrainGlassProjectile(request);
        foreach (BombTorizoStatueProjectileRequest request in _plms.BombTorizoHand.BombTorizoStatueProjectileRequests)
            _enemies.BombTorizoBehavior.SpawnBombTorizoStatueBreakingProjectile(request);
    }

    /// <summary>
    /// Runs the shared bank-$84 handler body used by ordinary gameplay and the explicit
    /// transition-time call. Player-facing events remain with each caller because the
    /// transition path cannot legitimately collect an item or activate a station.
    /// </summary>
    private void RunPlmHandlerCore(ushort controllerNewInput)
    {
        if (_room.LevelData is null || _room.BackgroundStreamer is null || _room.Camera is null || _player.Samus is null)
        {
            throw new InvalidOperationException(
                "The PLM handler requires an active room, camera, and Samus.");
        }

        IReadOnlyList<PlmTilemapUpdate> updates = _plms.Step(_addressSpace, _room.LevelData, _room.BackgroundStreamer, new PlmView { Layer1Position = _room.Camera.PixelPosition, Bg1XOffset = unchecked((ushort)_backgroundScroll.Bg1Offset.X) }, new PlmFrameInputs { Scrolls = _room.Camera.Scrolls, EnemyDeaths = _enemies.RoomStatus.EnemiesKilled, EnemyDeathQuota = _enemies.PopulationLoader.DeathQuota, ControllerNewInput = controllerNewInput, CollectedItems = _player.Samus.CollectedItems, PowerBombExplosionStatus = _bombProjectiles.PowerBombExplosion.Status });
        foreach (PlmTilemapUpdate update in updates)
            update.ExecuteTo(_vram);
        _enemyRoomEffects.ApplyPendingDownwardGateProjectileRequests();

        foreach (PlmVramWriteRequest request in _plms.BombTorizoHand.VramWriteRequests)
        {
            _vramWrites.Enqueue(
                request.SizeInBytes,
                request.SourceAddress,
                request.Destination);
        }
    }

    /// <summary>Runs plm phase.</summary>
    internal void RunPlmPhase(bool deathOwnsSamus, SamusState samus)
    {
        // Native gameplay state eight reaches PLM_Handler after Samus's new-state and
        // enemy-projectile passes but before MainScrollingRoutine. A grapple block
        // acquired above therefore consumes its initial timer and draws $E0B7 in this
        // same frame; when it later becomes air, anchor validation has already run and
        // sees that mutation on the following Samus frame.
        if (!deathOwnsSamus && !_freeze.TimeIsFrozen && !samus.Xray.ArePlmsSuspended)
        {
            RunPlmHandlerCore(_controller1.NewlyPressed);

            // Item PLMs publish acquisition only after their native trigger and
            // handler pass. Apply the hardware-facing consequences at that same seam:
            // beam combinations replace the projectile character/palette staging, and
            // suit changes defer their visible palette reveal until bank-$88 stage
            // three after the message. Resource tanks and other equipment bits are
            // consumed directly by the HUD/movement owners.
            foreach (CollectiblePickupEvent pickup in _plms.CollectiblePlms.CollectiblePickupEvents)
            {
                if (pickup.Kind is
                    InWorldCollectibleKind.ChargeBeam or
                    InWorldCollectibleKind.IceBeam or
                    InWorldCollectibleKind.WaveBeam or
                    InWorldCollectibleKind.SpazerBeam or
                    InWorldCollectibleKind.PlasmaBeam)
                {
                    SamusProjectileSystem.QueueBeamTilesAndLoadPalette(
                        _addressSpace,
                        _vramWrites,
                        _cgram,
                        samus.EquippedBeams, _artwork.BeamArtwork);
                }
                else if (pickup.Kind is
                         InWorldCollectibleKind.VariaSuit or
                         InWorldCollectibleKind.GravitySuit)
                {
                    if (_messageBoxes._pendingSuitPickup is not null || _suitPickup.IsActive)
                    {
                        throw new InvalidDataException(
                            "A second suit pickup attempted to replace an active transformation.");
                    }
                    _messageBoxes._pendingSuitPickup = pickup.Kind == InWorldCollectibleKind.VariaSuit
                        ? SamusSuitPickupKind.Varia
                        : SamusSuitPickupKind.Gravity;
                }

                if (_messageBox.IsActive)
                {
                    throw new InvalidDataException(
                        "Multiple permanent items attempted to enter the synchronous " +
                        "bank-$85 message routine during one PLM pass.");
                }
                _messageBox.Begin(_addressSpace, pickup.MessageBoxIndex, _controller1.Current);
            }
            foreach (StationActivationEvent station in _plms.Stations.StationActivationEvents)
            {
                if (station.Kind == StationKind.Save)
                {
                    if (station.MessageBoxIndex == GameplayMessageIds.SaveConfirmation)
                    {
                        if (_messageBoxes._pendingSaveStation is not null ||
                            _messageBoxes._pendingSaveStationCompletion is not null ||
                            _messageBox.IsActive)
                        {
                            throw new InvalidDataException(
                                "A save station attempted to replace an active PLM message owner.");
                        }
                        _messageBoxes._pendingSaveStation = station;
                        _messageBox.Begin(
                            _addressSpace,
                            GameplayMessageIds.SaveConfirmation,
                            _controller1.Current);
                        continue;
                    }
                    if (station.MessageBoxIndex == GameplayMessageIds.SaveCompleted)
                    {
                        if (_messageBoxes._pendingSaveStation is not null ||
                            _messageBoxes._pendingSaveStationCompletion is not null ||
                            _messageBox.IsActive)
                        {
                            throw new InvalidDataException(
                                "A completed save station attempted to replace an active message owner.");
                        }
                        _messageBoxes._pendingSaveStationCompletion = station;
                        _messageBox.Begin(_addressSpace, GameplayMessageIds.SaveCompleted, _controller1.Current);
                        continue;
                    }
                    throw new InvalidDataException(
                        $"Save station published unexpected message ${(int)station.MessageBoxIndex:X2}.");
                }
                if (_messageBox.IsActive)
                {
                    throw new InvalidDataException(
                        "Multiple PLMs attempted to enter the synchronous bank-$85 " +
                        "message routine during one handler pass.");
                }
                _messageBox.Begin(_addressSpace, station.MessageBoxIndex, _controller1.Current);
            }
            foreach (MotherBrainGlassProjectileRequest request in
                     _plms.MotherBrainGlass.MotherBrainGlassProjectileRequests)
            {
                _enemies.MotherBrainBehavior.SpawnMotherBrainGlassProjectile(request);
            }
            foreach (BombTorizoStatueProjectileRequest request in
                     _plms.BombTorizoHand.BombTorizoStatueProjectileRequests)
            {
                _enemies.BombTorizoBehavior.SpawnBombTorizoStatueBreakingProjectile(request);
            }
        }
    }
}
