namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>A beam frame-behaviour callback the fire handlers select by charge state and beams, including the entries they read past their tables.</summary>
internal enum SamusBeamCallback
{
    /// <summary>A beam without Wave: ordinary terrain-stopping beam motion.</summary>
    NoWave,
    /// <summary>Wave or Ice-Wave: Wave movement with a three-frame trail reload.</summary>
    WaveThreeFrameTrail,
    /// <summary>Wave-Plasma and the other Wave combinations: Wave movement with a four-frame trail reload.</summary>
    WaveFourFrameTrail,
    /// <summary>
    /// Uncharged combination twelve reads past the callback table into a value that
    /// names an unconnected region, so the game executes open bus. That is outside the emulation scope limit
    /// (#1500): the shot is rejected loudly.
    /// </summary>
    UnchargedCombinationTwelveOverrun,
    /// <summary>
    /// The invalid beam-combination fourteen entry read two entries beyond
    /// the uncharged firing's callback table. It enters the middle of the beam palette loader's
    /// sprite-palette store, then falls into its increment/loop tail with projectile-owned
    /// X and Y. Later animation frames can therefore copy a large range of game data through memory.
    /// </summary>
    SpacetimePaletteCopyTail,
    /// <summary>
    /// The invalid beam-combination thirteen entry produced by indexing two
    /// entries beyond the uncharged callback table. Execution begins partway into
    /// the preceding call, so it stores the shot's vertical value to the cached layer-scroll pair its horizontal value
    /// selects and falls through to the Power Bomb frame behaviour.
    /// </summary>
    ChainsawPowerBomb,
    /// <summary>
    /// Charged combination thirteen overruns the callback table and
    /// reads a callback from Samus's state. It therefore enters
    /// whatever Samus's cached previous Super-Missile count encodes. The game's
    /// execution is data-dependent and may crash; it is not a stable behaviour.
    /// </summary>
    ChargedChainsawOverrun,
    /// <summary>
    /// Uncharged combination fifteen reads past the callback table into a value that names a
    /// place in the game's live state, so the game executes Samus's state as code. That is outside the emulation scope limit
    /// (#1500): the shot is rejected loudly.
    /// </summary>
    UnchargedCombinationFifteenOverrun,
    /// <summary>
    /// Charged combination twelve reads past the charged
    /// callback table; it names a place in the game's live state, so the game executes that state as code. That is outside the
    /// emulation scope limit (#1500): the shot is rejected loudly.
    /// </summary>
    ChargedCombinationTwelveOverrun,
    /// <summary>
    /// The callback obtained when charged all-beams combination fifteen reads
    /// beyond the charged callback table. The safe left-facing Murder Beam has no
    /// animation, so the game never dispatches this callback; unsafe directions
    /// retain animations and enter unrelated code.
    /// </summary>
    MurderBeamMisalignedExecution,
}
