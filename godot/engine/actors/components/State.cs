namespace Engine.Actors.Components;

/// <summary>
/// One of the things its owner can be doing. A state holds its own data, which exists only while the owner is in it,
/// and names the state that follows it.
/// </summary>
/// <typeparam name="TOwner">What the state belongs to.</typeparam>
public abstract class State<TOwner>
{
    /// <summary>Called once as the owner enters this state.</summary>
    public virtual void Enter(TOwner owner)
    {
    }

    /// <summary>Runs one update and returns the state the owner is in next: this one to carry on, or another.</summary>
    public abstract State<TOwner> Step(TOwner owner);

    /// <summary>Called once as the owner leaves this state.</summary>
    public virtual void Exit(TOwner owner)
    {
    }
}
