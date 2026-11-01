namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>A beam pre-instruction callback the fire handlers select by charge state and beams, including the words they read past their tables.</summary>
internal enum SamusBeamCallback
{
    /// <summary>ProjPreInstr_Beam_NoWaveBeam: ordinary terrain-stopping beam motion.</summary>
    NoWave,
    /// <summary>ProjPreInstr_BeamOrIceWave: Wave movement with a three-frame trail reload.</summary>
    WaveThreeFrameTrail,
    /// <summary>ProjPreInstr_WavePlasmaEtc: Wave movement with a four-frame trail reload.</summary>
    WaveFourFrameTrail,
    /// <summary>
    /// The non-routine word observed when uncharged combination twelve
    /// indexes beyond the retail callback table. It remains untranslated and fails loudly.
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
    /// The non-routine word observed when uncharged combination fifteen
    /// indexes beyond the retail callback table. It remains untranslated and fails loudly.
    /// </summary>
    UnchargedCombinationFifteenAdjacentWord,
    /// <summary>
    /// The non-routine word observed when charged combination twelve
    /// indexes beyond the retail callback table. It remains untranslated and fails loudly.
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
