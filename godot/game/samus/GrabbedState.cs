using Engine.Actors;
using Engine.Input;
using Engine.Shared;
using Game.Data;

namespace Game;

/// <summary>
/// Seized and carried by something, as Draygon carries her: she goes where she is taken and cannot move, but can
/// still aim and fire. From some holds she struggles free by pressing a new way on the pad, again and again;
/// whatever holds her sees that she is no longer in this state.
/// </summary>
public sealed class GrabbedState : PlayerState, ISamusLook
{
    private readonly int _pressesToEscape;
    private int _presses;
    private int _lastWays;

    /// <summary>
    /// A hold she escapes with <paramref name="pressesToEscape"/> new presses of a way on the pad; with none, a hold
    /// she cannot struggle out of.
    /// </summary>
    public GrabbedState(int pressesToEscape)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(pressesToEscape);
        _pressesToEscape = pressesToEscape;
    }

    /// <inheritdoc/>
    public string Look => "grabbed";

    /// <summary>
    /// How she holds herself in the grab, by what is held, asked in the cartridge's order: the attack with the way
    /// she faces and up or down aims diagonally; the attack with the way she faces alone fires; then either angle
    /// action aims; then the attack alone fires; then any direction is the moving pose; and nothing held is none.
    /// </summary>
    public string Hold { get; private set; } = "forward";

    private static string HoldOf(Player owner)
    {
        GameInput input = owner.Input;
        bool attack = input.IsHeld(GameAction.Attack), up = input.IsHeld(GameAction.Up), down = input.IsHeld(GameAction.Down);
        bool left = input.IsHeld(GameAction.Left), right = input.IsHeld(GameAction.Right);
        bool outward = owner.Facing == Facing.Right ? right : left;
        if (attack && outward && up)
            return "diagonal_up";
        if (attack && outward && down)
            return "diagonal_down";
        if (attack && outward)
            return "firing";
        if (input.IsHeld(Actions.AngleUp))
            return "diagonal_up";
        if (input.IsHeld(Actions.AngleDown))
            return "diagonal_down";
        if (attack)
            return "firing";
        return left || right || up || down ? "moving" : "forward";
    }

    /// <inheritdoc/>
    public override bool InControl => false;

    /// <summary>A knockback waits while she is in this state, as the cartridge refuses it here and tries again each update of its time.</summary>
    public override bool HoldsOffInterruptions => true;

    /// <summary>
    /// The upright body, taken where she is with no feeling for room and hers at once, as the cartridge sets the grabbed
    /// pose and its size outright as it seizes her: whatever holds her has her, whatever was about her.
    /// </summary>
    public override FormAsked? EntersIn(Player owner) => new FormAsked(Player.Standing, Unfitted: true, AtOnce: true);

    /// <inheritdoc/>
    public override void Enter(Player owner)
    {
        owner.Velocity = PixelVelocity.Zero;
        owner.LoseMomentum();
    }

    /// <inheritdoc/>
    public override PlayerState? Move(Player owner)
    {
        var samus = (Samus)owner;
        GameInput input = owner.Input;
        owner.AimFromInput();
        Hold = HoldOf(owner);

        // Her grapple beam still works: it holds while the button is held, though she does not swing from it.
        if (samus.Grapple is { } grapple)
        {
            bool wanted = samus.Selected == Equipment.GrappleBeam && input.IsHeld(GameAction.Attack);
            if (grapple.IsLatched && !wanted)
                grapple.Release();
            else if (wanted && input.WasPressed(GameAction.Attack))
                grapple.Fire(samus);
        }

        // Only a press of the ways that differs from the last one counted is a struggle.
        int ways = (input.WasPressed(GameAction.Left) ? 1 : 0) | (input.WasPressed(GameAction.Right) ? 2 : 0) |
            (input.WasPressed(GameAction.Up) ? 4 : 0) | (input.WasPressed(GameAction.Down) ? 8 : 0);
        if (ways != 0 && ways != _lastWays)
        {
            _lastWays = ways;
            // Free, she stands, on the ground or not, as the cartridge lets her go.
            if (_pressesToEscape > 0 && ++_presses >= _pressesToEscape)
                return new StandingState();
        }

        return null;
    }

    /// <inheritdoc/>
    public override void Exit(Player owner)
    {
        if (owner is Samus { Grapple: { IsOut: true } grapple })
            grapple.Release();
    }
}
