using Engine.Shared;

namespace Engine.Actors;

/// <summary>
/// Turning around in the air. The player faces the new way from the start and goes on moving the old way, reversing,
/// by the turn's move values; each update of the turn ends their momentum. The turn does not land, is not cut short,
/// and does not lose its upward speed against a ceiling; when its time is up the jump or the fall it came from goes on,
/// a jump aiming level in the stance of steering the way the player now faces, and one begun aiming diagonally down
/// aiming straight down.
/// </summary>
public sealed class AirTurningState : PlayerState
{
    /// <summary>What is left of the turn, counted by its pictures.</summary>
    private PictureCount _left;

    /// <summary>A turn in the air out of a jump or out of a fall.</summary>
    public AirTurningState(bool fromJump) => FromJump = fromJump;

    /// <summary>Whether the turn came from a jump, and gives way to one; otherwise from a fall.</summary>
    public bool FromJump { get; }

    /// <summary>Whether the input asks a player in the air to turn: it steers against the way they face.</summary>
    public static bool IsAskedFor(Player owner) => owner.Intent != 0 && (owner.Intent > 0) != (owner.Facing == Facing.Right);

    /// <inheritdoc/>
    public override bool CanAttack => false;

    /// <inheritdoc/>
    public override bool HoldsOffInterruptions => true;

    /// <inheritdoc/>
    public override FormAsked? EntersIn(Player owner) =>
        // A turn is made in the full form of the jump or the fall; with no room for that form there is no turn.
        new FormAsked(owner.FormOr(FromJump ? Player.Jumping : Player.Falling), AroundMiddle: true);

    /// <inheritdoc/>
    public override void Enter(Player owner)
    {
        owner.TurnAround();
        _left = PictureCount.Begin(owner, owner.TurnPictures);
    }

    /// <summary>Anything held keeps the turn as it is; with nothing held at all the player stops reversing.</summary>
    public override PlayerState? Asked(Player owner) => owner.Input.AnyHeld ? this : null;

    /// <inheritdoc/>
    public override PlayerState? Move(Player owner)
    {
        owner.StepDash(building: false);
        owner.Steer(owner.Moves(TurningMoves));
        owner.MoveAcross();

        // A player adrift is held as one on the ground is through the turn: pressed down, gravity left out of it.
        if (owner.IsAdrift)
        {
            owner.PressToGround();
        }
        else
        {
            owner.TurnAtTop();
            owner.MoveDownUnderGravity(keepsSpeedWhenStopped: true);
        }

        owner.LoseMomentum();
        _left = _left.Stepped(owner);
        return null;
    }

    /// <inheritdoc/>
    public override PlayerState? Ended(Player owner)
    {
        if (_left.IsRunning)
            return null;

        // A turn made aiming down, straight or diagonally, comes out aiming straight down.
        if (owner.Aim == Aim.DiagonalDown)
            owner.Aim = Aim.Down;
        if (!FromJump)
            return new FallingState();
        return new JumpingState(owner.Aim == Aim.Forward ? new AirStance(Aim.Forward, AirArm.Forward) : new AirStance(owner.Aim));
    }

    /// <inheritdoc/>
    public override PlayerState? Unasked(Player owner)
    {
        owner.Steering = Steering.Steered;
        owner.EndMomentum();
        return null;
    }
}
