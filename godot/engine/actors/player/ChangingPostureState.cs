using Engine.Collision;
using Engine.Shared;

namespace Engine.Actors;

/// <summary>
/// Passing from one posture to another: crouching down, standing up, taking the low form or leaving it, where the
/// game gives the change a length. The body takes its new form at once. For as long as the change lasts the input is
/// not asked and the player moves across only as far as a dash carries them. Through a change between crouching and
/// standing they can attack, aiming as they did when it began; through one into or out of the low form they cannot.
/// A change into or out of the low form made in the air goes on falling, unless the player is adrift, who is pressed
/// down as one on the ground is. Then the state it leads to follows.
/// </summary>
public sealed class ChangingPostureState : PlayerState
{
    private readonly string _form;
    private readonly bool _fallsInAir;
    private readonly bool _attacks;
    private PictureCount _left;

    /// <summary>A change of posture.</summary>
    /// <param name="form">The form the body takes as the change begins.</param>
    /// <param name="then">The state the change leads to.</param>
    /// <param name="count">The count of the change's pictures, begun: the change lasts as long as it runs.</param>
    /// <param name="fallsInAir">Whether a change made in the air goes on rising and falling; otherwise the player hangs for its length.</param>
    /// <param name="attacks">Whether the player can attack for as long as the change lasts.</param>
    public ChangingPostureState(string form, PlayerState then, PictureCount count, bool fallsInAir, bool attacks)
    {
        ArgumentNullException.ThrowIfNull(then);
        (_form, Then, _fallsInAir, _attacks) = (form, then, fallsInAir, attacks);
        _left = count;
    }

    /// <summary>The state the change leads to.</summary>
    public PlayerState Then { get; }

    /// <inheritdoc/>
    public override bool CanAttack => _attacks;

    /// <inheritdoc/>
    public override bool HoldsOffInterruptions => true;

    /// <summary>
    /// A change into a taller form is fitted as the state is entered, where the update's movement has left the
    /// player: with no room for it there the change is not begun, and they stay as they are. A smaller form is taken
    /// as the state is entered.
    /// </summary>
    public override FormAsked? EntersIn(Player owner) =>
        _form != Player.Crawling && owner.HalfHeightOf(_form) > owner.HalfHeight ? new FormAsked(_form) : null;

    /// <summary>
    /// Whether the player is in the air through the change: rising or falling as it began, until ground stops a fall
    /// of theirs. One begun on the ground, or come down onto it, presses them to the ground on every update after,
    /// ground or none: it does not notice the ground gone from under them, and they go down by the press alone,
    /// adrift.
    /// </summary>
    private bool _inAir;

    /// <inheritdoc/>
    public override void Enter(Player owner)
    {
        _inAir = _fallsInAir && !owner.IsAdrift && (owner.IsRising || !owner.Velocity.Y.IsZero || !owner.IsOnGround);

        // A smaller body is the player's at once, as they shrink into it; a taller one is theirs from their next
        // update, so that what meets them meanwhile meets the body they had.
        // A change of posture comes once the update's movement is made and is no move of the player's: once the form
        // is theirs the view takes them to have stood where it leaves them, whether or not the change moved their
        // middle, and follows nothing of this update's way down.
        if (_form != Player.Crawling)
        {
            if (owner.TrySetForm(_form, atOnce: owner.HalfHeightOf(_form) < owner.HalfHeight))
                owner.Room.Session.Camera.TakePlayerToHaveStoodAtY(owner.Position.Y.Whole);
            return;
        }

        // The low form is taken as from a crouch, wherever the player is: the body shrinks about its middle, and the
        // middle then drops by as much as a crouch stands taller than the low form, or as far as there is room. The
        // drop is the low body's own: tried with the body they had it would be stopped by the floor they stand on.
        // Only the room itself stops it: something solid under them that is no part of the room does not.
        int drop = owner.HalfHeightOf(Player.Crouching) - owner.HalfHeightOf(Player.Crawling);
        owner.TrySetForm(Player.Crawling, aroundMiddle: true, atOnce: true);
        owner.MoveBodyDown(Pixels.FromWhole(drop), owner.BodyMoves with { StoppedBySolids = false });

        // The view takes them to have stood where the drop leaves them.
        owner.Room.Session.Camera.TakePlayerToHaveStoodAtY(owner.Position.Y.Whole);
    }

    /// <inheritdoc/>
    public override PlayerState? Move(Player owner)
    {
        owner.MoveAcross(byDashAlone: true);
        if (_inAir)
        {
            // A rise stopped by a ceiling keeps its speed for gravity to wear away; a fall stopped by ground is over,
            // and the player is on the ground from then on.
            owner.TurnAtTop();
            bool falling = !owner.Velocity.Y.IsNegative;
            MoveResult down = owner.MoveDownUnderGravity(keepsSpeedWhenStopped: true);
            if (falling && down.Blocked)
            {
                owner.FallFromRest();
                _inAir = false;
            }
        }
        else if (!owner.PressToGround())
        {
            // Pressed down with the ground gone they are in the air neither rising nor falling: adrift, and the state
            // the change leads to takes them as it takes anyone adrift.
            owner.Drift();
        }

        _left = _left.Stepped(owner);
        return null;
    }

    /// <summary>A change of posture begun by a player being thrown goes on while the throw moves them.</summary>
    public override void WhileThrown(Player owner) => _left = _left.Stepped(owner);

    /// <inheritdoc/>
    public override PlayerState? Ended(Player owner) => _left.IsRunning ? null : Then;
}
