using Engine.Actors;
using Engine.Input;
using Engine.Shared;
using Game.Data;

namespace Game;

/// <summary>
/// Held in place by the grapple beam, as the cartridge holds her when the line takes hold fired level or downward
/// from a body that is not moving up or down: she does not swing. She is put where the line's end is, less where a
/// line fired so leaves a standing body, and stays there, standing or crouched as she was, for as long as the
/// attack is held and the line holds. Then the line ends on the update after, and she stands or crouches again with
/// her speeds cleared.
/// </summary>
public sealed class GrappleLockedState : PlayerState, ISamusLook
{
    private readonly bool _crouched;

    /// <summary>Held by the line, crouched or standing.</summary>
    public GrappleLockedState(bool crouched) => _crouched = crouched;

    /// <inheritdoc/>
    public string Look => _crouched ? "grapple_crouching" : "grapple_standing";

    /// <inheritdoc/>
    public override bool CanAttack => false;

    /// <inheritdoc/>
    public override void Enter(Player owner)
    {
        var samus = (Samus)owner;
        SamusGrapple grapple = samus.Grapple ?? throw new InvalidOperationException("Samus is held by a grapple beam she has not got.");
        GrappleSwingRules rules = grapple.Swing;
        owner.LoseMomentum();
        owner.Velocity = PixelVelocity.Zero;

        // The line's end is reckoned as a swing's is, from the angle of where she was and the line's length as it
        // took hold, a long line shortened; she is put there, less where the line leaves a standing body.
        int length = grapple.Length >= rules.LongLine ? grapple.Length - rules.LongLineShortening : grapple.Length;
        PixelOffset hanging = owner.Position - grapple.Anchor;
        PixelPosition end = GrapplingState.EndOfLine(rules, grapple.Anchor, GrapplingState.AngleOf(hanging.X.Whole, hanging.Y.Whole), length);
        owner.Position = end - samus.StandingGrappleOrigin;
    }

    /// <inheritdoc/>
    public override PlayerState? Move(Player owner)
    {
        var samus = (Samus)owner;
        SamusGrapple? grapple = samus.Grapple;
        if (grapple is { IsLatched: true } && owner.Input.IsHeld(GameAction.Attack) && samus.Selected == Equipment.GrappleBeam)
            return null;

        // Let go, or the line lost: the line ends on the update after, and she is as she was before it held her.
        grapple?.EndOnNextUpdate();
        owner.Velocity = PixelVelocity.Zero;
        return _crouched ? new CrouchingState() : new StandingState();
    }
}
