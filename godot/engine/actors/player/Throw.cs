using Engine.Collision;
using Engine.Shared;

namespace Engine.Actors;

/// <summary>
/// A throw: a hit carries the player away from it for a few updates. It is no state of theirs. While it lasts it
/// moves them in place of the state they are in (<see cref="PlayerState.Step"/>), and that state goes on otherwise:
/// it is asked its input, and may give way to another, which the throw goes on carrying. An upright player is put in
/// <see cref="KnockedBackState"/> for it, out of their control; the low form is thrown as it is, in its own state.
/// The throw takes up whatever speed across the player had and gains by its own move values, a dash still carrying
/// them on top. It rises by the medium's knockback speed under gravity, unless it is kept low: pressed down to the
/// ground instead. A floor or a ceiling stops the speed across and the rise but not the throw. When its time is up
/// the player falls from rest, and whatever their input asked on that update is dropped.
/// </summary>
public sealed class Throw
{
    /// <summary>The way the player is thrown.</summary>
    private readonly Facing _toward;

    /// <summary>Whether the throw is kept low: pressed down to the ground instead of rising.</summary>
    private readonly bool _low;

    /// <summary>The name of the move values the throw goes by.</summary>
    private readonly string _moves;

    /// <summary>The updates left before the throw is over.</summary>
    private Countdown _left;

    /// <summary>A throw <paramref name="toward"/> one side for <paramref name="updates"/> updates, by the move values named.</summary>
    internal Throw(Facing toward, int updates, bool low, string moves)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(updates);
        ArgumentException.ThrowIfNullOrEmpty(moves);
        (_toward, _low, _moves, _left) = (toward, low, moves, Countdown.Of(updates));
    }

    /// <summary>The updates of the throw still to come.</summary>
    public int UpdatesLeft => _left.Remaining;

    /// <summary>
    /// Moves the player through one update of the throw, in place of <paramref name="state"/>'s own movement. Returns
    /// null while the throw goes on; on its last update the state itself, which the player stays in, their input for
    /// that update dropped.
    /// </summary>
    internal PlayerState? Carry(Player owner, PlayerState state)
    {
        // The speed across changes by the usual rule, and whatever it is carries the player the way they are thrown.
        owner.Steer(owner.Moves(_moves));
        owner.Velocity = owner.Velocity.WithX(_toward.Toward(owner.Velocity.X.Abs()));
        owner.MoveAcross();
        MoveResult down = _low ? PressDown(owner) : owner.MoveDownUnderGravity(keepsSpeedWhenStopped: true);
        if (down.Blocked)
        {
            owner.ComeToRest();
            owner.FallFromRest();
        }

        state.WhileThrown(owner);
        _left = _left.Stepped();
        if (_left.IsRunning)
            return null;

        // The throw is over: the player falls from rest, in the state they are in.
        owner.FallFromRest();
        owner.EndThrow();
        return state;
    }

    /// <summary>Moves the player down by as far as they move across this update and one pixel more, with no speed of their own.</summary>
    internal static MoveResult PressDown(Player owner) => owner.MoveDownBy(owner.SpeedAcross.Abs() + Pixels.FromWhole(1));
}
