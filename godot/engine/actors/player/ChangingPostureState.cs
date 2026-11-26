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
    private Countdown _left;

    /// <summary>A change of posture.</summary>
    /// <param name="form">The form the body takes as the change begins.</param>
    /// <param name="then">The state the change leads to.</param>
    /// <param name="updates">How many updates the change lasts.</param>
    /// <param name="fallsInAir">Whether a change made in the air goes on rising and falling; otherwise the player hangs for its length.</param>
    /// <param name="attacks">Whether the player can attack for as long as the change lasts.</param>
    public ChangingPostureState(string form, PlayerState then, int updates, bool fallsInAir, bool attacks)
    {
        ArgumentNullException.ThrowIfNull(then);
        (_form, Then, _fallsInAir, _attacks) = (form, then, fallsInAir, attacks);
        _left = Countdown.Of(updates);
    }

    /// <summary>The state the change leads to.</summary>
    public PlayerState Then { get; }

    /// <inheritdoc/>
    public override bool CanAttack => _attacks;

    /// <inheritdoc/>
    public override bool HoldsOffInterruptions => true;

    /// <inheritdoc/>
    public override void Enter(Player owner)
    {
        if (_form != Player.Crawling)
        {
            owner.TrySetForm(_form);
            return;
        }

        // The low form is taken as from a crouch, wherever the player is: the body shrinks about its middle, and the
        // middle then drops by as much as a crouch stands taller than the low form, or as far as there is room.
        int drop = owner.HalfHeightOf(Player.Crouching) - owner.HalfHeightOf(Player.Crawling);
        owner.TrySetForm(Player.Crawling, aroundMiddle: true);
        owner.Room.Mover.MoveDown(owner.Body, Pixels.FromWhole(drop), owner.BodyMoves);
    }

    /// <inheritdoc/>
    public override PlayerState? Move(Player owner)
    {
        owner.MoveAcross(byDashAlone: true);
        if (_fallsInAir && !owner.IsAdrift && (owner.IsRising || !owner.IsOnGround))
        {
            // A rise stopped by a ceiling keeps its speed for gravity to wear away; a fall stopped by ground is over.
            owner.TurnAtTop();
            bool falling = !owner.Velocity.Y.IsNegative;
            MoveResult down = owner.MoveDownUnderGravity(keepsSpeedWhenStopped: true);
            if (falling && down.Blocked)
                owner.FallFromRest();
        }
        else
        {
            owner.PressToGround();
        }

        _left = _left.Stepped();
        return null;
    }

    /// <summary>A change of posture begun by a player being thrown goes on while the throw moves them.</summary>
    public override void WhileThrown(Player owner) => _left = _left.Stepped();

    /// <inheritdoc/>
    public override PlayerState? Ended(Player owner) => _left.IsRunning ? null : Then;
}
