using Engine.Actors;
using Engine.Collision;
using Engine.Input;
using Engine.Shared;
using Game.Data;

namespace Game;

/// <summary>
/// A spin jump: the somersault a jump becomes when she is moving. It keeps its speed across without steering, can
/// jump off walls, jumps again in mid-air with the space jump, and hurts what it touches with the screw attack, or
/// with a fully charged beam. Aiming or firing straightens her out of it.
/// </summary>
public sealed class SpinJumpingState : PlayerState, ISamusLook
{
    /// <summary>The name of the move values a spin jump is steered by.</summary>
    public const string Moves = "spin_jumping";

    /// <summary>The name, in each medium's speeds, of the least falling speed at which the space jump works.</summary>
    public const string SpaceJumpLeastFall = "space_jump_least_fall";

    /// <summary>The name, in each medium's speeds, of the falling speed at and above which the space jump no longer works.</summary>
    public const string SpaceJumpMostFall = "space_jump_most_fall";

    private readonly bool _keepsSpeed;
    private readonly Facing? _toward;

    /// <summary>A spin jump from the ground.</summary>
    public SpinJumpingState()
    {
    }

    /// <summary>A spin jump from the ground that turns her to face <paramref name="toward"/> as it starts.</summary>
    public SpinJumpingState(Facing toward) => _toward = toward;

    /// <summary>A spin jump that carries on with the upward speed she already has, as after a wall jump.</summary>
    public SpinJumpingState(bool keepsSpeed) => _keepsSpeed = keepsSpeed;

    /// <summary>The look of the spin she is in: the screw attack's, the space jump's or the plain one.</summary>
    public string Look { get; private set; } = "spin_jumping";

    /// <inheritdoc/>
    public override void Enter(Player owner)
    {
        if (_toward is { } toward)
            owner.Facing = toward;
        owner.TrySetForm(owner.FormOr(Samus.Spinning), aroundMiddle: true);
        Look = owner.Inventory.IsOn(Equipment.ScrewAttack) ? "screw_attacking" : owner.Inventory.IsOn(Equipment.SpaceJump) ? "space_jumping" : "spin_jumping";
        if (_keepsSpeed)
            return;
        owner.Velocity = owner.Velocity.WithY(-owner.LaunchSpeed(offWall: false));
        owner.Room.Session.Sounds.Request(SpinSound(owner), channel: "samus");
    }

    /// <inheritdoc/>
    public override PlayerState? Asked(Player owner)
    {
        // Aiming or firing straightens her out of the spin once this update's movement is done.
        owner.AimFromInput();
        if (owner.Aim != Aim.Forward || owner.Input.WasPressed(GameAction.Attack))
            return owner.Velocity.Y.IsNegative ? new JumpingState(keepsSpeed: true) : new FallingState();
        return this;
    }

    /// <inheritdoc/>
    public override PlayerState? Move(Player owner)
    {
        var samus = (Samus)owner;
        if (WallJumpingState.CanStart(owner))
        {
            owner.Room.Session.Sounds.Request(SpinSound(owner), channel: "samus");
            return new WallJumpingState();
        }

        // Wholly under a liquid that hinders her, the spin neither jumps again nor hurts anything.
        bool swamped = owner.IsWhollyInLiquid;
        if (!swamped)
        {
            TrySpaceJump(owner);
            if (owner.Inventory.IsOn(Equipment.ScrewAttack) && samus.Power(HitKinds.ScrewAttack) is { } screw)
                screw.IsActive = true;
            else if (owner.Weapons.ChargedFor >= samus.ChargedSpinUpdates && samus.Power(HitKinds.ChargedSpin) is { } charged)
                charged.IsActive = true;
        }

        JumpingState.CutShort(owner);
        owner.StepDash(building: false);
        owner.SteerPlainly(owner.Moves(Moves));
        owner.MoveAcross();
        bool falling = !owner.Velocity.Y.IsNegative;
        MoveResult down = owner.MoveDown();
        owner.Fall();
        if ((down.Blocked && falling) || (falling && owner.IsOnGround))
            return owner.Intent != 0 ? new RunningState() : new StandingState();

        return null;
    }

    /// <summary>
    /// Jumps again in mid-air when the space jump is on: on a fresh press of jump while she is falling, and only while
    /// her falling speed is within the medium's window.
    /// </summary>
    private static void TrySpaceJump(Player owner)
    {
        if (!owner.Inventory.IsOn(Equipment.SpaceJump) || !owner.Input.WasPressed(GameAction.Jump))
            return;
        Pixels falling = owner.Velocity.Y;
        if (falling.IsNegative || falling < owner.Medium.Speed(SpaceJumpLeastFall) || falling >= owner.Medium.Speed(SpaceJumpMostFall))
            return;
        owner.Velocity = owner.Velocity.WithY(-owner.LaunchSpeed(offWall: false));
        owner.Room.Session.Sounds.Request(SpinSound(owner), channel: "samus");
    }

    /// <summary>The sound of the spin she is in.</summary>
    private static string SpinSound(Player owner) =>
        owner.IsWhollyInLiquid ? Sounds.UnderwaterSpinJump
        : owner.Inventory.IsOn(Equipment.ScrewAttack) ? Sounds.ScrewAttack
        : owner.Inventory.IsOn(Equipment.SpaceJump) ? Sounds.SpaceJump
        : Sounds.SpinJump;
}
