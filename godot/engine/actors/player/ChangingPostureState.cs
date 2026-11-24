using Engine.Collision;
using Engine.Shared;

namespace Engine.Actors;

/// <summary>
/// Passing from one posture to another: crouching down, standing up, taking the low form or leaving it, where the
/// game gives the change a length. The body takes its new form at once. For as long as the change lasts the input is
/// not asked, the player moves across only as far as a dash carries them, and they cannot attack; a change into or
/// out of the low form made in the air goes on falling. Then the state it leads to follows.
/// </summary>
public sealed class ChangingPostureState : PlayerState
{
    private readonly string _form;
    private readonly bool _fallsInAir;
    private Countdown _left;

    /// <summary>A change of posture.</summary>
    /// <param name="form">The form the body takes as the change begins.</param>
    /// <param name="then">The state the change leads to.</param>
    /// <param name="updates">How many updates the change lasts.</param>
    /// <param name="fallsInAir">Whether a change made in the air goes on rising and falling; otherwise the player hangs for its length.</param>
    public ChangingPostureState(string form, PlayerState then, int updates, bool fallsInAir)
    {
        ArgumentNullException.ThrowIfNull(then);
        (_form, Then, _fallsInAir) = (form, then, fallsInAir);
        _left = Countdown.Of(updates);
    }

    /// <summary>The state the change leads to.</summary>
    public PlayerState Then { get; }

    /// <inheritdoc/>
    public override bool CanAttack => false;

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
        owner.Room.Mover.MoveDown(owner.Body, Pixels.FromWhole(drop), MoveOptions.Plain);
    }

    /// <inheritdoc/>
    public override PlayerState? Move(Player owner)
    {
        owner.MoveAcross(byDashAlone: true);
        if (_fallsInAir && (owner.IsRising || !owner.IsOnGround))
        {
            // A rise stopped by a ceiling keeps its speed for gravity to wear away; a fall stopped by ground is over.
            owner.TurnAtTop();
            bool falling = !owner.Velocity.Y.IsNegative;
            MoveResult down = owner.MoveDownUnderGravity(keepsSpeedWhenStopped: true);
            if (falling && down.Blocked)
                owner.FallFromRest();
        }

        _left = _left.Stepped();
        return null;
    }

    /// <inheritdoc/>
    public override PlayerState? Ended(Player owner) => _left.IsRunning ? null : Then;
}
