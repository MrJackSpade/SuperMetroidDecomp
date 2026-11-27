using Engine.Actors;
using Engine.Collision;
using Engine.Shared;

namespace Game;

/// <summary>
/// A bomb jump: one of her own bombs going off against her throws her upward, and to one side when the bomb was off
/// to the other. The morph ball is thrown as it is; upright, she is thrown in the pose of a jump with her arm level,
/// facing as she did. Its first update only sets the rise. Then she rises out of her control: thrown to a side she
/// takes up whatever speed across she had and gains by the throw's own move values; a wall stops that speed and not
/// the rise. The throw is over at the top of the rise, the update after gravity has slowed it past rest, or at a
/// ceiling, and she is her own again: the ball, or in the jump she was thrown in.
/// </summary>
public sealed class BombJumpingState : PlayerState, ISamusLook
{
    /// <summary>The name of the move values a bomb jump thrown to one side goes by.</summary>
    public const string Moves = "bomb_jumping";

    /// <summary>The name, in each medium's speeds, of the upward speed a bomb jump starts with.</summary>
    public const string Speed = "bomb_jump";

    /// <summary>The way the ball is thrown across: -1 left, 1 right, 0 straight up.</summary>
    private readonly int _across;

    /// <summary>Whether the throw has yet to set the ball rising.</summary>
    private bool _starting = true;

    /// <summary>
    /// A bomb jump throwing her the given way across: the ball, or upright. <paramref name="ballWasAirborne"/> says
    /// the ball was moving as one in the air when it was thrown, which it goes on counting as: the cartridge throws
    /// the ball in the pose it had.
    /// </summary>
    public BombJumpingState(int across, bool upright = false, bool ballWasAirborne = false) =>
        (_across, Upright, MovesAsAirborne) = (Math.Sign(across), upright, upright || ballWasAirborne);

    /// <summary>Whether she is thrown upright, in a jump's pose, and not as the ball.</summary>
    public bool Upright { get; }

    /// <summary>
    /// Whether she counts as moving through the air while thrown (<see cref="Samus.MovesAsAirborne"/>): upright she
    /// does, being in a jump's pose; the ball does only if it did when it was thrown, so that a ball thrown from the
    /// ground counts as on it all the way up.
    /// </summary>
    public bool MovesAsAirborne { get; }

    /// <inheritdoc/>
    public string Look => Upright ? "jumping" : "morph_ball";

    /// <summary>
    /// An upright throw whose jump's body did not fit stays asked for, as the cartridge keeps the bomb's request until
    /// her pose handling takes it or turns it down: it is tried again each update, crouched under a low ceiling too,
    /// and is thrown late once the body fits.
    /// </summary>
    public override bool WaitsWhenItDoesNotFit => Upright;

    /// <summary>
    /// Each time it is tried, an upright throw is turned down for good if what she is doing by then refuses a bomb's
    /// throw, as a jump or a turn does (<see cref="Samus.TakesUprightBombJump"/>). The ball's is never turned down.
    /// </summary>
    public override bool StillInterrupts(Player owner, int waited) => !Upright || ((Samus)owner).TakesUprightBombJump;

    /// <summary>
    /// Upright she is thrown in the jump's body, fitted as the cartridge fits a larger pose: with no room for it she
    /// is not thrown, and with none above or below she is put in the crouch. The body is hers at once, as the
    /// cartridge sets the size of the jump pose it throws her in as it puts her in it. The ball is thrown as it is.
    /// </summary>
    public override FormAsked? EntersIn(Player owner) =>
        Upright ? new FormAsked(owner.FormOr(Player.Jumping), AroundMiddle: true, AtOnce: true) : null;

    /// <inheritdoc/>
    public override void Enter(Player owner)
    {
        if (Upright)
            owner.Aim = Aim.Forward;
    }

    /// <summary>What she is once the throw is over: the ball, or the jump she was thrown in, falling.</summary>
    private PlayerState Thrown() => Upright ? new JumpingState(keepsSpeed: true) : new CrawlingState();

    /// <inheritdoc/>
    public override PlayerState? Move(Player owner)
    {
        if (_starting)
        {
            // The start sets the rise and moves the ball nowhere.
            owner.Rise(owner.Medium.Speed(Speed));
            _starting = false;
            return null;
        }

        if (_across != 0)
        {
            Facing toward = _across > 0 ? Facing.Right : Facing.Left;
            owner.Steer(owner.Moves(Moves));
            owner.Velocity = owner.Velocity.WithX(toward.Toward(owner.Velocity.X.Abs()));
            owner.MoveAcross();
        }

        // At the top the rise becomes a fall from rest and the throw is over before the ball moves; thrown to a side,
        // it goes on unsteered.
        if (owner.TurnAtTop())
        {
            if (_across != 0)
                owner.Steering = Steering.Unsteered;
            return Thrown();
        }

        if (!owner.IsRising)
            return Thrown();
        MoveResult up = owner.MoveDownUnderGravity();
        return up.Blocked ? Thrown() : null;
    }
}
