using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Runtime;

/// <summary>
/// Message box outcomes: selection sounds, save-station persistence, suit pickups and the gunship exit sound that
/// follow a box closing.
/// </summary>
internal sealed class MessageBoxFlow
{
    private readonly BackgroundScrollState _backgroundScroll;
    private readonly SamusBombProjectileSystem _bombProjectiles;
    private readonly RoomEnemySystem _enemies;
    private readonly GameplayTimeFreeze _freeze;
    private readonly HudState _hud;
    private readonly GameplayMessageBoxState _messageBox;
    private readonly SamusFrameState _player;
    private readonly RoomPlmSystem _plms;
    private readonly ActiveRoomState _room;
    private readonly SamusSuitPickupState _suitPickup;
    private readonly Bank80SystemState _system;
    private readonly SnesVram _vram;
    private readonly VramWriteQueue _vramWrites;
    private readonly ISnesAddressSpace _addressSpace;

    internal MessageBoxFlow(MessageBoxFlowDependencies dependencies)
    {
        _backgroundScroll = dependencies.BackgroundScroll;
        _bombProjectiles = dependencies.BombProjectiles;
        _enemies = dependencies.Enemies;
        _freeze = dependencies.Freeze;
        _hud = dependencies.Hud;
        _messageBox = dependencies.MessageBox;
        _player = dependencies.Player;
        _plms = dependencies.Plms;
        _room = dependencies.Room;
        _suitPickup = dependencies.SuitPickup;
        _system = dependencies.System;
        _vram = dependencies.Vram;
        _vramWrites = dependencies.VramWrites;
        _addressSpace = dependencies.AddressSpace;
    }
    internal SamusSuitPickupKind? _pendingSuitPickup;
    internal StationActivationEvent? _pendingSaveStation;
    internal StationActivationEvent? _pendingSaveStationCompletion;
    private SaveStationPersistenceRequest? _completedSaveStation;

    /// <summary>One-frame bank-$85 request for the save selector's library-one sound $37.</summary>
    public bool MessageBoxSelectionSoundRequestedThisFrame { get; internal set; }

    /// <summary>
    /// Returns and clears the message selector's momentary sound publication.
    /// </summary>
    /// <remarks>
    /// Message boxes advance through an NMI-only path and therefore do not increment
    /// <see cref="NmiHandler.CompletedGameplayAudioPublication"/>. Giving this publisher explicit
    /// consume semantics keeps it audible once without reopening stale gameplay lists.
    /// </remarks>
    public bool ConsumeMessageBoxSelectionSoundRequest()
    {
        bool requested = MessageBoxSelectionSoundRequestedThisFrame;
        MessageBoxSelectionSoundRequestedThisFrame = false;
        return requested;
    }

    /// <summary>
    /// Returns and clears a confirmed save-station request. SRAM encoding remains owned by
    /// the frontend because it owns the selected file slot; the runtime supplies only the
    /// cartridge area/station identity and already-mutated gameplay state.
    /// </summary>
    public SaveStationPersistenceRequest? ConsumeSaveStationPersistenceRequest()
    {
        SaveStationPersistenceRequest? request = _completedSaveStation;
        _completedSaveStation = null;
        return request;
    }

    /// <summary>Applies the effects that follow a message box closing.</summary>
    internal void ResolveClosedMessageBox(bool frameSuspended)
    {

        if (_enemies.GunshipBehavior.GunshipSavePromptPending)
        {
            bool accepted = _messageBox.ConsumeConfirmationResult()
                ?? throw new InvalidDataException("Gunship message closed without a save selection.");
            _enemies.GunshipBehavior.AnswerGunshipSavePrompt(accepted);
            _gunshipExitSoundRequested = true;
            if (accepted)
            {
                _system.MarkSaveStationUsed(AreaId.Crateria, 0);
                _completedSaveStation = new SaveStationPersistenceRequest(AreaId.Crateria, 0);
            }
            return;
        }

        if (_pendingSaveStation is { } saveStation)
        {
            bool? accepted = _messageBox.ConsumeConfirmationResult();
            if (accepted is null)
            {
                throw new InvalidDataException(
                    "Save-station message $17 closed without publishing a selection.");
            }
            _pendingSaveStation = null;
            RoomLevelData level = _room.LevelData ?? throw new InvalidOperationException(
                "A save-station message returned without active room level data.");
            if (_room.BackgroundStreamer is null || _room.Camera is null)
                throw new InvalidOperationException("A save-station message returned without an active room.");
            SaveStationConfirmationResult confirmation = _plms.Stations.ResolveSaveStationConfirmation(_addressSpace, saveStation, accepted.Value, level, _room.BackgroundStreamer, new PlmView { Layer1Position = _room.Camera.PixelPosition, Bg1XOffset = unchecked((ushort)_backgroundScroll.Bg1Offset.X) });
            foreach (PlmTilemapUpdate update in confirmation.TilemapUpdates)
                update.ExecuteTo(_vram);
            if (confirmation.Saving)
            {
                _enemies.Projectiles.SpawnSaveStationElectricity(
                    saveStation.BlockIndex,
                    level.Size.X);
                _system.MarkSaveStationUsed(
                    saveStation.AreaIndex,
                    saveStation.StationIndex & 7);
                _completedSaveStation = new SaveStationPersistenceRequest(
                    saveStation.AreaIndex,
                    saveStation.StationIndex);
            }
            return;
        }

        if (_pendingSaveStationCompletion is { } completedStation)
        {
            _plms.Stations.CompleteSaveStation(completedStation);
            _pendingSaveStationCompletion = null;
        }

        if (_room.LevelData is null || _room.BackgroundStreamer is null || _room.Camera is null)
            throw new InvalidOperationException("A message box returned without an active room.");
        foreach (PlmTilemapUpdate update in _plms.CollectiblePlms.CompleteCollectibleMessage(_addressSpace, _room.LevelData, _room.BackgroundStreamer, new PlmView { Layer1Position = _room.Camera.PixelPosition, Bg1XOffset = unchecked((ushort)_backgroundScroll.Bg1Offset.X) }))
            update.ExecuteTo(_vram);
        // The resumed dispatch reaches the HUD handler, which shows the new inventory. A
        // suspended frame tail does that itself; a capture without one updates it here.
        if (_player.Samus is not null && !frameSuspended)
            _hud.UpdateGameplayCounters(_addressSpace, _player.Samus, _freeze.TimeIsFrozen,
                soundSuppressed: _bombProjectiles.PowerBombExplosion.IsActive);
        if (!frameSuspended)
            _hud.QueueUpload(_addressSpace, _vramWrites);

        // The routine returns directly to the suspended item-PLM instruction list.
        // Varia/Gravity immediately call their shared setup routine; for other items the
        // rest of the dispatch already ran when the box opened.
        if (_pendingSuitPickup is { } pendingSuit)
        {
            if (_player.Samus is null || _room.Camera is null)
            {
                throw new InvalidOperationException(
                    "A pending suit transformation requires an active Samus and room camera.");
            }
            _enemies.ElevatorBehavior.SetElevatorStatusForDebugging(0);
            _suitPickup.Begin(
                _addressSpace,
                _player.Samus,
                _room.Camera.PixelPosition,
                pendingSuit,
                soundSuppressed: _bombProjectiles.PowerBombExplosion.IsActive);
            _pendingSuitPickup = null;
        }
        return;
    }

    private bool _gunshipExitSoundRequested;

    /// <summary>
    /// Consumes the $A2:AB58 entrance-pad sound queued when the suspended gunship message
    /// returns. Message NMIs do not publish the ordinary EnemyMain sound list.
    /// </summary>
    public bool ConsumeGunshipExitSoundRequest()
    {
        bool requested = _gunshipExitSoundRequested;
        _gunshipExitSoundRequested = false;
        return requested;
    }
}
