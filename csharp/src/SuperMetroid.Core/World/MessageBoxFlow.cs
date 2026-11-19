using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Saves;
using SuperMetroid.Core.Gameplay;
using SuperMetroid.Core.Hud;

namespace SuperMetroid.Core.World;

/// <summary>
/// Message box outcomes: what waits for an open box to close (<see cref="MessageBoxOutcome"/>), the save-station
/// persistence and suit transformation that follow a box closing, and the sounds a box's updates make outside gameplay's
/// own requests.
/// </summary>
internal sealed class MessageBoxFlow
{
    /// <summary>Provides gameplay freeze state for HUD counter refresh after a box closes.</summary>
    private readonly GameplayTimeFreeze _freeze;
    /// <summary>Refreshes counters when message completion lacks a suspended frame tail.</summary>
    private readonly HudState _hud;
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
        _suitPickup = dependencies.SuitPickup;
        _system = dependencies.System;
        _audioRequests = Ensure.NotNull(dependencies.AudioRequests);
    }

    /// <summary>What waits for the open message box to close, or null when the box waits for nothing or none is open.</summary>
    private MessageBoxOutcome? _pending;

    /// <summary>
    /// The sounds the message box's updates made outside gameplay's own requests since the frontend last took them, in
    /// the order they happened: the save choice's move, the gunship's saving sound as its accepted prompt closes and the
    /// gunship's entrance pad opening as its prompt returns.
    /// </summary>
    private readonly List<SoundRequest> _sounds = [];

    /// <summary>Whether an outcome waits for the open message box to close.</summary>
    internal bool HasPendingOutcome => _pending is not null;

    /// <summary>Holds <paramref name="outcome"/> until the message box it opens with closes.</summary>
    /// <exception cref="InvalidDataException">Another outcome already waits: two message boxes opened at once.</exception>
    internal void Await(MessageBoxOutcome outcome)
    {
        Ensure.NotNull(outcome);
        if (_pending is not null)
            throw new InvalidDataException($"{outcome} attempted to replace {_pending}, which still waits for its message box.");
        _pending = outcome;
    }

    /// <summary>The save choice moved on this update, which plays the menu cursor's sound.</summary>
    internal void SaveChoiceMoved() =>
        _sounds.Add(new SoundRequest(SamusSound.MenuCursor, SoundRequestPriority.Normal));

    /// <summary>The gunship's accepted save closed its prompt on this update, which starts the saving sound.</summary>
    internal void GunshipSavingSoundStarted() =>
        _sounds.Add(new SoundRequest(SamusSound.Saving, SoundRequestPriority.Normal));

    /// <summary>
    /// Takes the sounds the message box's updates have made since the last take, in the order they were made.
    /// </summary>
    /// <remarks>
    /// Message-box updates run no gameplay; the frontend queues these at the update's end, after the room's music, as its
    /// own boundary's requests.
    /// </remarks>
    public IReadOnlyList<SoundRequest> TakeSounds()
    {
        SoundRequest[] taken = [.. _sounds];
        _sounds.Clear();
        return taken;
    }

    /// <summary>
    /// Applies the effects that follow a message box closing: the box hands its <paramref name="closing"/>, with a save
    /// confirmation's answer, to the outcome that waited on it.
    /// </summary>
    /// <returns>
    /// The save the answer completed (#2514), whose file the frontend writes to the selected slot after the update, as it
    /// owns the slot; the runtime supplies only the load station and the already-changed gameplay state. Null when the
    /// close completed no save.
    /// </returns>
    /// <exception cref="InvalidDataException">The close and the outcome waiting on it do not belong together.</exception>
    internal SaveStationPersistenceRequest? ResolveClosedMessageBox(MessageBoxStep.Closed closing, bool frameSuspended, RoomScope room, SamusState samus)
    {
        Ensure.NotNull(closing);
        MessageBoxOutcome? outcome = _pending;
        _pending = null;
        if (closing is MessageBoxStep.Answered answer)
        {
            // Only a save prompt's box answers, and only a save prompt's outcome waits for its answer.
            return outcome switch
            {
                MessageBoxOutcome.GunshipSaveAnswer => ResolveGunshipSaveAnswer(room, answer.Choice),
                MessageBoxOutcome.SaveConfirmation confirmation => ResolveSaveConfirmation(room, confirmation.Station, answer.Choice),
                _ => throw new InvalidDataException(
                    $"The message box closed with {answer.Choice}, but what waited on it ('{outcome}') waits for no answer."),
            };
        }
        switch (outcome)
        {
            case MessageBoxOutcome.GunshipSaveAnswer or MessageBoxOutcome.SaveConfirmation:
                throw new InvalidDataException($"{outcome} waited for an answer its message box closed without.");
            case MessageBoxOutcome.SaveCompletion completion:
                room.RoomObjects.Stations.CompleteSaveStation(completion.Station);
                break;
            case MessageBoxOutcome.SuitTransformation:
            case null:
                break;
            default:
                throw new InvalidOperationException($"Undefined message-box outcome {outcome}.");
        }

        room.RoomObjects.CollectibleObjects.CompleteCollectibleMessage(room.LevelData);
        // The resumed frame reaches the HUD update, which shows the new inventory. A
        // suspended frame tail does that itself; a capture without one updates it here.
        if (!frameSuspended)
            _hud.UpdateGameplayCounters(samus, _audioRequests, _freeze.TimeIsFrozen);

        // Closing the message resumes the item pickup that opened it.
        // Varia/Gravity immediately call their shared setup; for other items the
        // rest of the pickup already ran when the box opened.
        if (outcome is MessageBoxOutcome.SuitTransformation suit)
        {
            room.Enemies.Elevator.ResetStatus();
            _suitPickup.Begin(
                samus,
                room.Camera.PixelPosition,
                suit.Suit);
        }
        return null;
    }

    /// <summary>
    /// The gunship's prompt returns with its <paramref name="choice"/>: the gunship lets Samus out, its entrance pad
    /// opening, and a YES saves at the gunship.
    /// </summary>
    /// <returns>The save at the gunship a YES completed; null for a NO.</returns>
    private SaveStationPersistenceRequest? ResolveGunshipSaveAnswer(RoomScope room, SaveChoice choice)
    {
        room.Enemies.Gunship.AnswerSavePrompt();
        // Message-box updates do not publish the ordinary enemy sound list, so the pad's sound goes with the box's.
        _sounds.Add(new SoundRequest(RoomSound.GunshipPadOpening, SoundRequestPriority.Normal));
        if (choice != SaveChoice.Yes)
            return null;
        LoadStation gunship = LoadStationDefinitions.Gunship;
        _system.MarkSaveStationUsed(gunship);
        return new SaveStationPersistenceRequest(gunship);
    }

    /// <summary>
    /// A save station's confirmation returns with its <paramref name="choice"/>: the station saves on YES, starting its
    /// electricity.
    /// </summary>
    /// <returns>The station's save a YES completed; null when the station does not save.</returns>
    private SaveStationPersistenceRequest? ResolveSaveConfirmation(RoomScope room, SaveStationActivation station, SaveChoice choice)
    {
        bool saving = room.RoomObjects.Stations.ResolveSaveStationConfirmation(station, choice, room.LevelData);
        if (!saving)
            return null;
        room.Enemies.SaveStationElectricity.Spawn(station.Block);
        _system.MarkSaveStationUsed(station.SavesAt);
        return new SaveStationPersistenceRequest(station.SavesAt);
    }
}
