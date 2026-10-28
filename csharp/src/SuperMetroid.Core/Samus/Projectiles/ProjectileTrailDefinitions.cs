
namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>The trail programs $90:B5BB/B609 select for each side, including the native selector overlap.</summary>
public static class ProjectileTrailDefinitions
{
    /// <summary>The thirty-nine selections each side's table holds.</summary>
    internal const int TableSelections = 39;

    /// <summary>The first special (non-beam) selection; beam selections precede it.</summary>
    internal const int FirstSpecialSelection = 32;

    /// <summary>
    /// $90:B5FB..B607 and $90:B649..B655: the left and right programs of the special selections, from the first special
    /// selection to the table's end. Missile and Super Missile (the first two, after the native family-to-index
    /// conversion) trail on the left only; the next two draw none; the three Spazer SBA selections draw the left ice
    /// trail, except that the middle one draws the right ice trail on the right.
    /// </summary>
    internal static readonly ProjectileTrailPrograms[] SpecialSelectionPrograms =
    [
        new(ProjectileTrailProgram.Missile, ProjectileTrailProgram.Empty),
        new(ProjectileTrailProgram.Missile, ProjectileTrailProgram.Empty),
        new(ProjectileTrailProgram.Empty, ProjectileTrailProgram.Empty),
        new(ProjectileTrailProgram.Empty, ProjectileTrailProgram.Empty),
        new(ProjectileTrailProgram.LeftIce, ProjectileTrailProgram.LeftIce),
        new(ProjectileTrailProgram.LeftIce, ProjectileTrailProgram.RightIce),
        new(ProjectileTrailProgram.LeftIce, ProjectileTrailProgram.LeftIce),
    ];
}
