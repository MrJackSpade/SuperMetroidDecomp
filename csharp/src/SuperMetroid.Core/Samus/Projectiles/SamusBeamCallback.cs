namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>A beam pre-instruction callback the fire handlers select by charge state and beams, including the words they read past their tables.</summary>
internal enum SamusBeamCallback
{
    /// <summary>$90:AEF3, ProjPreInstr_Beam_NoWaveBeam: ordinary terrain-stopping beam motion.</summary>
    NoWave,
    /// <summary>$90:B0E4, ProjPreInstr_BeamOrIceWave: Wave movement with a three-frame trail reload.</summary>
    WaveThreeFrameTrail,
    /// <summary>$90:B0C3, ProjPreInstr_WavePlasmaEtc: Wave movement with a four-frame trail reload.</summary>
    WaveFourFrameTrail,
    /// <summary>
    /// <c>$3920</c>, the non-routine word observed when uncharged combination twelve
    /// indexes beyond the retail callback table. It remains untranslated and fails loudly.
    /// </summary>
    UnchargedCombinationTwelveAdjacentWord,
    /// <summary>
    /// $90:AD16, the invalid beam-combination fourteen entry read two words beyond
    /// FireUnchargedBeam's callback table. JSR enters the operand of LoadBeamPalette's
    /// sprite-palette store, then falls into its increment/loop tail with projectile-owned
    /// X and Y. Later animation frames can therefore copy a large ROM range through WRAM.
    /// </summary>
    SpacetimePaletteCopyTail,
    /// <summary>
    /// $90:B0AC, the invalid beam-combination thirteen entry produced by indexing two
    /// words beyond the uncharged callback table. Execution begins on the bank byte of
    /// the preceding JSL, so it stores Y to the cached PPU register pair at $60+X and
    /// falls through to the Power Bomb pre-instruction at $90:B0AE.
    /// </summary>
    ChainsawPowerBomb,
    /// <summary>
    /// Charged combination thirteen overruns the callback table at <c>$90:BA3E..BA55</c> and
    /// reads callback <c>$0A0A</c> at <c>$90:BA58</c>. JSR therefore enters the bank-$90 low-WRAM mirror at
    /// $7E:0A0A, whose word is Samus's cached previous Super-Missile count. Native
    /// execution is data-dependent and may crash; it is not a stable bank-$90 routine.
    /// </summary>
    ChargedChainsawOverrun,
    /// <summary>
    /// <c>$0DC2</c>, the non-routine word observed when uncharged combination fifteen
    /// indexes beyond the retail callback table. It remains untranslated and fails loudly.
    /// </summary>
    UnchargedCombinationFifteenAdjacentWord,
    /// <summary>
    /// <c>$1CAD</c>, the non-routine word observed when charged combination twelve
    /// indexes beyond the retail callback table. It remains untranslated and fails loudly.
    /// </summary>
    ChargedCombinationTwelveAdjacentWord,
    /// <summary>
    /// $90:A4AA, the callback obtained when charged all-beams combination fifteen reads
    /// beyond the charged callback table at <c>$90:BA3E..BA55</c>. The safe left-facing Murder Beam has a zero
    /// instruction pointer, so bank $90 never dispatches this callback; unsafe directions
    /// retain nonzero lists and enter unrelated native code.
    /// </summary>
    MurderBeamMisalignedExecution,
}
