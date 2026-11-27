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
/// ground instead. A floor or a ceiling stops the speed across and the rise but not the throw, and leaves the
/// player adrift (<see cref="Player.IsAdrift"/>) for what is left of it: drawn down with no fastest fall. When its time is up
/// the player falls from rest, and whatever their input asked on that update is dropped.
/// </summary>
public sealed class Throw
{
    /// <summary>The way the player is thrown.</summary>
    private Facing _toward;

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
    /// Renews the throw for a knockback that arrives while it is under way: it takes the new one's way and its time
    /// from the start, and goes on as it was otherwise, low or rising as it began, with no new rise.
    /// </summary>
    internal void Renew(Facing toward, int updates)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(updates);
        (_toward, _left) = (toward, Countdown.Of(updates));
    }

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

        // A low throw presses the player down as one on the ground is pressed after moving across: by as far as they
        // set out to move across, whatever a wall then left of their speed, and a pixel more.
        bool stopped = _low ? owner.PressToGround() : owner.MoveDownUnderGravity(keepsSpeedWhenStopped: true).Blocked;
        if (stopped)
        {
            // Stopped up or down, the player is left with no way up or down at all, not falling: adrift, for as long
            // as the throw goes on.
            owner.ComeToRest();
            owner.Drift();
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
