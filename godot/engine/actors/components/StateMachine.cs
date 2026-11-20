namespace Engine.Actors.Components;

/// <summary>Holds the one state its owner is in and moves it from state to state.</summary>
/// <typeparam name="TOwner">What the states belong to.</typeparam>
public sealed class StateMachine<TOwner>
{
    /// <summary>What the states belong to.</summary>
    private readonly TOwner _owner;

    /// <summary>A machine for <paramref name="owner"/>, starting in <paramref name="first"/>.</summary>
    public StateMachine(TOwner owner, State<TOwner> first)
    {
        ArgumentNullException.ThrowIfNull(first);
        _owner = owner;
        Current = first;
        first.Enter(owner);
    }

    /// <summary>The state the owner is in.</summary>
    public State<TOwner> Current { get; private set; }

    /// <summary>Runs one update of the current state and moves to the state it names.</summary>
    public void Step() => ChangeTo(Current.Step(_owner));

    /// <summary>Moves to <paramref name="next"/> now. Naming the current state changes nothing.</summary>
    public void ChangeTo(State<TOwner> next)
    {
        ArgumentNullException.ThrowIfNull(next);
        if (ReferenceEquals(next, Current))
            return;
        Current.Exit(_owner);
        Current = next;
        next.Enter(_owner);
    }
}
