namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>A beam frame behaviour the fire handlers select by charge state and beams, including the entries they read past their authored lists.</summary>
internal enum SamusBeamFrameBehaviorChoice
{
    /// <summary>A beam without Wave: ordinary terrain-stopping beam motion.</summary>
    NoWave,
    /// <summary>Wave or Ice-Wave: Wave movement with a three-frame trail reload.</summary>
    WaveThreeFrameTrail,
    /// <summary>Wave-Plasma and the other Wave combinations: Wave movement with a four-frame trail reload.</summary>
    WaveFourFrameTrail,
    /// <summary>
    /// Uncharged combination twelve reads past the authored frame behaviours into a value that
    /// names a region that holds nothing, so the game runs whatever it reads there. That is outside the emulation scope limit
    /// (#1500): the shot is rejected loudly.
    /// </summary>
    UnchargedCombinationTwelveOverrun,
    /// <summary>
    /// The invalid beam-combination fourteen entry read two entries beyond
    /// the uncharged firing's authored frame behaviours: the Spacetime shot's progression overwrite. From an animation that
    /// reaches far enough, the shot's frame behaviour turns saved progression on and off as
    /// <see cref="SuperMetroid.Core.Compatibility.SpaceTimeProgressionOverwrite{TCursor, TOverwrite}"/> decodes it.
    /// </summary>
    SpaceTimeProgressionOverwrite,
    /// <summary>
    /// The invalid beam-combination thirteen entry produced by indexing two
    /// entries beyond the uncharged authored frame behaviours. It starts partway into
    /// the step before it, so it stores the shot's vertical value to the cached layer-scroll pair its horizontal value
    /// selects and falls through to the Power Bomb frame behaviour.
    /// </summary>
    ChainsawPowerBomb,
    /// <summary>
    /// Charged combination thirteen overruns the authored frame behaviours and
    /// reads its frame behaviour from Samus's state. It therefore enters
    /// whatever Samus's cached previous Super-Missile count encodes. The game's
    /// execution is data-dependent and may crash; it is not a stable behaviour.
    /// </summary>
    ChargedChainsawOverrun,
    /// <summary>
    /// Uncharged combination fifteen reads past the authored frame behaviours into a value that names a
    /// place in the game's live state, so the game runs whatever Samus's state holds. That is outside the emulation scope limit
    /// (#1500): the shot is rejected loudly.
    /// </summary>
    UnchargedCombinationFifteenOverrun,
    /// <summary>
    /// Charged combination twelve reads past the charged
    /// frame behaviours; it names a place in the game's live state, so the game runs whatever that state holds. That is outside the
    /// emulation scope limit (#1500): the shot is rejected loudly.
    /// </summary>
    ChargedCombinationTwelveOverrun,
    /// <summary>
    /// The frame behaviour obtained when charged all-beams combination fifteen reads
    /// beyond the charged authored frame behaviours. The safe left-facing Murder Beam has no
    /// animation, so the game never runs this frame behaviour; unsafe directions
    /// keep their animations and run an unrelated part of the game.
    /// </summary>
    MurderBeamMisalignedExecution,
}
