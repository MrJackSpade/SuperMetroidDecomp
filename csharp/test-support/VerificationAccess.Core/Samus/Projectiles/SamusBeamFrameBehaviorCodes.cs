namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>Verification's native view of the beam callbacks: the bank-$90 word each fire handler table holds.</summary>
internal static class SamusBeamFrameBehaviorCodes
{
    /// <summary>$90:BA3E, FireChargedBeam's low-nibble-indexed callback words.</summary>
    public const int ChargedTable = 0x90ba3e;
    /// <summary>$90:B96E, FireUnchargedBeam's low-nibble-indexed callback words.</summary>
    public const int UnchargedTable = 0x90b96e;

    /// <summary>$90:AEF3, ProjPreInstr_Beam_NoWaveBeam: ordinary terrain-stopping beam motion.</summary>
    public const ushort NoWave = 0xaef3;
    /// <summary>$90:B0E4, ProjPreInstr_BeamOrIceWave: Wave movement with a three-frame trail reload.</summary>
    public const ushort WaveThreeFrameTrail = 0xb0e4;
    /// <summary>$90:B0C3, ProjPreInstr_WavePlasmaEtc: Wave movement with a four-frame trail reload.</summary>
    public const ushort WaveFourFrameTrail = 0xb0c3;
    /// <summary>
    /// <c>$3920</c>, the non-routine word observed when uncharged combination twelve
    /// indexes beyond the retail callback table. It remains untranslated and fails loudly.
    /// </summary>
    public const ushort UnchargedSpazerPlasmaUnconnectedRegionWord = 0x3920;
    /// <summary>
    /// $90:AD16, the invalid beam-combination fourteen entry read two words beyond
    /// FireUnchargedBeam's callback table. JSR enters the operand of ShowBeamShade's
    /// sprite-palette store, then falls into its increment/loop tail with projectile-owned
    /// X and Y. Later animation frames can therefore copy a large ROM range through WRAM.
    /// </summary>
    public const ushort SpaceTimeProgressionOverwrite = 0xad16;
    /// <summary>
    /// $90:B0AC, the invalid beam-combination thirteen entry produced by indexing two
    /// words beyond the uncharged callback table. Execution begins on the bank byte of
    /// the preceding JSL, so it stores Y to the cached PPU register pair at $60+X and
    /// falls through to the Power Bomb pre-instruction at $90:B0AE.
    /// </summary>
    public const ushort ChainsawPowerBomb = 0xb0ac;
    /// <summary>
    /// Charged combination thirteen overruns the callback table at <c>$90:BA3E..BA55</c> and
    /// reads callback <c>$0A0A</c> at <c>$90:BA58</c>. JSR therefore enters the bank-$90 low-WRAM mirror at
    /// $7E:0A0A, whose word is Samus's cached previous Super-Missile count. Native
    /// execution is data-dependent and may crash; it is not a stable bank-$90 routine.
    /// </summary>
    public const ushort ChargedChainsawSamusStateWord = 0x0a0a;
    /// <summary>
    /// <c>$0DC2</c>, the non-routine word observed when uncharged combination fifteen
    /// indexes beyond the retail callback table. It remains untranslated and fails loudly.
    /// </summary>
    public const ushort UnchargedAllBeamsLiveStateWord = 0x0dc2;
    /// <summary>
    /// <c>$1CAD</c>, the non-routine word observed when charged combination twelve
    /// indexes beyond the retail callback table. It remains untranslated and fails loudly.
    /// </summary>
    public const ushort ChargedSpazerPlasmaLiveStateWord = 0x1cad;
    /// <summary>
    /// $90:A4AA, the callback obtained when charged all-beams combination fifteen reads
    /// beyond the charged callback table at <c>$90:BA3E..BA55</c>. The safe left-facing Murder Beam has a zero
    /// instruction pointer, so bank $90 never dispatches this callback; unsafe directions
    /// retain nonzero lists and enter unrelated native code.
    /// </summary>
    public const ushort MurderBeamMisalignedExecution = 0xa4aa;

    /// <summary>
    /// The bank-$90 word each beam shot behaviour a fire handler table selects stands for (#1923); the uncharged all-beams
    /// and charged Spazer-Plasma shots run the same live state from different words, so the two words are told apart
    /// by the behaviour each table position selects.
    /// </summary>
    private static readonly IReadOnlyList<(SamusShotBehavior Behavior, ushort Word)> Words =
    [
        (SamusShotBehaviors.Beam, NoWave),
        (SamusShotBehaviors.UnchargedWaveBeamTrail, WaveThreeFrameTrail),
        (SamusShotBehaviors.WaveBeamTrail, WaveFourFrameTrail),
        (SamusShotBehaviors.UnchargedSpazerPlasma, UnchargedSpazerPlasmaUnconnectedRegionWord),
        (SamusShotBehaviors.Spacetime, SpaceTimeProgressionOverwrite),
        (SamusShotBehaviors.ChainsawPowerBomb, ChainsawPowerBomb),
        (SamusShotBehaviors.ChargedChainsaw, ChargedChainsawSamusStateWord),
        (SamusShotBehaviors.UnchargedAllBeams, UnchargedAllBeamsLiveStateWord),
        (SamusShotBehaviors.ChargedSpazerPlasma, ChargedSpazerPlasmaLiveStateWord),
        (SamusShotBehaviors.MurderBeam, MurderBeamMisalignedExecution),
    ];

    /// <summary>The bank-$90 word that selects <paramref name="behavior"/>.</summary>
    internal static ushort Of(SamusShotBehavior behavior)
    {
        foreach ((SamusShotBehavior candidate, ushort word) in Words)
        {
            if (ReferenceEquals(candidate, behavior))
                return word;
        }
        throw new ArgumentOutOfRangeException(nameof(behavior), behavior.GetType().Name, "No fire handler table selects this behaviour.");
    }

    /// <summary>A beam shot behaviour by its bank-$90 word.</summary>
    extension(SamusShotBehavior behavior)
    {
        /// <summary>The bank-$90 word the fire handler table holds for the behaviour.</summary>
        internal ushort NativePointer => Of(behavior);
    }
}
