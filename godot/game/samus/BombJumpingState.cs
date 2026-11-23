using Engine.Actors;
using Engine.Actors.Components;
using Engine.Collision;
using Engine.Shared;

namespace Game;

/// <summary>
/// A bomb jump: one of her own bombs going off beneath the morph ball throws it upward, and to one side when the bomb
/// was off to the other. The ball rises and comes back down out of her control, and then is hers again.
/// </summary>
public sealed class BombJumpingState : PlayerState, ISamusLook
{
    /// <summary>The name of the move values a bomb jump thrown to one side goes by.</summary>
    public const string Moves = "bomb_jumping";

    /// <summary>The name, in each medium's speeds, of the upward speed a bomb jump starts with.</summary>
    public const string Speed = "bomb_jump";

    /// <summary>The way the ball is thrown across: -1 left, 1 right, 0 straight up.</summary>
    private readonly int _across;

    /// <summary>A bomb jump throwing the ball the given way across.</summary>
    public BombJumpingState(int across) => _across = Math.Sign(across);

    /// <inheritdoc/>
    public string Look => "morph_ball";

    /// <inheritdoc/>
    public override void Enter(Player owner)
    {
        owner.LoseMomentum();
        owner.Velocity = new PixelVelocity(Pixels.Zero, -owner.Medium.Speed(Speed));
    }

    /// <inheritdoc/>
    public override State<Player> Step(Player owner)
    {
        if (_across != 0)
        {
            MoveValues moves = owner.Moves(Moves);
            Pixels faster = owner.Velocity.X.Abs() + moves.Gain;
            owner.Velocity = owner.Velocity.WithX((faster > moves.Top ? moves.Top : faster) * _across);
        }

        bool stoppedAcross = owner.MoveAcross().Blocked;
        bool rising = owner.Velocity.Y.IsNegative;
        MoveResult down = owner.MoveDown();
        owner.Fall();
        // The throw is over at the top of its rise, or as soon as anything stops it.
        if (!rising || down.Blocked || stoppedAcross)
            return new CrawlingState();
        return this;
    }
}
