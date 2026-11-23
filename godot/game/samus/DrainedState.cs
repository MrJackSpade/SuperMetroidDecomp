using Engine.Actors;
using Engine.Actors.Components;
using Engine.Shared;

namespace Game;

/// <summary>
/// Drained of her strength, as Mother Brain's beam leaves her: fallen to her knees, unable to move or fire, until
/// whatever drained her releases her.
/// </summary>
public sealed class DrainedState : PlayerState, ISamusLook
{
    /// <summary>Whether she has got back to her feet, as she does once before the last blow.</summary>
    public bool Standing { get; set; }

    /// <inheritdoc/>
    public string Look => Standing ? "drained_standing" : "drained";

    /// <inheritdoc/>
    public override bool CanAttack => false;

    /// <inheritdoc/>
    public override bool InControl => false;

    /// <inheritdoc/>
    public override void Enter(Player owner)
    {
        owner.StopAcross();
        owner.Weapons.DropCharge();
        owner.TrySetForm(Player.Standing);
    }

    /// <inheritdoc/>
    public override State<Player> Step(Player owner)
    {
        owner.Velocity = owner.Velocity.WithX(Pixels.Zero);
        owner.MoveDown();
        owner.Fall();
        return this;
    }
}
