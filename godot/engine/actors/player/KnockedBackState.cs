using Engine.Input;
using Engine.Shared;

namespace Engine.Actors;

/// <summary>
/// Knocked back by a hit: an upright player thrown away from it for a moment, out of their control, still facing as
/// they were. The throw itself (<see cref="Throw"/>) moves them; this is what they are meanwhile. The throw is kept
/// low when the player was holding the way they face as it began. Each update they are steered again and their
/// momentum ends. When the throw's time is up they spend one update more being set down, and then stand or fall.
/// The low form is not put in this state: it is thrown as it is, in its own (<see cref="Player.Admit"/>).
/// </summary>
public sealed class KnockedBackState : PlayerState
{
    /// <summary>The way the player is thrown.</summary>
    private readonly Facing _toward;

    /// <summary>How long this knockback lasts, where that is not as long as the player's knockbacks usually do.</summary>
    private int? _updates;

    /// <summary>A knockback throwing the player <paramref name="toward"/>.</summary>
    public KnockedBackState(Facing toward) => _toward = toward;

    /// <summary>A knockback throwing the player <paramref name="toward"/> for <paramref name="updates"/> updates, whatever knockbacks usually last.</summary>
    public KnockedBackState(Facing toward, int updates)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(updates);
        (_toward, _updates) = (toward, updates);
    }

    /// <inheritdoc/>
    public override bool CanAttack => false;

    /// <inheritdoc/>
    public override bool InControl => false;

    /// <summary>For how many updates it has waited to begin.</summary>
    private int _waited;

    /// <summary>How long the knockback lasts from here: its own length, less as long as it has waited.</summary>
    private int Length(Player owner) => (_updates ?? owner.KnockbackUpdates) - _waited;

    /// <summary>A knockback that had to wait is shorter by as long as it waited, and one that waited out its whole length does not happen.</summary>
    public override bool StillInterrupts(Player owner, int waited, PlayerState? asked)
    {
        _waited = waited;
        return Length(owner) > 0;
    }

    /// <summary>
    /// An upright player is knocked back in the upright form, fitted as any taller form is: where it does not fit
    /// they are not knocked back, and are put in whatever form the fitting answers with.
    /// </summary>
    public override FormAsked? EntersIn(Player owner) => new FormAsked(Player.Standing);

    /// <summary>
    /// A knockback whose form did not fit goes on waiting for as long as it would have lasted, and begins, the
    /// shorter for its wait, on the first update the form fits.
    /// </summary>
    public override bool WaitsWhenItDoesNotFit => true;

    /// <summary>
    /// The throw this knockback gives a player in the low form, who is thrown as they are: by the low form's own move
    /// values, on the ground or in the air as they are when it begins, and always upward. Which of the two they are
    /// is the low form's own account of itself (<see cref="CrawlingState.IsAirborne"/>), not a look at what lies
    /// under them: one rolling along sloped ground is on the ground.
    /// </summary>
    internal Throw ThrowOfLowForm(Player owner)
    {
        bool onGround = owner.State is CrawlingState crawling ? !crawling.IsAirborne : owner.IsOnGround;
        return new(_toward, Length(owner), low: false, onGround ? CrawlingMoves : CrawlingInAirMoves);
    }

    /// <summary>Renews <paramref name="under"/>, the throw already carrying the player, with this knockback's way and time.</summary>
    internal void Renew(Player owner, Throw under) => under.Renew(_toward, Length(owner));

    /// <inheritdoc/>
    public override void Enter(Player owner)
    {
        bool low = owner.Input.IsHeld(owner.Facing == Facing.Right ? GameAction.Right : GameAction.Left);
        owner.BeThrown(new Throw(_toward, Length(owner), low, KnockedBackMoves));
    }

    /// <summary>Left for another state, as a game's way out of a knockback leaves it, the throw is over.</summary>
    public override void Exit(Player owner) => owner.EndThrow();

    /// <summary>A knockback asks nothing of the input; a game's player may still ask for a way out of it.</summary>
    public override PlayerState? Asked(Player owner) => null;

    /// <summary>Each update of a knockback the player is steered again and their momentum ends; a dash's speed is left.</summary>
    public override PlayerState? Unasked(Player owner)
    {
        owner.Steering = Steering.Steered;
        owner.EndMomentum();
        return null;
    }

    /// <summary>
    /// The throw moves the player for as long as it lasts, so this is the update after it: they are pressed down
    /// once more as one on the ground is, by as far as the throw last set out to carry them across, whatever a wall
    /// then left of their speed, and a pixel more; and stand if that met ground, or fall.
    /// </summary>
    public override PlayerState? Move(Player owner)
    {
        if (!owner.PressToGround())
            return new FallingState(offFloor: true);
        owner.Land(Pixels.Zero);
        return new StandingState();
    }
}
