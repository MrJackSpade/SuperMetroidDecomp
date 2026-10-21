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
    /// record, or past the run of records the installed placement that read reaches. Null where the read reaches
    /// direct-page scratch, which draws no trail (#1318).
    /// </summary>
    /// <exception cref="InvalidOperationException">The read passes the run of records and no overread catalog is installed.</exception>
    internal static ProjectileTrailPlacement? Start(ProjectileTrailPlacementList list, int frame, SamusOverreadCatalog? overreads)
    {
        ProjectileTrailPlacement[] records = ProjectileTrailPlacementDefinitions.Records[list];
        if (frame < records.Length)
            return records[frame];
        return (overreads ?? throw new InvalidOperationException(
            $"A trail reading {list} past its placements needs the installed Samus overread catalog.")).TrailOverrun(list, frame);
    }

    /// <summary>
    /// The frames a trail can read <paramref name="list"/> at past its placements, ascending: every Samus projectile
    /// record's animation frame and every word before a projectile list, which a trail spawned at a list's start reads.
    /// </summary>
    internal static IReadOnlyList<int> OverrunFrames(ProjectileTrailPlacementList list) =>
        [.. FrameDomain.Where(frame => frame >= ProjectileTrailPlacementDefinitions.Records[list].Length)];

    /// <summary>Every frame a trail can read: each record's animation frame and each list-start word, ascending.</summary>
    private static readonly int[] FrameDomain =
    [
        .. SamusProjectileRecordDefinitions.Records.Select(record => record.AnimationFrame)
            .Concat(Enum.GetValues<SamusProjectileProgram>().Select(program => (int)SamusProjectileProgramAddresses.Prefix(program)))
            .Distinct()
            .Order(),
    ];
}
