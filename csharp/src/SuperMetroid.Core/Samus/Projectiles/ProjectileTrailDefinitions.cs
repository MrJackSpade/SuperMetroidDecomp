namespace SuperMetroid.Core.Samus.Projectiles;

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
    private enum TrailList
    {
        /// <summary>$90:B4C9 ends a trail without drawing another frame.</summary>
        Empty,
        /// <summary>$90:B4CB draws the left ice or charged-power beam trail.</summary>
        LeftIce,
        /// <summary>$90:B52D draws the right-side ice sparkle trail.</summary>
        RightIce,
        /// <summary>$90:B58F supplies the wave-beam trail for either side.</summary>
        Wave,
        /// <summary>$90:B5A1 is shared by ordinary and super-missile trail selection.</summary>
        Missile,
    }

    /// <summary>All five defined trail lists used to recognize a first-record cursor.</summary>
    private static readonly TrailList[] TrailLists = Enum.GetValues<TrailList>();

    /// <summary>Returns the native entry pointer for one defined trail list.</summary>
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

    /// <summary>
    /// Returns the complete bounded native selector window, including left-table overlap and adjacent observations.
    /// The right selector's low-six-bit index can reach past its thirty-nine entries into the trail spawner's code at
    /// $90:B657-$B688; those words are cartridge observations, which <paramref name="overreads"/> holds.
    /// </summary>
    public static ushort ReadSelector(int address, SamusOverreadCatalog? overreads)
    {
        int offset = address - SamusProjectileRomData.Trails.LeftInstructionPointers;
        if ((uint)offset >= 103 * sizeof(ushort) || (offset & 1) != 0)
            throw new InvalidDataException($"Projectile trail selector ${address:X6} is outside the bounded low-six-bit observation window.");
        int index = offset / sizeof(ushort);
        if (index >= 78)
            return (overreads ?? throw new InvalidOperationException(
                "A trail selector reading past its entries needs the installed Samus overread catalog."))
                .TrailSelectorAdjacentWord(index - 78);
        return Select(index % 39, right: index >= 39);
    }

    /// <summary>Chooses a left or right trail list from beam effects and special projectile selectors.</summary>
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
