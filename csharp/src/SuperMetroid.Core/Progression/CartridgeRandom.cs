namespace SuperMetroid.Core.Progression;

/// <summary>
/// The cartridge's random-number word at WRAM <c>$05E5</c>: one per game session, shared by the frontend, every
/// gameplay runtime and every random consumer. Lifetime: the session; only the reset vector reseeds it.
/// </summary>
public sealed class CartridgeRandom
{
    /// <summary>Creates the word with the game's power-on seed.</summary>
    public CartridgeRandom() => State = CartridgeRandomRomData.PowerOnState;

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
    /// Advances the word exactly as <c>$80:8111</c> (<c>GenerateRandomNumber</c>) does. Every consumer observes the
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
    /// Overwrites the word. The reset vector seeds it, and Sbug activation functions $A3:A315 and $A3:A325 both execute
    /// <c>STA $05E5</c> with <c>$000B</c>, so later unrelated consumers observe the replacement seed as well.
    /// </summary>
    /// <param name="value">The new word, 0 through 65535.</param>
    public void Seed(int value) => State = new SharedRandomState(value);

    /// <summary>Stores the power-on seed, as the reset vector does.</summary>
    internal void SeedPowerOn() => State = CartridgeRandomRomData.PowerOnState;

    /// <summary>Stores the state with its two bytes exchanged, as the lava and acid FX routine does.</summary>
    internal void SwapBytes() => State = State.WithBytesSwapped();
}
