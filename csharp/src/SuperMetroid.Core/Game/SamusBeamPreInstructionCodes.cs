namespace SuperMetroid.Core.Game;

/// <summary>Native bank-$90 beam callback tables and their translated entry points.</summary>
public static class SamusBeamPreInstructionCodes
{
    /// <summary>$90:B96E, FireUnchargedBeam's low-nibble-indexed callback words.</summary>
    public const int UnchargedTable = 0x90b96e;
    /// <summary>$90:BA3E, FireChargedBeam's low-nibble-indexed callback words.</summary>
    public const int ChargedTable = 0x90ba3e;
    /// <summary>$90:AEF3, ProjPreInstr_Beam_NoWaveBeam: ordinary terrain-stopping beam motion.</summary>
    public const ushort NoWave = 0xaef3;
    /// <summary>$90:B0E4, ProjPreInstr_BeamOrIceWave: Wave movement with a three-frame trail reload.</summary>
    public const ushort WaveThreeFrameTrail = 0xb0e4;
    /// <summary>$90:B0C3, ProjPreInstr_WavePlasmaEtc: Wave movement with a four-frame trail reload.</summary>
    public const ushort WaveFourFrameTrail = 0xb0c3;
    /// <summary>
    /// $90:AD16, the invalid beam-combination fourteen entry read two words beyond
    /// FireUnchargedBeam's callback table. JSR enters the operand of LoadBeamPalette's
    /// sprite-palette store, then falls into its increment/loop tail with projectile-owned
    /// X and Y. Later animation frames can therefore copy a large ROM range through WRAM.
    /// </summary>
    public const ushort SpacetimePaletteCopyTail = 0xad16;
    /// <summary>
    /// $90:B0AC, the invalid beam-combination thirteen entry produced by indexing two
    /// words beyond the uncharged callback table. Execution begins on the bank byte of
    /// the preceding JSL, so it stores Y to the cached PPU register pair at $60+X and
    /// falls through to the Power Bomb pre-instruction at $90:B0AE.
    /// </summary>
    public const ushort ChainsawWindowStoreThenPowerBomb = 0xb0ac;
    /// <summary>
    /// Charged combination thirteen overruns <see cref="ChargedTable"/> at $90:BA58 and
    /// reads callback <c>$0A0A</c>. JSR therefore enters the bank-$90 low-WRAM mirror at
    /// $7E:0A0A, whose word is Samus's cached previous Super-Missile count. Native
    /// execution is data-dependent and may crash; it is not a stable bank-$90 routine.
    /// </summary>
    public const ushort ChargedChainsawLowWramExecution = 0x0a0a;

    /// <summary>
    /// $90:A4AA, the callback obtained when charged all-beams combination fifteen reads
    /// beyond <see cref="ChargedTable"/>. The safe left-facing Murder Beam has a zero
    /// instruction pointer, so bank $90 never dispatches this callback; unsafe directions
    /// retain nonzero lists and enter unrelated native code.
    /// </summary>
    public const ushort MurderBeamMisalignedExecution = 0xa4aa;
}
