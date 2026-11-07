namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>A beam pre-instruction callback the fire handlers select by charge state and beams, including the words they read past their tables.</summary>
internal enum SamusBeamCallback
{
    /// <summary>A beam without Wave: ordinary terrain-stopping beam motion.</summary>
    NoWave,
    /// <summary>Wave or Ice-Wave: Wave movement with a three-frame trail reload.</summary>
    WaveThreeFrameTrail,
    /// <summary>Wave-Plasma and the other Wave combinations: Wave movement with a four-frame trail reload.</summary>
    WaveFourFrameTrail,
    /// <summary>
    /// Uncharged combination twelve reads the charged-shot routine's first instruction bytes past the callback table;
    /// they name an unconnected region, so the game executes open bus. That is outside the emulation scope limit
    /// (#1500): the shot is rejected loudly.
    /// </summary>
    UnchargedCombinationTwelveAdjacentWord,
    /// <summary>
    /// The invalid beam-combination fourteen entry read two words beyond
    /// FireUnchargedBeam's callback table. It enters the middle of the beam palette loader's
    /// sprite-palette store, then falls into its increment/loop tail with projectile-owned
    /// X and Y. Later animation frames can therefore copy a large range of game data through memory.
    /// </summary>
    SpacetimePaletteCopyTail,
    /// <summary>
    /// The invalid beam-combination thirteen entry produced by indexing two
    /// words beyond the uncharged callback table. Execution begins inside
    /// the preceding call instruction, so it stores Y to the cached layer-scroll pair indexed by X and
    /// falls through to the Power Bomb pre-instruction.
    /// </summary>
    ChainsawPowerBomb,
    /// <summary>
    /// Charged combination thirteen overruns the callback table and
    /// reads a callback from Samus's state. It therefore enters
    /// whatever Samus's cached previous Super-Missile count encodes. The game's
    /// execution is data-dependent and may crash; it is not a stable routine.
    /// </summary>
    ChargedChainsawOverrun,
    /// <summary>
    /// Uncharged combination fifteen reads an operand of the charged-shot routine past the callback table; it names a
    /// word of work memory, so the game executes Samus's state as code. That is outside the emulation scope limit
    /// (#1500): the shot is rejected loudly.
    /// </summary>
    UnchargedCombinationFifteenAdjacentWord,
    /// <summary>
    /// Charged combination twelve reads the first instruction of the projectile placement routine past the charged
    /// callback table; it names a word of work memory, so the game executes that memory as code. That is outside the
    /// emulation scope limit (#1500): the shot is rejected loudly.
    /// </summary>
    ChargedCombinationTwelveAdjacentWord,
    /// <summary>
    /// The callback obtained when charged all-beams combination fifteen reads
    /// beyond the charged callback table. The safe left-facing Murder Beam has a zero
    /// instruction pointer, so the game never dispatches this callback; unsafe directions
    /// retain nonzero lists and enter unrelated code.
    /// </summary>
    MurderBeamMisalignedExecution,
}
