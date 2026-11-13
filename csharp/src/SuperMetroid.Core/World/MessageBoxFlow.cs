using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Saves;
using SuperMetroid.Core.Gameplay;
using SuperMetroid.Core.Hud;

namespace SuperMetroid.Core.World;

/// <summary>
/// Message box outcomes: selection sounds, save-station persistence, suit pickups and the gunship exit sound that
/// follow a box closing.
/// </summary>
internal sealed class MessageBoxFlow
{
    /// <summary>Provides gameplay freeze state for HUD counter refresh after a box closes.</summary>
    private readonly GameplayTimeFreeze _freeze;
    /// <summary>Refreshes counters when message completion lacks a suspended frame tail.</summary>
    private readonly HudState _hud;
    /// <summary>Publishes the confirmation choice consumed by save-station and gunship flows.</summary>
    private readonly GameplayMessageBoxState _messageBox;
    /// <summary>Supplies Samus inventory and state for HUD refresh and suit pickup.</summary>
    private readonly SamusFrameState _player;
    /// <summary>Supplies level data and camera for resumed room-object drawing.</summary>
    private readonly ActiveRoomState _room;
    /// <summary>Starts Varia or Gravity transformation after its suspended room object resumes.</summary>
    private readonly SamusSuitPickupState _suitPickup;
    /// <summary>Marks accepted gunship and room station saves in area progression bits.</summary>
    private readonly SharedGameState _system;
    /// <summary>The runtime's audio requests, which the HUD update after a message box queues its selection sound into.</summary>
    private readonly GameplayAudioRequests _audioRequests;

    /// <summary>Retains the runtime owners needed to resolve message outcomes once the box closes.</summary>
    internal MessageBoxFlow(MessageBoxFlowDependencies dependencies)
    {
        _freeze = dependencies.Freeze;
        _hud = dependencies.Hud;
        _messageBox = dependencies.MessageBox;
        _player = dependencies.Player;
        _room = dependencies.Room;
        _suitPickup = dependencies.SuitPickup;
        _system = dependencies.System;
        _audioRequests = Ensure.NotNull(dependencies.AudioRequests);
    }
    /// <summary>Suit transformation to start when its item message returns to the room object.</summary>
    private SamusSuitPickupKind? _pendingSuitPickup;
    /// <summary>Station prompt waiting for the save confirmation's yes/no result.</summary>
    private StationActivationEvent? _pendingSaveStation;
    /// <summary>Station animation to finish after its confirmation message closes.</summary>
    private StationActivationEvent? _pendingSaveStationCompletion;

    /// <summary>Whether a suit transformation waits for its item message to close.</summary>
    internal bool HasPendingSuitPickup => _pendingSuitPickup is not null;

    /// <summary>Whether a save station waits for its confirmation or completion message.</summary>
    internal bool HasPendingSaveStationWork => _pendingSaveStation is not null || _pendingSaveStationCompletion is not null;

    /// <summary>Holds a suit transformation until its item message returns to the room object.</summary>
    internal void AwaitSuitPickup(SamusSuitPickupKind suit) => _pendingSuitPickup = suit;

    /// <summary>Holds a save station until the save confirmation's yes/no answer.</summary>
    internal void AwaitSaveConfirmation(StationActivationEvent station) => _pendingSaveStation = station;

    /// <summary>Holds a save station's animation until its completion message closes.</summary>
    internal void AwaitSaveCompletion(StationActivationEvent station) => _pendingSaveStationCompletion = station;
    /// <summary>Confirmed area and station identity awaiting the frontend's save to the selected file.</summary>
    private SaveStationPersistenceRequest? _completedSaveStation;

    /// <summary>One-update request for the save choice's move sound.</summary>
    public bool MessageBoxSelectionSoundRequestedThisFrame { get; internal set; }

    /// <summary>
    /// Returns and clears the message selector's momentary sound publication.
    /// </summary>
    /// <remarks>
    /// Message-box updates run no gameplay; the frontend queues this request at the update's end, and consuming it keeps
    /// it audible once.
    /// </remarks>
    public bool ConsumeMessageBoxSelectionSoundRequest()
    {
        bool requested = MessageBoxSelectionSoundRequestedThisFrame;
        MessageBoxSelectionSoundRequestedThisFrame = false;
        return requested;
    }

    /// <summary>
    /// Returns and clears a confirmed save-station request. The save file's encoding remains owned by
    /// the frontend because it owns the selected file slot; the runtime supplies only the
    /// area/station identity and already-mutated gameplay state.
    /// </summary>
    public SaveStationPersistenceRequest? ConsumeSaveStationPersistenceRequest()
    {
        SaveStationPersistenceRequest? request = _completedSaveStation;
        _completedSaveStation = null;
        return request;
    }

    /// <summary>Applies the effects that follow a message box closing.</summary>
    internal void ResolveClosedMessageBox(bool frameSuspended, IRoomObjects roomObjects, SamusBombProjectileSystem bombProjectiles, RoomScope room)
    {
        if (room.Enemies.Gunship.SavePromptPending)
        {
            bool accepted = _messageBox.ConsumeConfirmationResult()
                ?? throw new InvalidDataException("Gunship message closed without a save selection.");
            room.Enemies.Gunship.AnswerSavePrompt(accepted);
            _gunshipExitSoundRequested = true;
            if (accepted)
            {
                LoadStation gunship = LoadStationDefinitions.Gunship;
                _system.MarkSaveStationUsed(gunship);
                _completedSaveStation = new SaveStationPersistenceRequest(gunship);
            }
            return;
        }

        if (_pendingSaveStation is { } saveStation)
        {
            bool? accepted = _messageBox.ConsumeConfirmationResult();
            if (accepted is null)
            {
                throw new InvalidDataException(
                    "The save station's save-confirmation message closed without publishing a selection.");
            }
            _pendingSaveStation = null;
            RoomLevelData level = _room.LevelData ?? throw new InvalidOperationException(
                "A save-station message returned without active room level data.");
            if (_room.Camera is null)
                throw new InvalidOperationException("A save-station message returned without an active room.");
            bool saving = roomObjects.Stations.ResolveSaveStationConfirmation(saveStation, accepted.Value, level);
            if (saving)
            {
                room.Enemies.Projectiles.SpawnSaveStationElectricity(saveStation.Block);
                _system.MarkSaveStationUsed(saveStation.Station);
                _completedSaveStation = new SaveStationPersistenceRequest(saveStation.Station);
            }
            return;
        }

        if (_pendingSaveStationCompletion is { } completedStation)
        {
            roomObjects.Stations.CompleteSaveStation(completedStation);
            _pendingSaveStationCompletion = null;
        }

        if (_room.LevelData is null || _room.Camera is null)
            throw new InvalidOperationException("A message box returned without an active room.");
        roomObjects.CollectibleObjects.CompleteCollectibleMessage(_room.LevelData);
        // The resumed dispatch reaches the HUD handler, which shows the new inventory. A
        // suspended frame tail does that itself; a capture without one updates it here.
        if (_player.Samus is not null && !frameSuspended)
            _hud.UpdateGameplayCounters(_player.Samus, _audioRequests, _freeze.TimeIsFrozen,
                soundSuppressed: bombProjectiles.PowerBombExplosion.IsActive);

        // The message returns directly to the suspended item's script.
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
    /// Consumes the gunship's entrance-pad sound queued when the suspended gunship message
    /// returns. Message-box updates do not publish the ordinary enemy sound list.
    /// </summary>
    public bool ConsumeGunshipExitSoundRequest()
    {
        bool requested = _gunshipExitSoundRequested;
        _gunshipExitSoundRequested = false;
        return requested;
    }
}
