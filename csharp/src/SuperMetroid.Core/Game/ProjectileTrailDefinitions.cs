namespace SuperMetroid.Core.Game;

/// <summary>Compiled $90:B5BB/B609 trail-list selection, including every reachable adjacent-code observation.</summary>
public static class ProjectileTrailDefinitions
{
    /// <summary>$90:B4C9, InstList_BeamTrail_Empty: immediate trail termination.</summary>
    public const ushort Empty = 0xb4c9;
    /// <summary>$90:B4CB, InstList_LeftBeamTrail_IceBeams_ChargedPowerBeam.</summary>
    public const ushort LeftIce = 0xb4cb;
    /// <summary>$90:B52D, InstList_RightBeamTrail_SomeIceBeams.</summary>
    public const ushort RightIce = 0xb52d;
    /// <summary>$90:B58F, InstList_BeamTrail_WaveBeam, shared by both sides.</summary>
    public const ushort Wave = 0xb58f;
    /// <summary>$90:B5A1, InstList_BeamTrail_SuperMissile, also used by ordinary missiles.</summary>
    public const ushort Missile = 0xb5a1;

    /// <summary>
    /// $90:B657-$B688: the25 words observed when the right trail selector's low-six-bit
    /// index reaches past its39 entries into Spawn projectile trail's machine code.
    /// </summary>
    /// <remarks>
    /// Narrow nonsense exception under #1165: these are native instruction encodings,
    /// addresses and relative branches, from PHB through the first byte of the next LDA.
    /// Managed trail behavior has no corresponding machine-code layout from which to
    /// calculate them; adding an assembler would merely reconstruct this same byte data.
    /// The supported NTSC J/U1.0 ROM and native$90:B609 base/low-six-bit consumer confirm
    /// every bounded observation. The preceding78 genuine selector entries calculate
    /// separately from beam/effect semantics and are not covered by this exception.
    /// </remarks>
    private static ReadOnlySpan<ushort> AdjacentCodeWords =>
    [
        0xbd8b, 0x0c18, 0x0089, 0xd00f, 0x2905, 0x003f, 0x0d80, 0x29eb,
        0x000f, 0x03c9, 0xb000, 0x1817, 0x1f69, 0x0a00, 0xf4aa, 0x7e7e,
        0xabab, 0x22a0, 0xb900, 0xd658, 0x07f0, 0x8888, 0xf710, 0x38ab,
        0xa96b,
    ];

    /// <summary>$90:B5FB/B5FD: native missile and super-missile trail selection indices.</summary>
    private const int MissileSelection = 0x20;
    /// <summary>$90:B5FD: super-missile selector after native family-to-index conversion.</summary>
    private const int SuperMissileSelection = 0x21;
    /// <summary>$90:B603-B607/$B651-B655: three Spazer SBA trail selections.</summary>
    private const int SpazerSbaFirstSelection = 0x24;
    /// <summary>$90:B605/$B653: middle Spazer SBA selector uses distinct left/right ice trails.</summary>
    private const int SpazerSbaMiddleSelection = 0x25;
    /// <summary>$90:B607/$B655: final Spazer SBA selector shares the left ice trail on both sides.</summary>
    private const int SpazerSbaLastSelection = 0x26;

    /// <summary>Returns the complete bounded native selector window, including left-table overlap and adjacent observations.</summary>
    public static ushort ReadSelector(int address)
    {
        int offset = address - SamusProjectileRomData.Trails.LeftInstructionPointers;
        if ((uint)offset >= 103 * sizeof(ushort) || (offset & 1) != 0)
            throw new InvalidDataException($"Projectile trail selector ${address:X6} is outside the bounded low-six-bit observation window.");
        int index = offset / sizeof(ushort);
        if (index >= 78) return AdjacentCodeWords[index - 78];
        return Select(index % 39, right: index >= 39);
    }

    private static ushort Select(int selection, bool right)
    {
        if (selection >= MissileSelection)
            return selection switch
            {
                MissileSelection or SuperMissileSelection => right ? Empty : Missile,
                SpazerSbaFirstSelection or SpazerSbaLastSelection => LeftIce,
                SpazerSbaMiddleSelection => right ? RightIce : LeftIce,
                _ => Empty,
            };
        var type = new SamusProjectileTypeWord((ushort)selection);
        var beams = (SamusBeamFlags)type.BeamCombinationIndex;
        if (beams == SamusBeamFlags.Wave)
            return !right || type.IsChargedBeam ? Wave : Empty;
        if (!right)
        {
            if (beams == 0 && type.IsChargedBeam) return LeftIce;
            bool ice = (beams & SamusBeamFlags.Ice) != 0;
            bool incompatible = (beams & (SamusBeamFlags.Spazer | SamusBeamFlags.Plasma)) ==
                (SamusBeamFlags.Spazer | SamusBeamFlags.Plasma);
            return ice && !incompatible ? LeftIce : Empty;
        }
        bool spazerIce = (beams & (SamusBeamFlags.Spazer | SamusBeamFlags.Ice)) ==
            (SamusBeamFlags.Spazer | SamusBeamFlags.Ice) && (beams & SamusBeamFlags.Plasma) == 0;
        bool waveIcePlasma = beams == (SamusBeamFlags.Wave | SamusBeamFlags.Ice | SamusBeamFlags.Plasma);
        bool chargedWaveIce = type.IsChargedBeam && beams == (SamusBeamFlags.Wave | SamusBeamFlags.Ice);
        return spazerIce || waveIcePlasma || chargedWaveIce ? RightIce : Empty;
    }
}