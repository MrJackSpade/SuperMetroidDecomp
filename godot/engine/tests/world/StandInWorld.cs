using Engine.Actors;
using Engine.Collision;
using Engine.Core;
using Engine.Input;
using Engine.Shared;
using Engine.Tests.Collision;
using Engine.Tests.Content;
using Engine.Tests.Core;
using Engine.Tests.Input;
using Engine.World;
using Godot;

namespace Engine.Tests.World;

/// <summary>
/// A whole stand-in game for the engine's own checks: a session, a player, and one hand-written room built from a grid
/// of letters (the letters are <see cref="StandInRoom"/>'s). Things are added to the room before it is set up, as a
/// scene would place them, or spawned afterwards.
/// </summary>
public sealed class StandInWorld : IDisposable
{
    private readonly GameLoop _loop = new();
    private bool _setUp;

    /// <summary>A world whose room's terrain is the given rows of letters. The player starts at "Start", placed by <see cref="StartAt"/>.</summary>
    public StandInWorld(params string[] rows)
    {
        StandInRoom.WriteAssets(Assets, rows);
        _loop.Start(
            new StandInDefinition { UpdatesBeforeHandOver = 0 },
            new ScriptedInputSource(),
            Assets.Library,
            new SaveFile(Path.Combine(Assets.Folder, "saves"), 3),
            new Options());
        Room = new Room { Name = "Room" };
        Room.AddChild(new Terrain { Name = "Terrain", LayoutFile = "room.json" });
        _loop.AddChild(Room);
        StartAt(24, 32);
    }

    /// <summary>The world's assets, to which a check may add files before the room is set up.</summary>
    public TempAssets Assets { get; } = new();

    /// <summary>The session.</summary>
    public GameSession Session => _loop.Session;

    /// <summary>The room.</summary>
    public Room Room { get; }

    /// <summary>The player.</summary>
    public Player Player => Session.Player;

    /// <summary>
    /// Places the point the player starts at, given as the middle of their feet: the point itself is where their
    /// middle is put, half their standing height above.
    /// </summary>
    public StandInWorld StartAt(int x, int feetY)
    {
        Node? old = Room.GetNodeOrNull("Start");
        if (old is not null)
        {
            Room.RemoveChild(old);
            old.Free();
        }

        Room.AddChild(new Marker2D { Name = "Start", Position = new Vector2(x, feetY - Player.HalfHeightOf(Player.Standing)) });
        return this;
    }

    /// <summary>Places something in the room as a scene would, before the room is set up.</summary>
    public T Place<T>(T node, int x, int y) where T : Node2D
    {
        node.Position = new Vector2(x, y);
        Room.AddChild(node);
        return node;
    }

    /// <summary>Sets the room up, if it has not been, with the player arriving at "Start".</summary>
    public StandInWorld Setup()
    {
        if (!_setUp)
        {
            _setUp = true;
            Room.Setup(Session, Player, new RoomEntry("Start", PixelVelocity.Zero, Facing.Right));
        }

        return this;
    }

    /// <summary>Plays <paramref name="updates"/> updates of the room with the given actions held throughout.</summary>
    public void Step(int updates, params GameAction[] held)
    {
        Setup();
        for (int update = 0; update < updates; update++)
            Play(new GameInput(held, []));
    }

    /// <summary>
    /// Plays one update of the room as play does: an update a message stopped part-way is finished at once, the
    /// message left waiting in the session for the check to take.
    /// </summary>
    private void Play(GameInput input)
    {
        Room.Step(input);
        if (Room.UpdateIsSuspended)
            Room.FinishSuspendedUpdate();
    }

    /// <summary>Plays one update of the room in which the given actions are newly pressed, and held.</summary>
    public void Press(params GameAction[] pressed)
    {
        Setup();
        Play(new GameInput(pressed, pressed));
    }

    /// <summary>Plays updates, with the given actions held, until the player stands on the ground or a limit is reached.</summary>
    public void StepUntilLanded(params GameAction[] held)
    {
        Setup();
        for (int update = 0; update < 600; update++)
        {
            Play(new GameInput(held, []));
            if (Player.IsOnGround)
                return;
        }

        throw new CheckFailedException("The player did not land.");
    }

    /// <summary>Adds a collider of <paramref name="role"/> to <paramref name="owner"/>, covering <paramref name="rect"/> from its origin.</summary>
    public static Collider AddCollider(Node owner, ColliderRole role, PixelRect rect, string name = "")
    {
        var collider = new Collider { Role = role, Rect = rect, Name = name.Length > 0 ? name : role.ToString() };
        owner.AddChild(collider);
        return collider;
    }

    /// <summary>Frees the world's nodes and deletes its assets.</summary>
    public void Dispose()
    {
        _loop.Free();
        Assets.Dispose();
    }
}
