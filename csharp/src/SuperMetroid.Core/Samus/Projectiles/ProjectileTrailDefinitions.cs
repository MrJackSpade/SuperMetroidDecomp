using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>The trail programs $90:B5BB/B609 select for each side, including the native selector overlap.</summary>
public static class ProjectileTrailDefinitions
{
    /// <summary>The thirty-nine selections each side's table holds.</summary>
    private const int TableSelections = 39;

    /// <summary>The non-beam trail selections with a trail program; every other one draws none.</summary>
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
    /// The program <paramref name="side"/> runs for low-six-bit selection <paramref name="selection"/>. Native indexes each
    /// side's thirty-nine-entry table with all six bits: a left selection past its table reads the right table's entries,
    /// and a right selection past its table reads the trail spawner's own code, which names no trail program, so that
    /// trail is not drawn (null).
    /// </summary>
    internal static ProjectileTrailProgram? Select(int selection, HorizontalDirection side)
    {
        if ((uint)selection >= 64)
            throw new ArgumentOutOfRangeException(nameof(selection), selection, "A trail selection is six bits.");
        if (selection < TableSelections)
            return Select(selection, right: side == HorizontalDirection.Right);
        return side == HorizontalDirection.Left ? Select(selection - TableSelections, right: true) : null;
    }

    /// <summary>Chooses a left or right trail program from beam effects and special projectile selectors.</summary>
    private static ProjectileTrailProgram Select(int selection, bool right)
    {
        if (selection >= (int)SpecialSelection.Missile)
        {
            var special = (SpecialSelection)selection;
            if (!Enum.IsDefined(special))
                return ProjectileTrailProgram.Empty;
            return special switch
            {
                SpecialSelection.Missile or SpecialSelection.SuperMissile => right ? ProjectileTrailProgram.Empty : ProjectileTrailProgram.Missile,
                SpecialSelection.SpazerSbaFirst or SpecialSelection.SpazerSbaLast => ProjectileTrailProgram.LeftIce,
                SpecialSelection.SpazerSbaMiddle => right ? ProjectileTrailProgram.RightIce : ProjectileTrailProgram.LeftIce,
                _ => throw new InvalidOperationException($"Undefined SpecialSelection {special}."),
            };
        }
        var type = new SamusProjectileTypeWord((ushort)selection);
        bool charged = type.IsChargedBeam;
        // Ice draws its sparkle unless Spazer and Plasma are both present.
        return right
            ? type.BeamCombination switch
            {
                SamusBeamCombination.Wave => charged ? ProjectileTrailProgram.Wave : ProjectileTrailProgram.Empty,
                SamusBeamCombination.IceWave => charged ? ProjectileTrailProgram.RightIce : ProjectileTrailProgram.Empty,
                SamusBeamCombination.SpazerIce or SamusBeamCombination.SpazerIceWave or
                    SamusBeamCombination.PlasmaIceWave => ProjectileTrailProgram.RightIce,
                SamusBeamCombination.Power or SamusBeamCombination.Ice or SamusBeamCombination.Spazer or
                    SamusBeamCombination.SpazerWave or SamusBeamCombination.Plasma or
                    SamusBeamCombination.PlasmaWave or SamusBeamCombination.PlasmaIce or
                    SamusBeamCombination.SpazerPlasma or SamusBeamCombination.SpazerPlasmaWave or
                    SamusBeamCombination.SpazerPlasmaIce or SamusBeamCombination.SpazerPlasmaIceWave => ProjectileTrailProgram.Empty,
                _ => throw new ArgumentOutOfRangeException(nameof(selection), selection, "Undefined beam combination."),
            }
            : type.BeamCombination switch
            {
                SamusBeamCombination.Wave => ProjectileTrailProgram.Wave,
                SamusBeamCombination.Power => charged ? ProjectileTrailProgram.LeftIce : ProjectileTrailProgram.Empty,
                SamusBeamCombination.Ice or SamusBeamCombination.IceWave or SamusBeamCombination.SpazerIce or
                    SamusBeamCombination.SpazerIceWave or SamusBeamCombination.PlasmaIce or
                    SamusBeamCombination.PlasmaIceWave => ProjectileTrailProgram.LeftIce,
                SamusBeamCombination.Spazer or SamusBeamCombination.SpazerWave or SamusBeamCombination.Plasma or
                    SamusBeamCombination.PlasmaWave or SamusBeamCombination.SpazerPlasma or
                    SamusBeamCombination.SpazerPlasmaWave or SamusBeamCombination.SpazerPlasmaIce or
                    SamusBeamCombination.SpazerPlasmaIceWave => ProjectileTrailProgram.Empty,
                _ => throw new ArgumentOutOfRangeException(nameof(selection), selection, "Undefined beam combination."),
            };
    }
}
