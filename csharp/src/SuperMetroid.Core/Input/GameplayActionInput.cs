namespace SuperMetroid.Core.Input;

/// <summary>
/// The player's gameplay actions, latched once per accepted update after the controller is read: the actions held and
/// the actions newly pressed. Gameplay reads its input here.
/// </summary>
public sealed partial class GameplayActionInput
{
    /// <summary>
    /// The actions the previous latch held, against which the next latch finds the newly pressed ones; a demo's substituted
    /// actions never change it (<see cref="UseDemoInput"/>).
    /// </summary>
    private GameplayAction _previous;

    /// <summary>Actions currently held.</summary>
    public GameplayAction Current { get; private set; }

    /// <summary>
    /// Rising-edge actions. An action is set for exactly the update on which it changes from released to held.
    /// </summary>
    public GameplayAction NewlyPressed { get; private set; }

    /// <summary>
    /// Temporarily publishes a demo's explicit held/edge pair in the player's place. Disposing restores the real
    /// latched actions for the demo-cancellation check. It does not <see cref="Latch"/>: recorded edges need not equal
    /// held-input changes.
    /// </summary>
    internal IDisposable UseDemoInput(GameplayAction held, GameplayAction newlyPressed)
    {
        var restore = new DemoInputRestore(this, Current, NewlyPressed);
        Current = held;
        NewlyPressed = newlyPressed;
        return restore;
    }

    /// <summary>Restores the real latched actions after a scoped demo input override.</summary>
    private sealed partial class DemoInputRestore(GameplayActionInput owner, GameplayAction held, GameplayAction newlyPressed) : IDisposable
    {
        /// <summary>Prevents a repeated dispose from replacing a later latch.</summary>
        private bool disposed;

        /// <summary>Restores the held and newly pressed actions captured before the demo override.</summary>
        public void Dispose()
        {
            if (disposed) return;
            owner.Current = held;
            owner.NewlyPressed = newlyPressed;
            disposed = true;
        }
    }

    /// <summary>Latches the actions of one accepted update.</summary>
    public void Latch(GameplayAction actions)
    {
        Current = actions;

        // The actions that changed since the previous latch, kept only where they are now held, are the newly pressed
        // ones. Released actions drop out because they are absent from Current.
        NewlyPressed = Current & (_previous ^ Current);
        _previous = Current;
    }
}
