namespace Engine.Actors.Components;

/// <summary>Holds the one state its owner is in and moves it from state to state.</summary>
/// <typeparam name="TOwner">What the states belong to.</typeparam>
public sealed class StateMachine<TOwner>
{
    /// <summary>What the states belong to.</summary>
    private readonly TOwner _owner;

    /// <summary>What the owner makes of a state asked for before it is entered, if the owner has a say.</summary>
    private readonly Func<State<TOwner>, State<TOwner>>? _admits;

    /// <summary>A machine for <paramref name="owner"/>, starting in <paramref name="first"/>.</summary>
    /// <param name="owner">What the states belong to.</param>
    /// <param name="first">The state the owner starts in, which is entered as it is.</param>
    /// <param name="admits">
    /// Asked with each state the machine is about to move to, before the current one is left: it gives the state to
    /// enter, which is the one asked for, another in its place, or the current state when the owner stays as it is.
    /// Null for an owner that enters whatever is asked for.
    /// </param>
    public StateMachine(TOwner owner, State<TOwner> first, Func<State<TOwner>, State<TOwner>>? admits = null)
    {
        ArgumentNullException.ThrowIfNull(first);
        _owner = owner;
        _admits = admits;
        Current = first;
        first.Enter(owner);
    }

    /// <summary>The state the owner is in.</summary>
    public State<TOwner> Current { get; private set; }

    /// <summary>Runs one update of the current state and moves to the state it names.</summary>
    public void Step() => ChangeTo(Current.Step(_owner));

    /// <summary>
    /// Moves to <paramref name="next"/> now, or to what the owner makes of it. Naming the current state changes
    /// nothing.
    /// </summary>
    public void ChangeTo(State<TOwner> next)
    {
        ArgumentNullException.ThrowIfNull(next);
        if (ReferenceEquals(next, Current))
            return;
        if (_admits is not null)
        {
            next = _admits(next) ?? throw new InvalidOperationException("The owner of a state machine admitted no state.");
            if (ReferenceEquals(next, Current))
                return;
        }

        Current.Exit(_owner);
        Current = next;
        next.Enter(_owner);
    }
}
