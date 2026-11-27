using Engine.Actors;
using Engine.Input;
using Engine.Shared;
using Game.Compatibility;
using Game.Data;

namespace Game;

/// <summary>
/// Held in place by the grapple beam, as the cartridge holds her when the line takes hold fired level or downward
/// from a body that is not moving up or down: she does not swing. She is put where the line's end is, less where a
/// line fired so leaves a standing body, and stays there, standing or crouched as she was, for as long as the
/// attack is held and the line holds. Then, on the update after, the line ends and she stands or crouches again with
/// her speeds cleared; she does not move meanwhile.
/// </summary>
public sealed class GrappleLockedState : PlayerState, ISamusLook
{
    private readonly bool _crouched;
    private readonly PixelPosition? _at;
    private readonly Aim? _aim;

    /// <summary>Held by the line as it takes hold, crouched or standing.</summary>
    public GrappleLockedState(bool crouched) => _crouched = crouched;

    /// <summary>Held by the line at a place and with an aim of the holder's own: a swing stopped by her hand at one of its exact angles.</summary>
    public GrappleLockedState(bool crouched, PixelPosition at, Aim aim) => (_crouched, _at, _aim) = (crouched, at, aim);

    /// <inheritdoc/>
    public string Look => _crouched ? "grapple_crouching" : "grapple_standing";

    /// <inheritdoc/>
    public override bool CanAttack => false;

    /// <summary>A knockback waits while she is in this state, as the cartridge refuses it here and tries again each update of its time.</summary>
    public override bool HoldsOffInterruptions => true;

    /// <inheritdoc/>
    public override void Enter(Player owner)
    {
        var samus = (Samus)owner;
        SamusGrapple grapple = samus.Grapple ?? throw new InvalidOperationException("Samus is held by a grapple beam she has not got.");
        GrappleSwingRules rules = grapple.Swing;
        owner.LoseMomentum();
        owner.Velocity = PixelVelocity.Zero;
        if (_at is { } at && _aim is { } aim)
        {
            // She is put there outright and takes the pose's body where she is, as the cartridge sets a pose's size.
            (owner.Aim, owner.Position) = (aim, at);
            owner.TrySetForm(owner.FormOr(_crouched ? Player.Crouching : Player.Standing), aroundMiddle: true);
            samus.CapViewAfterPlacing();
            return;
        }

        // The line's end is reckoned as a swing's is, from the angle of where she was and the line's length as it
        // took hold, a long line shortened; she is put there, less where the line leaves a standing body.
        int length = grapple.Length >= rules.LongLine ? grapple.Length - rules.LongLineShortening : grapple.Length;
        PixelOffset hanging = owner.Position - grapple.Anchor;
        int angle = CartridgeAngles.AngleClockwiseFromUp(hanging.X.Whole, hanging.Y.Whole);
        PixelPosition end = GrapplingState.EndOfLine(rules, grapple.Anchor, angle, length);
        owner.Position = end - samus.StandingGrappleOrigin;
        grapple.ShowLineAs(angle, length);

        // Taking hold puts her where the line holds her, and the view follows that by no more than its cap.
        samus.CapViewAfterPlacing();
    }

    /// <inheritdoc/>
    public override PlayerState? Move(Player owner)
    {
        // Let go on the update before, she is as she was before the line held her, her speeds cleared, as the
        // cartridge's teardown puts her on the update after it is decided.
        if (_lettingGo)
        {
            owner.Velocity = PixelVelocity.Zero;
            return _crouched ? new CrouchingState() : new StandingState();
        }

        var samus = (Samus)owner;
        SamusGrapple? grapple = samus.Grapple;
        if (grapple is { IsLatched: true } && owner.Input.IsHeld(GameAction.Attack) && samus.Selected == Equipment.GrappleBeam)
            return null;

        // Let go, or the line lost: the line ends on the update after, and so does her hold, which keeps her where
        // she is until then.
        grapple?.EndOnNextUpdate();
        _lettingGo = true;
        return null;
    }

    /// <summary>Whether she has let go, to be let go of on the next update.</summary>
    private bool _lettingGo;
}
