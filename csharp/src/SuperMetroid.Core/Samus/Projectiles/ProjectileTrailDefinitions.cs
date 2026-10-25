using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>The trail programs $90:B5BB/B609 select for each side, including the native selector overlap.</summary>
public static class ProjectileTrailDefinitions
{
    /// <summary>The thirty-nine selections each side's table holds.</summary>
    private const int TableSelections = 39;

    /// <summary>The first special (non-beam) selection; beam selections precede it.</summary>
    private const int FirstSpecialSelection = 32;

    /// <summary>
    /// $90:B5FB..B607 and $90:B649..B655: the left and right programs of the special selections, from the first special
    /// selection to the table's end. Missile and Super Missile (the first two, after the native family-to-index
    /// conversion) trail on the left only; the next two draw none; the three Spazer SBA selections draw the left ice
    /// trail, except that the middle one draws the right ice trail on the right.
    /// </summary>
    private static readonly ProjectileTrailPrograms[] SpecialSelectionPrograms =
    [
        new(ProjectileTrailProgram.Missile, ProjectileTrailProgram.Empty),
        new(ProjectileTrailProgram.Missile, ProjectileTrailProgram.Empty),
        new(ProjectileTrailProgram.Empty, ProjectileTrailProgram.Empty),
        new(ProjectileTrailProgram.Empty, ProjectileTrailProgram.Empty),
        new(ProjectileTrailProgram.LeftIce, ProjectileTrailProgram.LeftIce),
        new(ProjectileTrailProgram.LeftIce, ProjectileTrailProgram.RightIce),
        new(ProjectileTrailProgram.LeftIce, ProjectileTrailProgram.LeftIce),
    ];

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
