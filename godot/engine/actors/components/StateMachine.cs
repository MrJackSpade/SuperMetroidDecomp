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

    /// <summary>The state the current one covers: kept as it was, neither left nor stepped, until it is uncovered.</summary>
    private State<TOwner>? _covered;

    /// <summary>Whether the current state covers another that is kept beneath it.</summary>
    public bool Covers => _covered is not null;

    /// <summary>
    /// Puts <paramref name="over"/> over the current state, which is kept as it is: it is not left, and is not
    /// stepped while it is covered. The owner has no say in it.
    /// </summary>
    /// <exception cref="InvalidOperationException">The current state covers another already.</exception>
    public void Cover(State<TOwner> over)
    {
        ArgumentNullException.ThrowIfNull(over);
        if (_covered is not null)
            throw new InvalidOperationException("A state that covers another cannot itself be covered.");
        _covered = Current;
        Current = over;
        over.Enter(_owner);
    }

    /// <summary>
    /// Takes the covering state away, which is left, and goes on in the state it covered, which is not entered
    /// afresh: it is as it was when it was covered.
    /// </summary>
    /// <exception cref="InvalidOperationException">The current state covers nothing.</exception>
    public void Uncover()
    {
        State<TOwner> kept = _covered ?? throw new InvalidOperationException("The current state covers nothing to go back to.");
        Current.Exit(_owner);
        Current = kept;
        _covered = null;
    }

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

        // A state kept beneath the one left is left with it: the owner moves on from both.
        if (_covered is { } kept)
        {
            _covered = null;
            kept.Exit(_owner);
        }

        Current = next;
        next.Enter(_owner);
    }
}
