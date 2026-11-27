using Engine.Actors;
using Engine.Shared;

namespace Game;

/// <summary>
/// Appearing where a saved game is taken up: she faces out of the screen, still, answering nothing she is asked, while
/// her suit comes to its colours and her fanfare plays, for a set number of her updates. On the last of them she is
/// given back to herself, standing as she is (<see cref="StandingForwardState"/>) and still shown as her appearance
/// left her, until she is turned.
/// </summary>
public sealed class AppearingState : PlayerState, ISamusLook
{
    /// <summary>The look she appears in, which her standing after it goes on showing.</summary>
    public const string Appearance = "appearing";

    /// <summary>How many of her updates the appearance lasts.</summary>
    private const int Updates = 360;

    private int _updatesLeft = Updates;

    /// <inheritdoc/>
    public string Look => Appearance;

    /// <inheritdoc/>
    public override bool CanAttack => false;

    /// <inheritdoc/>
    public override bool InControl => false;

    /// <inheritdoc/>
    public override bool InAnothersHands => true;

    /// <summary>
    /// A knockback waits while she appears, as the cartridge looks at none until her appearance has ended, and is
    /// taken on its last update, in place of her standing, if its time has not run out.
    /// </summary>
    public override bool HoldsOffInterruptions => _updatesLeft > 0;

    /// <summary>
    /// The place a game is taken up at is her middle's in the front view, as the cartridge's table of them has it: she
    /// is given the front view's body about her middle with no feeling for room, as the cartridge puts her in the
    /// front pose outright, which sets her feet on the ground there.
    /// </summary>
    public override FormAsked? EntersIn(Player owner) => new FormAsked(RidingElevatorState.Form, Unfitted: true);

    /// <inheritdoc/>
    public override void Enter(Player owner) => owner.Velocity = PixelVelocity.Zero;

    /// <inheritdoc/>
    public override PlayerState? Move(Player owner)
    {
        owner.Velocity = PixelVelocity.Zero;
        _updatesLeft--;
        return _updatesLeft == 0 ? new StandingForwardState(Appearance) : null;
    }
}
