using Engine.Actors;
using Engine.Shared;
using Engine.World;
using Game.Data;
using Godot;

namespace Game.Rooms.Ceres;

/// <summary>
/// The elevator shaft of Ceres in its last moments. Once the way to the elevator has been blown open the whole
/// shaft tilts, one way and then the other, slowly near each end of its swing; and when Samus stands on the elevator
/// it takes her and her departure begins (<see cref="Cinematics.CeresDeparture"/>), the shaft tilting on beneath it.
/// </summary>
public partial class CeresShaft : RoomSequence
{
    /// <summary>The picture of the shaft, which is what tilts.</summary>
    [Export]
    public NodePath Floor { get; set; } = new();

    /// <summary>The point the shaft tilts about, in the room.</summary>
    [Export]
    public Vector2I Centre { get; set; } = new(128, 1008);

    /// <summary>
    /// The room's vents of steam, which are shown tilted with Samus. The cartridge tilts nothing else that stands in
    /// the shaft: its doors are drawn where they stand.
    /// </summary>
    [Export]
    public Godot.Collections.Array<NodePath> Steam { get; set; } = [];

    /// <summary>The room play goes on in once she has left, by its scene.</summary>
    [Export(PropertyHint.File)]
    public string LandingRoom { get; set; } = "";

    /// <summary>The entry point she arrives at there.</summary>
    [Export]
    public string LandingEntry { get; set; } = "";

    /// <summary>How far the shaft tilts either way, in steps of a 256th of a radian.</summary>
    private const int FullTilt = 34;

    /// <summary>How many updates the shaft stays level before it first tilts.</summary>
    private const int LevelUpdates = 60;

    /// <summary>
    /// How many updates the shaft holds each tilt, by how many steps from level it is, the update that turns it there
    /// the first: the old game's table, entry for entry.
    /// </summary>
    private static readonly int[] HoldUpdates =
    [
        2, 2, 2, 2, 2, 3, 3, 3, 3, 4, 4, 4, 4, 5, 5, 5, 6, 6, 7, 7, 8, 8,
        9, 10, 11, 12, 13, 15, 17, 19, 21, 23, 25, 27, 61,
    ];

    private int _hold;
    private int _tilt;
    private bool _backward;
    private bool _taken;

    /// <inheritdoc/>
    protected override bool Play(Room room, int elapsed)
    {
        if (elapsed == 0)
            (_hold, _tilt, _backward, _taken) = (LevelUpdates, 0, false, false);
        if (!room.Session.Progress.Has(StoryEvents.CeresElevatorDoorDestroyed))
            return false;

        // The shaft leaves each step of its turn for the hall's falling pieces to count from: its level start as its
        // turn is set up, and every step after as it is reached.
        CeresDebrisCount count = room.Session.Service<CeresDebrisCount>();
        if (elapsed == 0)
            count.LeftByShaft(_tilt + FullTilt, _backward);

        // The elevator takes her only on an update that is play's own, as the cartridge's does only while the game is
        // still in its main play: not on the update the countdown runs out, which the time up takes, nor beneath the
        // time up's white-out. Once play is its own again it takes her, the countdown at nothing or not.
        Player samus = room.Player;
        Escape escape = room.Session.Service<Escape>();
        bool playIsItsOwn = !escape.WantsToTakeOver && room.Session.Mode is not TimeUpMode;
        if (!_taken && playIsItsOwn && IsOnElevator(samus))
        {
            // The elevator has her: she stands still, and her departure begins after this update. The shaft goes on
            // tilting beneath it.
            samus.TrySetForm(Player.Standing);
            samus.Hold();
            _taken = true;

            // The countdown stands still while the elevator has her, and still shows.
            escape.ShowsWhileHeld = true;
            escape.Depart(LandingRoom, LandingEntry);
        }

        if (_hold > 0)
        {
            _hold--;
            return false;
        }

        _hold = HoldUpdates[Math.Abs(_tilt)] - 1;
        double radians = _tilt / 256.0;
        (GetNode(Floor) as TurningFloor
            ?? throw new InvalidOperationException($"The shaft \"{Name}\" is to tilt \"{Floor}\", but that is no turning floor."))
            .Turn(radians, 1, Centre, Vector2.Zero);

        // Samus and the steam are shown at the floor's own turn.
        RoomObject[] steam = [.. Steam.Select(path => GetNode(path) as RoomObject
            ?? throw new InvalidOperationException($"The shaft \"{Name}\" is to tilt \"{path}\", but that is no object of the room."))];
        room.ShownTurn = new RoomTurn(radians, PixelPosition.FromWhole(Centre.X, Centre.Y), steam);

        // It swings to one end, jumps to the very end, and swings back the other way.
        if (_backward)
        {
            if (_tilt == -FullTilt + 1)
                _backward = false;
            _tilt--;
        }
        else if (_tilt == FullTilt - 1)
        {
            _backward = true;
            _tilt = FullTilt;
        }
        else
        {
            _tilt++;
        }

        count.LeftByShaft(_tilt + FullTilt, _backward);
        return false;
    }

    /// <summary>Whether Samus is standing still on the elevator.</summary>
    private static bool IsOnElevator(Player samus)
    {
        int x = samus.Position.X.Whole, y = samus.Position.Y.Whole;
        return x > 112 && x <= 144 && y >= 75 && y < 128 && samus.Velocity.Y.IsZero;
    }
}
