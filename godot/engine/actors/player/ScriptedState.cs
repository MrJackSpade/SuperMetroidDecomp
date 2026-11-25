using Engine.Shared;

namespace Engine.Actors;

/// <summary>
/// Under a script's control: the player moves at the speed the script sets and does not act. The script that took
/// control sets <see cref="Velocity"/> as it goes and releases the player when it is done.
/// </summary>
public sealed class ScriptedState : PlayerState
{
    /// <summary>The speed the script moves the player at.</summary>
    public PixelVelocity Velocity { get; set; }

    /// <summary>Whether gravity acts on the player while scripted.</summary>
    public bool Falls { get; set; }

    /// <inheritdoc/>
    public override bool CanAttack => false;

    /// <inheritdoc/>
    public override bool InControl => false;

    /// <inheritdoc/>
    public override bool InAnothersHands => true;

    /// <inheritdoc/>
    public override PlayerState? Move(Player owner)
    {
        owner.Velocity = Falls ? Velocity.WithY(owner.Velocity.Y) : Velocity;
        if (Falls)
            owner.Fall();
        owner.MoveAcross();
        owner.MoveDown();
        return null;
    }
}
