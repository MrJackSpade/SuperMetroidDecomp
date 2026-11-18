using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Abilities;
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
    /// <summary>Starts Varia or Gravity transformation after its suspended room object resumes.</summary>
    private readonly SamusSuitPickupState _suitPickup;
    /// <summary>Records the area's save stations where an accepted gunship or room save is made.</summary>
    private readonly SharedGameState _system;
    /// <summary>The runtime's audio requests, which the HUD update after a message box queues its selection sound into.</summary>
    private readonly GameplayAudioRequests _audioRequests;

    /// <summary>Retains the runtime owners needed to resolve message outcomes once the box closes.</summary>
    internal MessageBoxFlow(MessageBoxFlowDependencies dependencies)
    {
        _freeze = dependencies.Freeze;
        _hud = dependencies.Hud;
        _messageBox = dependencies.MessageBox;
        _suitPickup = dependencies.SuitPickup;
        _system = dependencies.System;
        _audioRequests = Ensure.NotNull(dependencies.AudioRequests);
    }
    /// <summary>Suit transformation to start when its item message returns to the room object.</summary>
    private SamusSuitPickupKind? _pendingSuitPickup;
    /// <summary>Station prompt waiting for the save confirmation's yes/no result.</summary>
    private SaveStationActivation? _pendingSaveStation;
    /// <summary>Station animation to finish after its confirmation message closes.</summary>
    private SaveStationActivation? _pendingSaveStationCompletion;

    /// <summary>Whether a suit transformation waits for its item message to close.</summary>
    internal bool HasPendingSuitPickup => _pendingSuitPickup is not null;

    /// <summary>Whether a save station waits for its confirmation or completion message.</summary>
    internal bool HasPendingSaveStationWork => _pendingSaveStation is not null || _pendingSaveStationCompletion is not null;

    /// <summary>Holds a suit transformation until its item message returns to the room object.</summary>
    internal void AwaitSuitPickup(SamusSuitPickupKind suit) => _pendingSuitPickup = suit;

    /// <summary>Holds a save station until the save confirmation's yes/no answer.</summary>
    internal void AwaitSaveConfirmation(SaveStationActivation station) => _pendingSaveStation = station;

    /// <summary>Holds a save station's animation until its completion message closes.</summary>
    internal void AwaitSaveCompletion(SaveStationActivation station) => _pendingSaveStationCompletion = station;
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
    internal void ResolveClosedMessageBox(bool frameSuspended, RoomScope room, SamusState samus)
    {
        if (room.Enemies.Gunship.SavePromptPending)
        {
            bool accepted = _messageBox.ConsumeConfirmationResult()
                ?? throw new InvalidDataException("Gunship message closed without a save selection.");
            room.Enemies.Gunship.AnswerSavePrompt();
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
            bool saving = room.RoomObjects.Stations.ResolveSaveStationConfirmation(saveStation, accepted.Value, room.LevelData);
            if (saving)
            {
                room.Enemies.SaveStationElectricity.Spawn(saveStation.Block);
                _system.MarkSaveStationUsed(saveStation.Station);
                _completedSaveStation = new SaveStationPersistenceRequest(saveStation.Station);
            }
            return;
        }

        if (_pendingSaveStationCompletion is { } completedStation)
        {
            room.RoomObjects.Stations.CompleteSaveStation(completedStation);
            _pendingSaveStationCompletion = null;
        }

        room.RoomObjects.CollectibleObjects.CompleteCollectibleMessage(room.LevelData);
        // The resumed frame reaches the HUD update, which shows the new inventory. A
        // suspended frame tail does that itself; a capture without one updates it here.
        if (!frameSuspended)
            _hud.UpdateGameplayCounters(samus, _audioRequests, _freeze.TimeIsFrozen);

        // Closing the message resumes the item pickup that opened it.
        // Varia/Gravity immediately call their shared setup; for other items the
        // rest of the pickup already ran when the box opened.
        if (_pendingSuitPickup is { } pendingSuit)
        {
            room.Enemies.Elevator.ResetStatus();
            _suitPickup.Begin(
                samus,
                room.Camera.PixelPosition,
                pendingSuit);
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
