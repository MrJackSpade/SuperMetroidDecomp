using Engine.Actors.Components;

namespace Engine.Actors;

/// <summary>One of the things the player can be doing. The engine provides the standard ones; a game adds its own.</summary>
public abstract class PlayerState : State<Player>
{
    /// <summary>Whether the player can use weapons in this state.</summary>
    public virtual bool CanAttack => true;

    /// <summary>Whether the player is in control of themselves in this state.</summary>
    public virtual bool InControl => true;
}
