namespace SuperMetroid.Core.Game;

/// <summary>
/// The beam-combination table index: bits 0-3 of a beam projectile's type word
/// (<c>$0C18</c>), copied from the equipped-beam word (<c>$09A6</c>). Bank-$90/$93/$9B
/// tables index projectile damage, speed, trails, sounds and enemy vulnerability by it.
/// Retail equipment never pairs Spazer with Plasma; the four values that do are reachable
/// only through glitched equipment, and their native table reads run past the twelve retail
/// rows (Chainsaw $D and SpaceTime $E read adjacent data as artwork).
/// </summary>
public enum SamusBeamCombination : byte
{
    /// <summary>$0: the unmodified Power Beam.</summary>
    Power = 0x0,
    /// <summary>$1: Wave.</summary>
    Wave = 0x1,
    /// <summary>$2: Ice.</summary>
    Ice = 0x2,
    /// <summary>$3: Ice and Wave.</summary>
    IceWave = 0x3,
    /// <summary>$4: Spazer.</summary>
    Spazer = 0x4,
    /// <summary>$5: Spazer and Wave.</summary>
    SpazerWave = 0x5,
    /// <summary>$6: Spazer and Ice.</summary>
    SpazerIce = 0x6,
    /// <summary>$7: Spazer, Ice and Wave.</summary>
    SpazerIceWave = 0x7,
    /// <summary>$8: Plasma.</summary>
    Plasma = 0x8,
    /// <summary>$9: Plasma and Wave.</summary>
    PlasmaWave = 0x9,
    /// <summary>$A: Plasma and Ice.</summary>
    PlasmaIce = 0xa,
    /// <summary>$B: Plasma, Ice and Wave.</summary>
    PlasmaIceWave = 0xb,
    /// <summary>$C: glitched Spazer and Plasma.</summary>
    SpazerPlasma = 0xc,
    /// <summary>$D: glitched Spazer, Plasma and Wave (the Chainsaw beam).</summary>
    SpazerPlasmaWave = 0xd,
    /// <summary>$E: glitched Spazer, Plasma and Ice (the SpaceTime beam).</summary>
    SpazerPlasmaIce = 0xe,
    /// <summary>$F: glitched Spazer, Plasma, Ice and Wave.</summary>
    SpazerPlasmaIceWave = 0xf,
}

/// <summary>Decoding and component predicates for <see cref="SamusBeamCombination"/>.</summary>
public static class SamusBeamCombinations
{
    /// <summary>The combination at a four-bit table index.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The index is outside $0-$F.</exception>
    public static SamusBeamCombination FromTableIndex(int index) => (uint)index <= 0xf
        ? (SamusBeamCombination)index
        : throw new ArgumentOutOfRangeException(nameof(index), index, "Beam-combination table index is outside $0-$F.");

    extension(SamusBeamCombination combination)
    {
        /// <summary>The four-bit index the native tables use.</summary>
        public int TableIndex => (int)combination;

        /// <summary>The beam components as equipment bits.</summary>
        public SamusBeamFlags Components => (SamusBeamFlags)(byte)combination;

        /// <summary>Whether Wave is a component.</summary>
        public bool HasWave => (combination.Components & SamusBeamFlags.Wave) != 0;

        /// <summary>Whether Ice is a component.</summary>
        public bool HasIce => (combination.Components & SamusBeamFlags.Ice) != 0;

        /// <summary>Whether Spazer is a component.</summary>
        public bool HasSpazer => (combination.Components & SamusBeamFlags.Spazer) != 0;

        /// <summary>Whether Plasma is a component.</summary>
        public bool HasPlasma => (combination.Components & SamusBeamFlags.Plasma) != 0;

        /// <summary>Whether retail equipment can produce this combination: every row the twelve-entry native tables define.</summary>
        public bool IsRetail => combination switch
        {
            SamusBeamCombination.Power or SamusBeamCombination.Wave or SamusBeamCombination.Ice or
                SamusBeamCombination.IceWave or SamusBeamCombination.Spazer or SamusBeamCombination.SpazerWave or
                SamusBeamCombination.SpazerIce or SamusBeamCombination.SpazerIceWave or SamusBeamCombination.Plasma or
                SamusBeamCombination.PlasmaWave or SamusBeamCombination.PlasmaIce or
                SamusBeamCombination.PlasmaIceWave => true,
            SamusBeamCombination.SpazerPlasma or SamusBeamCombination.SpazerPlasmaWave or
                SamusBeamCombination.SpazerPlasmaIce or SamusBeamCombination.SpazerPlasmaIceWave => false,
            _ => throw new ArgumentOutOfRangeException(nameof(combination), combination, "Undefined beam combination."),
        };
    }
}
