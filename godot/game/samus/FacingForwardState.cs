using Engine.Actors;
using Engine.Shared;

namespace Game;

/// <summary>
/// Facing forward and still, in the hands of something else: in her gunship or on her way into or out of it, or on
/// the pad that sets her down in Ceres. She does nothing and goes where she is put until she is let go.
/// </summary>
/// <param name="turned">
/// Whether she turns to face the screen where she stands, as she does going aboard her gunship, and is lifted as the
/// cartridge's turn lifts her (<see cref="RidingElevatorState.TurnToFaceForward"/>); otherwise she is put in the front
/// view about her middle, as where Ceres begins and where she lands on Zebes.
/// </param>
public sealed class FacingForwardState(bool turned = false) : PlayerState, ISamusLook
{
    /// <inheritdoc/>
    public string Look => "facing_forward";

    /// <inheritdoc/>
    public override bool CanAttack => false;

    /// <inheritdoc/>
    public override bool InControl => false;

    /// <inheritdoc/>
    public override bool InAnothersHands => true;

    /// <summary>
    /// Put in the front view (Ceres's start, the landing on Zebes) a knockback waits, as the cartridge looks at none
    /// until she is let go.
    /// </summary>
    public override bool HoldsOffInterruptions => !turned;

    /// <summary>The front view's own body, taken with no feeling for room, as the cartridge sets the front pose outright.</summary>
    public override FormAsked? EntersIn(Player owner) => new FormAsked(RidingElevatorState.Form, Unfitted: true);

    /// <inheritdoc/>
    public override void Enter(Player owner)
    {
        owner.Velocity = PixelVelocity.Zero;
        // Her charge is kept as it stands, as the cartridge keeps it: she cannot attack here to count it on or let it go.
        owner.LoseMomentum();
        if (turned)
            RidingElevatorState.TurnToFaceForward(owner);
    }

    /// <inheritdoc/>
    public override PlayerState? Move(Player owner)
    {
        owner.Velocity = PixelVelocity.Zero;
        return null;
    }
}
