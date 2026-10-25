namespace SuperMetroid.Core.Progression;

/// <summary>
/// The game's shared random-number generator: one per game session, shared by the frontend, every gameplay runtime and
/// every random consumer, so each draw advances the sequence every later consumer observes. Lifetime: the session; only
/// a reset reseeds it. Its arithmetic and sixteen-bit state are the cartridge's exactly (<see cref="SharedRandomState"/>),
/// because which value each consumer draws is gameplay-observable.
/// </summary>
public sealed class SharedRandom
{
    /// <summary>Creates the generator with the game's power-on seed.</summary>
    public SharedRandom() => State = SharedRandomDefinitions.PowerOnState;

    /// <summary>The current state.</summary>
    public SharedRandomState State { get; private set; }

    /// <summary>
    /// Diagnostic-only observer of each <see cref="Generate"/> call, used by movie-replay tracing to attribute RNG
    /// consumers. Never part of gameplay state.
    /// </summary>
    internal Action? CallObserver { get; set; }

    /// <summary>Samples the current state, 0 through 65535, without advancing it.</summary>
    public int Read() => State.Value;

    /// <summary>
    /// Advances the generator exactly as the cartridge's <c>GenerateRandomNumber</c> does. Every consumer observes the
    /// updated state.
    /// </summary>
    /// <returns>The new state as a sample, 0 through 65535.</returns>
    public int Generate()
    {
        CallObserver?.Invoke();
        State = State.Next();
        return State.Value;
    }

    /// <summary>
    /// Overwrites the state. A reset seeds it, and the Sbug's activation reseeds it with eleven, so later unrelated
    /// consumers observe the replacement seed as well.
    /// </summary>
    /// <param name="value">The new state, 0 through 65535.</param>
    public void Seed(int value) => State = new SharedRandomState(value);

    /// <summary>Stores the power-on seed, as a reset does.</summary>
    internal void SeedPowerOn() => State = SharedRandomDefinitions.PowerOnState;

    /// <summary>Stores the state with its two bytes exchanged, as the lava and acid FX routine does.</summary>
    internal void SwapBytes() => State = State.WithBytesSwapped();
}
