namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// The game's shared random sequence: one per game session, shared by the frontend, every gameplay runtime and every
/// random consumer, so each draw advances the sequence every later consumer observes, including the draws effect work
/// makes. Its advance and its values, 0 through 65535, are the game's exactly (<see cref="SharedRandomState"/>), because which
/// value each consumer draws, and in which order, is gameplay-observable.
/// </summary>
/// <remarks>
/// Created once per game session by its composition root (<c>SuperMetroidGame</c>, or a verification host supplying its
/// own) and injected into <c>FrontendSession</c>, which hands it to every gameplay runtime; only power-on and a soft reset
/// reseed it. Advanced once per frontend update by the game-state loop, once per gameplay update after the room liquid's
/// wave and the Tourian statues' descent (<see cref="LiquidWaveAndStatueDescentOrder"/>), and whenever a consumer draws during the update. Evidence: the original's
/// random routine (<c>$80:8111</c>) and its main loop's draw; the SMV replays compare the random value on every update.
/// </remarks>
[CompatibilityService("the exact sequence of the shared random value's draws and advances, across the frontend, every runtime and effect work")]
public sealed class SharedRandom
{
    /// <summary>Creates the generator with the game's power-on seed.</summary>
    public SharedRandom() => State = SharedRandomDefinitions.PowerOnState;

    /// <summary>The current state.</summary>
    public SharedRandomState State { get; private set; }

    /// <summary>Samples the current state, 0 through 65535, without advancing it.</summary>
    public int Read() => State.Value;

    /// <summary>
    /// Advances the generator to the sequence's next value (<see cref="SharedRandomState.Next"/>). Every consumer observes
    /// the updated state.
    /// </summary>
    /// <returns>The new state as a sample, 0 through 65535.</returns>
    public int Generate()
    {
        Advance();
        return State.Value;
    }

    /// <summary>
    /// Advances the generator to the sequence's next value without sampling it, as a draw whose value decides nothing does:
    /// every later consumer still observes the advance.
    /// </summary>
    public void Advance() => State = State.Next();

    /// <summary>
    /// Overwrites the state with a species' seed (<see cref="SpeciesRandomReseed"/>), so later unrelated consumers draw on
    /// from the replacement as well.
    /// </summary>
    public void Reseed(SpeciesRandomReseed reseed) => State = Ensure.NotNull(reseed).Seed;

    /// <summary>Stores the power-on seed, as a reset does.</summary>
    internal void SeedPowerOn() => State = SharedRandomDefinitions.PowerOnState;

    /// <summary>Stores the state with its two halves exchanged, as the lava and acid effect does.</summary>
    internal void SwapHalves() => State = State.WithHalvesSwapped();
}
