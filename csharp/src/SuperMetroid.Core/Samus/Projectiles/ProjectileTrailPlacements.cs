using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>Finds where a projectile's trail starts: its placement list and the placement its animation frame reads.</summary>
internal static class ProjectileTrailPlacements
{
    /// <summary>
    /// The list a trail of <paramref name="beams"/> from selector <paramref name="group"/> reads for direction-table index
    /// <paramref name="directionIndex"/>, or null when its row or direction entry names none, which draws no trail. An
    /// empty row's direction table lies in direct-page scratch the port does not model (owner decision on #1318).
    /// </summary>
    internal static ProjectileTrailPlacementList? List(ProjectileTrailFamilyGroup group, SamusBeamCombination beams, int directionIndex) =>
        ProjectileTrailPlacementDefinitions.Families[new(group, beams)] is { } family
            ? ProjectileTrailPlacementDefinitions.Lists[family][directionIndex]
            : null;

    /// <summary>
    /// The placement a trail reads from <paramref name="list"/> at animation frame <paramref name="frame"/>: a placement
    /// record, or past the run of records what <paramref name="starts"/> decoded there. Null where the read reaches
    /// scratch memory, which draws no trail (#1318).
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">No trail reads past the list's records at that frame.</exception>
    internal static ProjectileTrailPlacement? Start(ProjectileTrailPlacementList list, int frame,
        TrailStartPastPlacements<SamusProjectileProgram, ProjectileTrailPlacementList, ProjectileTrailPlacement> starts)
    {
        Ensure.NotNull(starts);
        ProjectileTrailPlacement[] records = ProjectileTrailPlacementDefinitions.Records[list];
        return frame < records.Length ? records[frame] : starts.PastRecords(list, frame);
    }
}
