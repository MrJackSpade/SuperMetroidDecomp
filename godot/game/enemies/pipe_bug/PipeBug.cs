using Engine.Actors;
using Engine.Content;
using Engine.Shared;
using Game.Compatibility;
using Godot;

namespace Game.Enemies;

/// <summary>
/// A Zeb or a Zebbo: a bug that lives in a pipe. When Samus stands near and above its pipe it rises out, up to her
/// height, and shoots off across the room at her. Once it has flown out of view it is back in its pipe, and after a
/// pause is ready again.
/// </summary>
/// <remarks>Its timelines are "rising_left", "rising_right", "shooting_left" and "shooting_right".</remarks>
public partial class PipeBug : Enemy
{
    /// <summary>How far it rises out of its pipe at most, in pixels.</summary>
    [Export, DataValue("emergence_height")]
    public int EmergenceHeight { get; set; } = 16;

    /// <summary>How near across Samus must be, in pixels, to bring it out.</summary>
    [Export, DataValue("trigger_width")]
    public int TriggerWidth { get; set; } = 64;

    /// <summary>How far above it Samus may be, in pixels, and still bring it out.</summary>
    [Export, DataValue("trigger_top")]
    public int TriggerTop { get; set; } = 96;

    /// <summary>How many updates it waits, back in its pipe, before it can come out again.</summary>
    [Export, DataValue("respawn_wait")]
    public int RespawnWait { get; set; } = 48;

    /// <summary>How far it flies each update, in pixels.</summary>
    [Export, DataValue("flight_step")]
    public int FlightStep { get; set; } = 2;

    private enum Doing
    {
        WaitingToBeSeen,
        WaitingForSamus,
        Emerging,
        Flying,
        Resting,
    }

    private Doing _doing;
    private PixelPosition _pipe;
    private Countdown _rest;

    /// <inheritdoc/>
    protected override void OnEnterRoom()
    {
        _pipe = PixelPosition.FromWhole(Position.X.Whole, Position.Y.Whole);
        _rest = Countdown.Of(RespawnWait);
        Play("rising_left");
        Visual?.SetShown(false);
    }

    /// <inheritdoc/>
    protected override void Behave()
    {
        switch (_doing)
        {
            case Doing.WaitingToBeSeen:
                // It starts watching for Samus once its pipe is inside the window in which enemies take turns.
                if (IsWithinTurnReach)
                    _doing = Doing.WaitingForSamus;
                return;

            case Doing.WaitingForSamus:
                int below = Target.Y.Whole - Position.Y.Whole;
                if (below >= 0 || below < -TriggerTop)
                    return;
                Facing = Target.X.Whole - Position.X.Whole < 0 ? Facing.Left : Facing.Right;
                if (Math.Abs(Position.X.Whole - Target.X.Whole) >= TriggerWidth)
                    return;
                _doing = Doing.Emerging;
                Visual?.SetShown(true);
                Play(Facing == Facing.Left ? "rising_left" : "rising_right");
                return;

            case Doing.Emerging:
                // It rises a pixel an update, and two on the update the fine part of its height is nothing; that fine
                // part is counted down by one each update, apart from the rise.
                Pixels y = Position.Y;
                int fine = y.Fraction;
                Position = new PixelPosition(Position.X, Pixels.FromParts(y.Whole - (fine == 0 ? 2 : 1), (fine + Pixels.FractionsPerPixel - 1) % Pixels.FractionsPerPixel));
                if (_pipe.Y.Whole - EmergenceHeight < Position.Y.Whole || Position.Y.Whole >= Target.Y.Whole)
                    return;
                Play(Facing == Facing.Left ? "shooting_left" : "shooting_right");
                _doing = Doing.Flying;
                return;

            case Doing.Flying:
                Position += PixelOffset.FromWhole(Facing == Facing.Left ? -FlightStep : FlightStep, 0);
                // It flies on until its middle has passed the right edge of the enemies' view, or its body has wholly
                // passed the left, or the room's.
                int left = Room.Session.Camera.Left, x = Position.X.Whole;
                if (x >= 0 && x + BodyHalfWidth >= left && x < left + EnemyView.Extent)
                    return;
                // Back in its pipe, the fine part of its height takes the number of its height in pixels.
                Position = new PixelPosition(_pipe.X, Pixels.FromParts(_pipe.Y.Whole, _pipe.Y.Whole % Pixels.FractionsPerPixel));
                Play("rising_left");
                Visual?.SetShown(false);
                _rest = Countdown.Of(RespawnWait);
                _doing = Doing.Resting;
                return;

            case Doing.Resting:
                if (_rest.IsOver)
                    return;
                _rest = _rest.Stepped();
                if (_rest.IsOver)
                    _doing = Doing.WaitingForSamus;
                return;

            default:
                throw new InvalidOperationException($"Undefined pipe bug stage {_doing}.");
        }
    }
}
