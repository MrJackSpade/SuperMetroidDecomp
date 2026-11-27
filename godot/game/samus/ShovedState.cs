using Engine.Actors;
using Engine.Shared;

namespace Game;

/// <summary>
/// Samus thrown aside by Ridley as he leaves Ceres: she is flung toward the nearer side of the screen, falling
/// fast, until a wall stops her. Her weapons still answer; her movement does not.
/// </summary>
public sealed class ShovedState : PlayerState, ISamusLook
{
    /// <summary>How fast she falls, in pixels each update.</summary>
    private const int FallSpeed = 5;

    private Facing? _toward;

    /// <inheritdoc/>
    public string Look => "knocked_back";

    /// <inheritdoc/>
    public override bool InControl => false;

    /// <summary>A knockback waits through the shove, as the cartridge's knockback pose she is shoved in refuses one.</summary>
    public override bool HoldsOffInterruptions => true;

    /// <inheritdoc/>
    public override void Enter(Player owner)
    {
        owner.TrySetForm(Player.Standing);
        _toward = null;
    }

    /// <inheritdoc/>
    public override PlayerState? Move(Player owner)
    {
        if (_toward is not { } toward)
        {
            // The first update only chooses the side, by where she is on the screen, and starts her falling.
            int onScreen = owner.Position.X.Whole - owner.Room.Session.Camera.Left;
            _toward = onScreen < owner.Room.Session.Camera.View.Width / 2 ? Facing.Left : Facing.Right;
            owner.Velocity = new PixelVelocity(Pixels.Zero, Pixels.FromWhole(FallSpeed));
            return null;
        }

        owner.Velocity = new PixelVelocity(toward.Toward(owner.Moves(KnockedBackMoves).Top), Pixels.FromWhole(FallSpeed));
        if (owner.MoveAcross().Blocked)
        {
            owner.LoseMomentum();
            owner.Velocity = PixelVelocity.Zero;
            return owner.StateAtRest();
        }

        owner.MoveDown();
        return null;
    }
}
