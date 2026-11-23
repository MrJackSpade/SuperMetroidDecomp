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
public partial class PlayingMode : GameMode
{
    private readonly string _firstRoom;
    private readonly string _firstEntryPoint;
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

    /// <summary>The room being played.</summary>
    public Room Room => _room ?? throw new InvalidOperationException("Play has no room yet.");

    /// <summary>The scene of the room being played.</summary>
    public string RoomScene => _roomScene;

    /// <inheritdoc/>
    protected override void Enter()
    {
        string room = _firstRoom.Length > 0 ? _firstRoom : Session.Definition.FirstRoom;
        string entryPoint = _firstRoom.Length > 0 ? _firstEntryPoint : Session.Definition.FirstEntryPoint;
        if (room.Length == 0)
            throw new InvalidOperationException("The game definition names no first room.");
        ChangeRoom(room, new RoomEntry(entryPoint, PixelVelocity.Zero, Facing.Right));
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

        _room = Session.Definition.CreateRoom(scene);
        _roomScene = scene;
        AddChild(_room);
        _room.Setup(Session, Session.Player, entry);
        Session.Sounds.RequestMusic(_room.Version?.Music ?? "");
    }

    /// <inheritdoc/>
    public override GameMode Step(GameInput input)
    {
        Room.Step(input, WhatIsStopped());
        Session.Progress.CountPlayUpdate();

        if (Session.TakeSaveRequest() is { } save)
            Session.Saves.Write(Session.Slot, SaveData.Capture(Session, save.Room.Length > 0 ? save.Room : _roomScene, save.EntryPoint));
        if (Session.Player.IsDead)
            return Session.Definition.CreateModeAfterDeath();
        if (Session.TakeMessage() is { } message)
            return new MessageBoxMode(this, message);
        if (Room.ExitEntered is { } exit)
            return new RoomTransitionMode(this, exit);
        if (input.WasPressed(GameAction.Pause) && Session.Player.State.InControl)
            return Session.Definition.CreatePauseMode(this);
        return this;
    }

    /// <summary>The groups of things in the room that do not run this update. Nothing, unless a game says otherwise.</summary>
    protected virtual Stopped WhatIsStopped() => Stopped.Nothing;
}
