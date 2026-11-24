using Engine.Actors;
using Engine.Collision;
using Engine.Shared;

namespace Game;

/// <summary>
/// A bomb jump: one of her own bombs going off against the morph ball throws it upward, and to one side when the bomb
/// was off to the other. Its first update only sets the rise. Then the ball rises out of her control: thrown to a side
/// it takes up whatever speed across it had and gains by the throw's own move values; a wall stops that speed and not
/// the rise. The throw is over at the top of the rise, the update after gravity has slowed it past rest, or at a
/// ceiling, and the ball is hers again.
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

    /// <summary>A bomb jump throwing the ball the given way across.</summary>
    public BombJumpingState(int across) => _across = Math.Sign(across);

    /// <inheritdoc/>
    public string Look => "morph_ball";

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
            return new CrawlingState();
        }

        if (!owner.IsRising)
            return new CrawlingState();
        MoveResult up = owner.MoveDownUnderGravity();
        return up.Blocked ? new CrawlingState() : null;
    }
}
