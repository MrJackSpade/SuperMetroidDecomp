using Engine.Actors;
using Engine.Ui;
using Engine.Input;
using Engine.Shared;
using Engine.World;

namespace Engine.Core;

/// <summary>
/// Play itself: the mode in which a room is played. It owns the room being played, steps it each update, and hands
/// over to whatever play calls for: a room transition when the player enters an exit, a message, the pause screen, or
/// the mode that follows the player's death. A game says what is stopped while the rest runs by overriding
/// <see cref="WhatIsStopped"/>.
/// </summary>
public partial class PlayingMode : GameMode, IRoomPlay
{
    private readonly string _firstRoom;
    private readonly string _firstEntryPoint;
    private readonly RoomEntry? _firstEntry;
    private Room? _room;
    private string _roomScene = "";

    /// <summary>Play starting in the game definition's first room.</summary>
    public PlayingMode()
    {
        _firstRoom = "";
        _firstEntryPoint = "";
    }

    /// <summary>Play starting in <paramref name="room"/> at <paramref name="entryPoint"/>, as after loading a save.</summary>
    public PlayingMode(string room, string entryPoint)
    {
        _firstRoom = room;
        _firstEntryPoint = entryPoint;
    }

    /// <summary>Play starting in <paramref name="room"/> with the player arriving as <paramref name="entry"/> says.</summary>
    public PlayingMode(string room, RoomEntry entry)
    {
        ArgumentNullException.ThrowIfNull(entry);
        _firstRoom = room;
        _firstEntryPoint = entry.EntryPoint;
        _firstEntry = entry;
    }

    private GameHud? _hud;

    /// <summary>What is shown over play, if the game shows anything.</summary>
    public GameHud? Hud => _hud;

    /// <summary>The room being played.</summary>
    public Room Room => _room ?? throw new InvalidOperationException("Play has no room yet.");

    /// <inheritdoc/>
    protected override void Enter()
    {
        string room = _firstRoom.Length > 0 ? _firstRoom : Session.Definition.FirstRoom;
        string entryPoint = _firstRoom.Length > 0 ? _firstEntryPoint : Session.Definition.FirstEntryPoint;
        if (room.Length == 0)
            throw new InvalidOperationException("The game definition names no first room.");
        _hud = Session.Definition.CreateHud();
        if (_hud is not null)
        {
            AddChild(_hud);
            _hud.Present(Session);
        }

        ChangeRoom(room, _firstEntry ?? new RoomEntry(entryPoint, PixelVelocity.Zero, Facing.Right));
    }

    /// <summary>Leaves the room being played, if any, and sets up the room at <paramref name="scene"/> for the player's arrival.</summary>
    public void ChangeRoom(string scene, RoomEntry entry)
    {
        if (_room is not null)
        {
            _room.Teardown();
            RemoveChild(_room);
            _room.Free();
        }

        _room = Session.Definition.CreateRoom(scene, Session.Content);
        _roomScene = scene;
        AddChild(_room);
        _room.Setup(Session, Session.Player, entry, asksForMusic: true);
        _hud?.Refresh(Session, _room);
    }

    /// <summary>The input of the update that was suspended for a message, which the rest of it is played with.</summary>
    private GameInput _suspendedInput = GameInput.None;

    /// <summary>Whether the update just played handed over to a message.</summary>
    private bool _handedToMessage;

    /// <summary>
    /// An update that hands over to a message gives the sounds no turn: the message gives that turn on the update
    /// it hands back.
    /// </summary>
    public override ClosingSoundWork SoundWorkAtClose => _handedToMessage ? ClosingSoundWork.None : ClosingSoundWork.Usual;

    /// <summary>The terms the update being played is played on, and its input.</summary>
    private PlayTerms _terms = PlayTerms.Usual;
    private GameInput _input = GameInput.None;

    /// <summary>Whether, as the player's energy was settled, the update was already going to take play elsewhere.</summary>
    private bool _leadsElsewhere;

    /// <inheritdoc/>
    public override GameMode Step(GameInput input) => Play(input, PlayTerms.Usual);

    /// <summary>
    /// Plays one whole update of play for a mode laid over it, on <paramref name="terms"/>. Returns the mode play
    /// hands over to, which takes control in place of the mode laid over it, or null when play goes on beneath.
    /// </summary>
    public GameMode? PlayBeneath(GameInput input, PlayTerms terms)
    {
        ArgumentNullException.ThrowIfNull(terms);
        GameMode next = Play(input, terms);
        return ReferenceEquals(next, this) ? null : next;
    }

    /// <summary>Whatever lasts beyond the room and runs beside it plays its update: the session's services.</summary>
    void IRoomPlay.AfterView(Room room)
    {
        foreach (ISessionService service in Session.Services)
            service.Step(Session);
    }

    /// <summary>
    /// A player whose energy is gone does not begin to die on an update that takes play elsewhere: one on which a
    /// service wants control or the pause is asked for, as the room itself holds it off for an exit entered. Play
    /// beneath another mode holds off what its terms say.
    /// </summary>
    (bool MayDie, bool MayBeSaved) IRoomPlay.EmptyEnergyTerms(Room room)
    {
        _leadsElsewhere = (_terms.ServicesMayTakeOver && Session.Services.Any(service => service.WantsToTakeOver)) || PauseIsAsked();
        return (_terms.PlayerMayDie && !_leadsElsewhere, _terms.PlayerMayBeSaved);
    }

    /// <summary>
    /// Whether this update's input asks for the pause and play may give it: the player is in control, has entered no
    /// exit on this update, which is gone through instead, and the game lets the pause be asked for here.
    /// </summary>
    private bool PauseIsAsked() =>
        _terms.MayPause && _input.WasPressed(GameAction.Pause) && Session.Player.State.InControl && Room.ExitEntered is null &&
        Session.Definition.PauseMayBeAsked(Session, Room);

    /// <summary>Plays one update of play on the terms given, and returns the mode in control next.</summary>
    private GameMode Play(GameInput input, PlayTerms terms)
    {
        _handedToMessage = false;
        (_terms, _input) = (terms, input);

        // The player's state plays its part in the update's beginning first: what it gives way to is the state the
        // whole update is played in, the pause and what the update holds still decided by it.
        Room.Player.GiveWayAsUpdateBegins();

        // A pause that play asked for comes once the messages asked for with it have been shown, before play goes on.
        if (terms.MayPause && Session.TakePauseRequest() is { } askedBy)
            return Session.Definition.CreatePauseMode(this, askedBy);
        Room.Step(input, WhatIsStopped(), this);

        // A message asked for by the time the room's objects have played is shown at once: the rest of the update,
        // the room's and play's own, is played on the update the message hands back.
        if (Room.UpdateIsSuspended)
        {
            _suspendedInput = input;
            return NextMessage(input, followsAnother: false)
                ?? throw new InvalidOperationException("The room suspended its update for a message, and none is waiting.");
        }

        return AfterRoom(input);
    }

    /// <summary>
    /// A message hands control back. The next message waiting opens as the one before it hands back, and what is
    /// left of the suspended update waits for it too; with none waiting, the rest of the suspended update is played
    /// now, with the input it began with.
    /// </summary>
    public override GameMode TakeBack(GameInput input)
    {
        if (NextMessage(input, followsAnother: true) is { } next)
            return next;
        if (!Room.UpdateIsSuspended)
            return this;
        Room.FinishSuspendedUpdate();
        return AfterRoom(_suspendedInput);
    }

    /// <summary>While a message covers play, the room's update opens when the message says so: its liquid, its sequences' and its projectiles' openings, and nothing of the session's.</summary>
    public override void StepBeneath() => Room.OpenUpdate(movesLiquid: true, opensProjectiles: true, opensSession: false);

    /// <summary>The message box for the next question or message waiting, or null when none is.</summary>
    private MessageBoxMode? NextMessage(GameInput input, bool followsAnother)
    {
        MessageBoxMode? box = null;
        if (Session.TakeQuestion() is { } question)
            box = new MessageBoxMode(this, question.Message, question.Answered) { AfterYes = question.AfterYes, Pictures = Room.Content, HeldBefore = input, FollowsAnother = followsAnother };
        else if (Session.TakeMessage() is { } message)
            box = new MessageBoxMode(this, message.Message) { Fanfare = message.Fanfare, HoldUpdates = message.HoldUpdates, Pictures = Room.Content, HeldBefore = input, FollowsAnother = followsAnother };
        if (box is not null)
            _handedToMessage = true;
        return box;
    }

    /// <summary>What play does of an update once the room has played all of its own.</summary>
    private GameMode AfterRoom(GameInput input)
    {
        // An update that withheld what follows from the player's energy running out does not count toward the play time.
        if (_terms.CountsPlayTime && Room.EnergySettled != EmptyEnergy.Withheld)
            Session.Progress.CountPlayUpdate(Session.Definition.MostPlayUpdates);

        if (Session.TakeSaveRequest() is { } save)
        {
            // That the game has been saved at this place is itself remembered, and is in the save.
            Session.Progress.Record(SaveData.SavedAtFact(save.EntryPoint));
            Session.Saves.Write(Session.Slot, SaveData.Capture(Session, save.Room.Length > 0 ? save.Room : _roomScene, save.EntryPoint));
        }
        _hud?.Refresh(Session, Room);

        // A player who began to die on this update is the game's from the next: it plays their death over this play.
        if (Room.EnergySettled == EmptyEnergy.Dying)
            return Session.Definition.CreateDeathMode(this);

        // What saved a player whose energy was gone takes the place of whatever else the update began: a service that
        // wanted control wants it still and is asked again, and the pause is not given.
        bool saved = Room.EnergySettled == EmptyEnergy.Saved;
        if (_terms.ServicesMayTakeOver && !saved)
        {
            foreach (ISessionService service in Session.Services)
            {
                if (service.WantsToTakeOver && service.TakesOver(this) is { } mode)
                    return mode;
            }
        }

        // A message asked for after the room's objects had played is shown with nothing of the update left to play.
        if (NextMessage(input, followsAnother: false) is { } box)
            return box;

        // An exit entered is gone through, and the pause is not given on the update that entered it.
        if (Room.ExitEntered is { } exit)
            return Session.Definition.CreateRoomTransition(this, exit);
        if (!saved && PauseIsAsked())
            return Session.Definition.CreatePauseMode(this, PauseAsker.Player);
        return this;
    }

    /// <summary>
    /// The groups of things in the room that do not run this update: what the state the player is in holds still
    /// (<see cref="PlayerState.Stops"/>) and what the room's playing sequences do
    /// (<see cref="RoomSequence.Stops"/>), unless a game's own play says otherwise.
    /// </summary>
    protected virtual Stopped WhatIsStopped() => Session.Player.State.Stops | Room.StoppedBySequences;
}
