using SuperMetroid.Core.Progression;
using static SuperMetroid.Core.Samus.Projectiles.ProjectileTrailDefinitions;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>The trail program each side's selection runs.</summary>
public static class ProjectileTrailSelection
{
    /// <summary>
    /// The program <paramref name="side"/> runs for selection <paramref name="selection"/>, one of the thirty-nine each
    /// side's table holds.
    /// </summary>
    /// <remarks>
    /// The game indexes both tables with the low six bits of the shot's type, so a selection past thirty-eight would read
    /// the right table from the left and the trail spawner's own code from the right. No shot reaches one (#1500): only
    /// the ordinary beam updates, the missile and Super Missile updates and the Ice, Wave and Spazer special attacks spawn
    /// trails, and their selections are a beam row of 0 through 31, the missiles' 32 and 33, and the Spazer attack's
    /// trailing pair's 36. The Shinespark crash's echo, at 41, has its own update, which spawns none. A selection past
    /// the table is therefore rejected.
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">The selection lies past the side's table.</exception>
    internal static ProjectileTrailProgram Select(int selection, HorizontalDirection side) =>
        (uint)selection < TableSelections
            ? Select(selection, right: side == HorizontalDirection.Right)
            : throw new ArgumentOutOfRangeException(nameof(selection), selection, "No shot that spawns a trail selects one past the trail tables.");

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
