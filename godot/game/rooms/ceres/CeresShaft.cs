using Engine.Actors;
using Engine.Shared;
using Engine.World;
using Game.Data;
using Godot;

namespace Game.Rooms.Ceres;

/// <summary>
/// The elevator shaft of Ceres in its last moments. Once the way to the elevator has been blown open the whole
/// shaft tilts, one way and then the other, slowly near each end of its swing; and when Samus stands on the elevator
/// it takes her, and she leaves the station.
/// </summary>
public partial class CeresShaft : RoomSequence
{
    /// <summary>The picture of the shaft, which is what tilts.</summary>
    [Export]
    public NodePath Floor { get; set; } = new();

    /// <summary>The point the shaft tilts about, in the room.</summary>
    [Export]
    public Vector2I Centre { get; set; } = new(128, 1008);

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

    /// <summary>How many updates Samus stands on the elevator before it takes her.</summary>
    private const int DepartureUpdates = 60;

    /// <summary>How many updates the shaft holds each tilt, by how many steps from level it is.</summary>
    private static readonly int[] HoldUpdates =
    [
        2, 2, 2, 2, 2, 3, 3, 3, 3, 4, 4, 4, 4, 5, 5, 5, 6, 6, 7, 7, 8, 8,
        9, 10, 11, 12, 13, 15, 17, 19, 21, 23, 25, 27, 61,
    ];

    private int _hold;
    private int _tilt;
    private bool _backward;
    private int _leavingIn;

    /// <inheritdoc/>
    protected override bool Play(Room room, int elapsed)
    {
        if (elapsed == 0)
            (_hold, _tilt, _backward, _leavingIn) = (LevelUpdates, 0, false, 0);
        if (!room.Session.Progress.Has(StoryEvents.CeresElevatorDoorDestroyed))
            return false;

        Player samus = room.Player;
        if (_leavingIn > 0)
        {
            if (--_leavingIn == 0)
            {
                // She is away, and that much is saved.
                room.Session.RequestSave(LandingRoom, LandingEntry);
                room.Session.Service<Escape>().Depart(LandingRoom, LandingEntry);
                return true;
            }
        }
        else if (IsOnElevator(samus))
        {
            // The elevator has her: she stands still, and after a moment it carries her off.
            samus.TrySetForm(Player.Standing);
            samus.Hold();
            _leavingIn = DepartureUpdates;
        }

        if (_hold > 0)
        {
            _hold--;
            return false;
        }

        _hold = HoldUpdates[Math.Abs(_tilt)] - 1;
        double radians = _tilt / 256.0;
        room.ShownTurn = new RoomTurn(radians, PixelPosition.FromWhole(Centre.X, Centre.Y));
        (GetNode(Floor) as TurningFloor
            ?? throw new InvalidOperationException($"The shaft \"{Name}\" is to tilt \"{Floor}\", but that is no turning floor."))
            .Turn(radians, 1, Centre, Vector2.Zero);

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

        return false;
    }

    /// <summary>Whether Samus is standing still on the elevator.</summary>
    private static bool IsOnElevator(Player samus)
    {
        int x = samus.Position.X.Whole, y = samus.Position.Y.Whole;
        return x > 112 && x <= 144 && y >= 75 && y < 128 && samus.Velocity.Y.IsZero;
    }
}
