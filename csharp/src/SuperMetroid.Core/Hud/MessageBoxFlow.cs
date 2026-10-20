using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Saves;
using SuperMetroid.Core.Gameplay;

namespace SuperMetroid.Core.Hud;

/// <summary>
/// Message box outcomes: selection sounds, save-station persistence, suit pickups and the gunship exit sound that
/// follow a box closing.
/// </summary>
internal sealed class MessageBoxFlow
{
    /// <summary>Provides gameplay freeze state for HUD counter refresh after a box closes.</summary>
    private readonly GameplayTimeFreeze _freeze;
    /// <summary>Refreshes counters and queues uploads when message completion lacks a suspended frame tail.</summary>
    private readonly HudState _hud;
    /// <summary>Publishes the confirmation choice consumed by save-station and gunship flows.</summary>
    private readonly GameplayMessageBoxState _messageBox;
    /// <summary>Supplies Samus inventory and state for HUD refresh and suit pickup.</summary>
    private readonly SamusFrameState _player;
    /// <summary>Supplies level data and camera for resumed PLM drawing.</summary>
    private readonly ActiveRoomState _room;
    /// <summary>Starts Varia or Gravity transformation after its suspended PLM resumes.</summary>
    private readonly SamusSuitPickupState _suitPickup;
    /// <summary>Marks accepted gunship and room station saves in area progression bits.</summary>
    private readonly Bank80SystemState _system;
    /// <summary>Queues HUD graphics when message completion runs outside a suspended frame.</summary>
    private readonly VramWriteQueue _vramWrites;

    /// <summary>Retains the runtime owners needed to resolve message outcomes after NMI-only execution.</summary>
    internal MessageBoxFlow(MessageBoxFlowDependencies dependencies)
    {
        _freeze = dependencies.Freeze;
        _hud = dependencies.Hud;
        _messageBox = dependencies.MessageBox;
        _player = dependencies.Player;
        _room = dependencies.Room;
        _suitPickup = dependencies.SuitPickup;
        _system = dependencies.System;
        _vramWrites = dependencies.VramWrites;
    }
    /// <summary>Suit transformation to start when its item message returns to the PLM.</summary>
    private SamusSuitPickupKind? _pendingSuitPickup;
    /// <summary>Station prompt waiting for a yes/no result from message box $17.</summary>
    private StationActivationEvent? _pendingSaveStation;
    /// <summary>Station animation to finish after its confirmation message closes.</summary>
    private StationActivationEvent? _pendingSaveStationCompletion;

    /// <summary>Whether a suit transformation waits for its item message to close.</summary>
    internal bool HasPendingSuitPickup => _pendingSuitPickup is not null;

    /// <summary>Whether a save station waits for its confirmation or completion message.</summary>
    internal bool HasPendingSaveStationWork => _pendingSaveStation is not null || _pendingSaveStationCompletion is not null;

    /// <summary>Holds a suit transformation until its item message returns to the PLM.</summary>
    internal void AwaitSuitPickup(SamusSuitPickupKind suit) => _pendingSuitPickup = suit;

    /// <summary>Holds a save station until message $17's yes/no answer.</summary>
    internal void AwaitSaveConfirmation(StationActivationEvent station) => _pendingSaveStation = station;

    /// <summary>Holds a save station's animation until its completion message closes.</summary>
    internal void AwaitSaveCompletion(StationActivationEvent station) => _pendingSaveStationCompletion = station;
    /// <summary>Confirmed area and station identity awaiting frontend SRAM persistence.</summary>
    private SaveStationPersistenceRequest? _completedSaveStation;

    /// <summary>One-frame bank-$85 request for the save selector's library-one sound $37.</summary>
    public bool MessageBoxSelectionSoundRequestedThisFrame { get; internal set; }

    /// <summary>
    /// Returns and clears the message selector's momentary sound publication.
    /// </summary>
    /// <remarks>
    /// Message boxes advance through an NMI-only path and therefore do not increment
    /// <see cref="AcceptedFrameBoundary.CompletedGameplayAudioPublication"/>. Giving this publisher explicit
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
    internal void ResolveClosedMessageBox(bool frameSuspended, RoomPlmSystem plms, SamusBombProjectileSystem bombProjectiles, RoomScope room)
    {
        if (room.Enemies.Gunship.SavePromptPending)
        {
            bool accepted = _messageBox.ConsumeConfirmationResult()
                ?? throw new InvalidDataException("Gunship message closed without a save selection.");
            room.Enemies.Gunship.AnswerSavePrompt(accepted);
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
            if (_room.Camera is null)
                throw new InvalidOperationException("A save-station message returned without an active room.");
            bool saving = plms.Stations.ResolveSaveStationConfirmation(saveStation, accepted.Value, level);
            if (saving)
            {
                room.Enemies.Projectiles.SpawnSaveStationElectricity(
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
            plms.Stations.CompleteSaveStation(completedStation);
            _pendingSaveStationCompletion = null;
        }

        if (_room.LevelData is null || _room.Camera is null)
            throw new InvalidOperationException("A message box returned without an active room.");
        plms.CollectiblePlms.CompleteCollectibleMessage(_room.LevelData);
        // The resumed dispatch reaches the HUD handler, which shows the new inventory. A
        // suspended frame tail does that itself; a capture without one updates it here.
        if (_player.Samus is not null && !frameSuspended)
            _hud.UpdateGameplayCounters(_player.Samus, _freeze.TimeIsFrozen,
                soundSuppressed: bombProjectiles.PowerBombExplosion.IsActive);
        if (!frameSuspended)
            _hud.QueueUpload(_vramWrites);

        // The routine returns directly to the suspended item-PLM instruction list.
        // Varia/Gravity immediately call their shared setup routine; for other items the
        // rest of the dispatch already ran when the box opened.
        if (_pendingSuitPickup is { } pendingSuit)
        {
            if (_player.Samus is null)
            {
                throw new InvalidOperationException(
                    "A pending suit transformation requires an active Samus.");
            }
            room.Enemies.Elevator.ResetStatus();
            _suitPickup.Begin(
                _player.Samus,
                _room.Camera.PixelPosition,
                pendingSuit,
                soundSuppressed: bombProjectiles.PowerBombExplosion.IsActive);
            _pendingSuitPickup = null;
        }
        return;
    }

    /// <summary>Entrance-pad sound pending after the suspended gunship prompt returns.</summary>
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
