using Engine.Collision;
using Engine.Input;
using Engine.Shared;

namespace Engine.Actors;

/// <summary>
/// Knocked back by a hit: thrown away from it for a moment, out of the player's control, still facing as they were.
/// The throw takes up whatever speed across the player had and gains by the knockback's own move values, a dash still
/// carrying them on top. It rises by the medium's knockback speed, unless the player was holding the way they face,
/// which keeps the throw low: pressed down to the ground instead of rising. A floor or a ceiling stops the speed
/// across and the rise but not the knockback. When its time is up the player falls from rest; an upright player
/// spends one update more being set down, and then stands or falls. The low form is thrown in its own form, by its
/// own move values, and always upward.
/// </summary>
public sealed class KnockedBackState : PlayerState
{
    /// <summary>The way the player is thrown.</summary>
    private readonly Facing _toward;

    /// <summary>How long this knockback lasts, where that is not as long as the player's knockbacks usually do.</summary>
    private int? _updates;

    /// <summary>The updates left before the throw is over.</summary>
    private Countdown _left;

    /// <summary>Whether the throw is kept low, whether it is over and the player is being set down, and whether they are in the low form.</summary>
    private bool _low, _settingDown, _crawling;

    /// <summary>The name of the move values the throw goes by.</summary>
    private string _moves = KnockedBackMoves;

    /// <summary>A knockback throwing the player <paramref name="toward"/>.</summary>
    public KnockedBackState(Facing toward) => _toward = toward;

    /// <summary>A knockback throwing the player <paramref name="toward"/> for <paramref name="updates"/> updates, whatever knockbacks usually last.</summary>
    public KnockedBackState(Facing toward, int updates)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(updates);
        (_toward, _updates) = (toward, updates);
    }

    /// <summary>The updates of the throw still to come.</summary>
    public int UpdatesLeft => _left.Remaining;

    /// <inheritdoc/>
    public override bool CanAttack => false;

    /// <inheritdoc/>
    public override bool InControl => false;

    /// <summary>A knockback that had to wait is shorter by as long as it waited, and one that waited out its whole length does not happen.</summary>
    public override bool StillInterrupts(Player owner, int waited)
    {
        _updates = (_updates ?? owner.KnockbackUpdates) - waited;
        return _updates > 0;
    }

    /// <inheritdoc/>
    public override void Enter(Player owner)
    {
        _crawling = owner.Form == Player.Crawling;
        if (_crawling)
        {
            _moves = owner.IsOnGround ? CrawlingMoves : CrawlingInAirMoves;
        }
        else
        {
            owner.TrySetForm(Player.Standing);
            _low = owner.Input.IsHeld(owner.Facing == Facing.Right ? GameAction.Right : GameAction.Left);
        }

        owner.Rise(Pixels.FromNumber(owner.Medium.KnockbackUpSpeed));
        _left = Countdown.Of(_updates ?? owner.KnockbackUpdates);
    }

    /// <summary>A knockback asks nothing of the input; a game's player may still ask for a way out of it.</summary>
    public override PlayerState? Asked(Player owner) => null;

    /// <summary>Each update of a knockback the player is steered again and their momentum ends; a dash's speed is left.</summary>
    public override PlayerState? Unasked(Player owner)
    {
        owner.Steering = Steering.Steered;
        owner.EndMomentum();
        return null;
    }

    /// <inheritdoc/>
    public override PlayerState? Move(Player owner)
    {
        if (_settingDown)
            return SetDown(owner);

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

        _left = _left.Stepped();
        if (_left.IsRunning)
            return null;

        // The throw is over: the player falls from rest. Whatever their input asked for on this update is dropped.
        owner.FallFromRest();
        if (_crawling)
            return owner.StateAtRest();
        _settingDown = true;
        return this;
    }

    /// <summary>Moves the player down by as far as they move across this update and one pixel more, with no speed of their own.</summary>
    private static MoveResult PressDown(Player owner) => owner.MoveDownBy(owner.SpeedAcross.Abs() + Pixels.FromWhole(1));

    /// <summary>The update after the throw: the player is pressed down once more, and stands if that met ground, or falls.</summary>
    private static PlayerState SetDown(Player owner)
    {
        if (!PressDown(owner).Blocked)
            return new FallingState(offFloor: true);
        owner.Land(Pixels.Zero);
        return new StandingState();
    }
}
