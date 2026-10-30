using SuperMetroid.Core.Progression;
using static SuperMetroid.Core.Samus.Projectiles.ProjectileTrailDefinitions;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>The trail program each side's selection runs.</summary>
public static class ProjectileTrailSelection
{
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
        if (selection >= FirstSpecialSelection)
        {
            ProjectileTrailPrograms programs = SpecialSelectionPrograms[selection - FirstSpecialSelection];
            return right ? programs.Right : programs.Left;
        }
        bool charged = selection >= SamusFiringTableDefinitions.ChargedRowOffset;
        SamusBeamCombination beams = SamusBeamCombinations.FromTableIndex(selection % SamusFiringTableDefinitions.ChargedRowOffset);
        // Ice draws its sparkle unless Spazer and Plasma are both present.
        return right
            ? beams switch
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
            : beams switch
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
