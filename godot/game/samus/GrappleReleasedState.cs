using Engine.Actors;
using Engine.Collision;
using Engine.Shared;

namespace Game;

/// <summary>
/// Her flight after letting go of a swing on the grapple beam, as the cartridge flies her: it is not a jump. It
/// begins on the update the swing lets go, with the speeds the swing gave her, and moves her on that update too. Her
/// speed across only slows, by the release's own move values, whatever is held; her speed up wears away under
/// gravity. It is over, and she is in an ordinary jump from the next update, once her rise has passed its top, on
/// any update she moves down, or at a ceiling. On the update it begins she is still shown as she swung.
/// </summary>
public sealed class GrappleReleasedState : PlayerState, ISamusLook
{
    /// <summary>The name of the move values the flight slows by, the same in every medium.</summary>
    public const string Moves = "grapple_release";

    private readonly string _swingLook;
    private int _updates;

    /// <summary>A flight out of a swing that was shown as <paramref name="swingLook"/>.</summary>
    public GrappleReleasedState(string swingLook)
    {
        ArgumentException.ThrowIfNullOrEmpty(swingLook);
        _swingLook = swingLook;
    }

    /// <summary>The swing's look on the update the flight begins, and a jump's from the next.</summary>
    public string Look => _updates <= 1 ? _swingLook : "jumping";

    /// <inheritdoc/>
    public override bool CanAttack => false;

    /// <summary>
    /// The jump's body, taken with no feeling for room and hers at once, as the cartridge sets its release pose, a
    /// jump's, outright once the update's first move of the flight is made in the swing's body.
    /// </summary>
    public override FormAsked? EntersIn(Player owner) => new FormAsked(owner.FormOr(Player.Jumping), Unfitted: true, AtOnce: true);

    /// <inheritdoc/>
    public override PlayerState? Move(Player owner)
    {
        _updates++;

        // A rise gravity has slowed past rest becomes a fall from rest, and the flight is over, though this update
        // of it is still played.
        bool over = owner.TurnAtTop();

        // Her speed across always slows; slowed past rest it is at rest.
        owner.Steering = Steering.Unsteered;
        owner.Steer(owner.Moves(Moves));
        owner.MoveAcross();

        // Only a free move upward keeps the flight for another update.
        bool rising = owner.IsRising;
        MoveResult moved = owner.MoveDownUnderGravity();
        if (!rising || moved.Blocked)
            over = true;
        return over ? new JumpingState(keepsSpeed: true) : null;
    }
}
