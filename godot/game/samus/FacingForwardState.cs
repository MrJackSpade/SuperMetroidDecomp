using Engine.Actors;
using Engine.Shared;

namespace Game;

/// <summary>
/// Facing forward and still, in the hands of something else: in her gunship or on her way into or out of it, or on
/// the pad that sets her down in Ceres. She does nothing and goes where she is put until she is let go.
/// </summary>
public sealed class FacingForwardState : PlayerState, ISamusLook
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
    /// Whether she is put in the front view's height where she is, with no room felt for around her, as the cartridge
    /// sets her up on Ceres's pad at the top of its room; otherwise she is fitted to it where she stands.
    /// </summary>
    public bool PutWhereSheIs { get; init; }

    /// <inheritdoc/>
    public override FormAsked? EntersIn(Player owner) => PutWhereSheIs ? new FormAsked(RidingElevatorState.Form, Unfitted: true) : null;

    /// <inheritdoc/>
    public override void Enter(Player owner)
    {
        owner.Velocity = PixelVelocity.Zero;
        // Her charge is kept as it stands, as the cartridge keeps it: she cannot attack here to count it on or let it go.
        owner.LoseMomentum();

        // The front view's own height: taller than standing, so on the ground she is lifted by the difference.
        if (!PutWhereSheIs)
            owner.TrySetForm(RidingElevatorState.Form);
    }

    /// <inheritdoc/>
    public override PlayerState? Move(Player owner)
    {
        owner.Velocity = PixelVelocity.Zero;
        return null;
    }
}
