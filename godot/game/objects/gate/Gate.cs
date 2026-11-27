using Engine.Actors;
using Engine.World.Objects;
using Godot;

namespace Game.Objects;

/// <summary>
/// The downward gate: a barrier its switches open and shut, which holds one of the room's places for what its enemies
/// launch while its column stands, as the cartridge's column is one of its enemies' shots. Shut as the room is set up,
/// it takes a place; opening, it gives it back once its column has risen out of sight; shutting, it takes one as its
/// column starts down after the open gate's wait. With no place free it holds none, and goes on as it would.
/// </summary>
public partial class Gate : Barrier
{
    /// <summary>
    /// The updates from the one it opens on to the one its column has risen out of sight: four blocks at a pixel an
    /// update.
    /// </summary>
    [Export]
    public int RiseUpdates { get; set; } = 64;

    /// <summary>The updates from the one it starts to shut on to the one its column starts down: the open gate's wait.</summary>
    [Export]
    public int LowerDelay { get; set; } = 16;

    /// <summary>The place its column holds, or null while it holds none.</summary>
    private Effect? _place;

    /// <summary>Whether it was open as its last turn ended.</summary>
    private bool _wasOpen;

    /// <summary>The updates since it began to open or to shut, counting that one as nought; null once neither is under way.</summary>
    private int? _moving;

    /// <inheritdoc/>
    protected override void OnEnterRoom()
    {
        base.OnEnterRoom();
        (_place, _wasOpen, _moving) = (null, IsOpen, null);
        if (!IsOpen)
            TakePlace();
    }

    /// <inheritdoc/>
    protected override void OnStep()
    {
        base.OnStep();
        if (IsOpen != _wasOpen)
            _moving = 0;
        else if (_moving is { } since)
            _moving = since + 1;
        _wasOpen = IsOpen;

        if (IsOpen && _moving == RiseUpdates)
        {
            GiveBackPlace();
            _moving = null;
        }
        else if (!IsOpen && _moving == LowerDelay)
        {
            TakePlace();
            _moving = null;
        }
    }

    /// <summary>Takes one of the room's places for its column, if one is free.</summary>
    private void TakePlace()
    {
        Effect place = Effect.HoldingPlace(Position);
        _place = Room.Spawn(place) ? place : null;
    }

    /// <summary>Gives back the place its column held, if it held one.</summary>
    private void GiveBackPlace()
    {
        if (_place is { } place)
            Room.Remove(place);
        _place = null;
    }
}
