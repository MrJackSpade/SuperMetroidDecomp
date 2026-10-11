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

    /// <summary>The five trail instruction lists, in address order.</summary>
    private enum TrailList { Empty, LeftIce, RightIce, Wave, Missile }

    private static readonly TrailList[] TrailLists = Enum.GetValues<TrailList>();

    private static ushort StartOf(TrailList list) => list switch
    {
        TrailList.Empty => Empty,
        TrailList.LeftIce => LeftIce,
        TrailList.RightIce => RightIce,
        TrailList.Wave => Wave,
        TrailList.Missile => Missile,
        _ => throw new InvalidOperationException($"Undefined {nameof(TrailList)} {(int)list}."),
    };

    /// <summary>True when a trail cursor still sits on a list's first record, before any record is consumed.</summary>
    public static bool IsListStart(ushort cursor)
    {
        foreach (TrailList list in TrailLists)
            if (StartOf(list) == cursor) return true;
        return false;
    }

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

    /// <summary>The non-beam trail selections with a trail list; every other one draws none.</summary>
    private enum SpecialSelection
    {
        /// <summary>$90:B5FB/B5FD: native missile trail selection index.</summary>
        Missile = 0x20,
        /// <summary>$90:B5FD: super-missile selector after native family-to-index conversion.</summary>
        SuperMissile = 0x21,
        /// <summary>$90:B603/$B651: first of the three Spazer SBA trail selections.</summary>
        SpazerSbaFirst = 0x24,
        /// <summary>$90:B605/$B653: middle Spazer SBA selector uses distinct left/right ice trails.</summary>
        SpazerSbaMiddle = 0x25,
        /// <summary>$90:B607/$B655: final Spazer SBA selector shares the left ice trail on both sides.</summary>
        SpazerSbaLast = 0x26,
    }

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
        if (selection >= (int)SpecialSelection.Missile)
        {
            var special = (SpecialSelection)selection;
            if (!Enum.IsDefined(special))
                return Empty;
            return special switch
            {
                SpecialSelection.Missile or SpecialSelection.SuperMissile => right ? Empty : Missile,
                SpecialSelection.SpazerSbaFirst or SpecialSelection.SpazerSbaLast => LeftIce,
                SpecialSelection.SpazerSbaMiddle => right ? RightIce : LeftIce,
                _ => throw new InvalidOperationException($"Undefined SpecialSelection {special}."),
            };
        }
        var type = new SamusProjectileTypeWord((ushort)selection);
        bool charged = type.IsChargedBeam;
        // Ice draws its sparkle unless Spazer and Plasma are both present.
        return right
            ? type.BeamCombination switch
            {
                SamusBeamCombination.Wave => charged ? Wave : Empty,
                SamusBeamCombination.IceWave => charged ? RightIce : Empty,
                SamusBeamCombination.SpazerIce or SamusBeamCombination.SpazerIceWave or
                    SamusBeamCombination.PlasmaIceWave => RightIce,
                SamusBeamCombination.Power or SamusBeamCombination.Ice or SamusBeamCombination.Spazer or
                    SamusBeamCombination.SpazerWave or SamusBeamCombination.Plasma or
                    SamusBeamCombination.PlasmaWave or SamusBeamCombination.PlasmaIce or
                    SamusBeamCombination.SpazerPlasma or SamusBeamCombination.SpazerPlasmaWave or
                    SamusBeamCombination.SpazerPlasmaIce or SamusBeamCombination.SpazerPlasmaIceWave => Empty,
                _ => throw new ArgumentOutOfRangeException(nameof(selection), selection, "Undefined beam combination."),
            }
            : type.BeamCombination switch
            {
                SamusBeamCombination.Wave => Wave,
                SamusBeamCombination.Power => charged ? LeftIce : Empty,
                SamusBeamCombination.Ice or SamusBeamCombination.IceWave or SamusBeamCombination.SpazerIce or
                    SamusBeamCombination.SpazerIceWave or SamusBeamCombination.PlasmaIce or
                    SamusBeamCombination.PlasmaIceWave => LeftIce,
                SamusBeamCombination.Spazer or SamusBeamCombination.SpazerWave or SamusBeamCombination.Plasma or
                    SamusBeamCombination.PlasmaWave or SamusBeamCombination.SpazerPlasma or
                    SamusBeamCombination.SpazerPlasmaWave or SamusBeamCombination.SpazerPlasmaIce or
                    SamusBeamCombination.SpazerPlasmaIceWave => Empty,
                _ => throw new ArgumentOutOfRangeException(nameof(selection), selection, "Undefined beam combination."),
            };
    }
}