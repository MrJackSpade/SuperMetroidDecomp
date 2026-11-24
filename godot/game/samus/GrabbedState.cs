using Engine.Actors;
using Engine.Actors.Components;
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

    /// <inheritdoc/>
    public override bool InControl => false;

    /// <inheritdoc/>
    public override void Enter(Player owner)
    {
        owner.Velocity = PixelVelocity.Zero;
        owner.LoseMomentum();
        owner.TrySetForm(Player.Standing);
    }

    /// <inheritdoc/>
    public override State<Player> Step(Player owner)
    {
        var samus = (Samus)owner;
        GameInput input = owner.Input;
        owner.AimFromInput();

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
            if (_pressesToEscape > 0 && ++_presses >= _pressesToEscape)
                return owner.StateAtRest();
        }

        return this;
    }

    /// <inheritdoc/>
    public override void Exit(Player owner)
    {
        if (owner is Samus { Grapple: { IsOut: true } grapple })
            grapple.Release();
    }
}
