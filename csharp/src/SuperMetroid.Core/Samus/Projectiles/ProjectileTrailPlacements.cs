using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>Finds where a projectile's trail starts: its placement list and the placement its animation frame reads.</summary>
internal static class ProjectileTrailPlacements
{
    /// <summary>
    /// The list a trail of <paramref name="beams"/> from selector <paramref name="group"/> reads for a shot fired in
    /// <paramref name="direction"/>, or null when its row or direction entry names none, which draws no trail. An
    /// empty row's direction table is left-over values the model does not keep (owner decision on #1318).
    /// </summary>
    internal static ProjectileTrailPlacementList? List(ProjectileTrailFamilyGroup group, SamusBeamCombination beams, SamusProjectileDirection direction) =>
        ProjectileTrailPlacementDefinitions.Families[new(group, beams)] is { } family
            ? ProjectileTrailPlacementDefinitions.Lists[family].For(direction)
            : null;

    /// <summary>
    /// The placement a trail reads from <paramref name="list"/> at animation frame <paramref name="frame"/>, or null for a
    /// frame past the list's placements, which leaves no trail: what lies past a list is not part of the game's design
    /// (#1625).
    /// </summary>
    internal static ProjectileTrailPlacement? Start(ProjectileTrailPlacementList list, int frame)
    {
        ProjectileTrailPlacement[] records = ProjectileTrailPlacementDefinitions.Records[list];
        return frame >= 0 && frame < records.Length ? records[frame] : null;
    }
}
