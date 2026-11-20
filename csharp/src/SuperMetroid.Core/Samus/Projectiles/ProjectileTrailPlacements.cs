using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>Finds where a projectile's trail starts: its placement list and the placement the list gives its frame.</summary>
internal static class ProjectileTrailPlacements
{
    /// <summary>
    /// The list a trail of <paramref name="beams"/> from selector <paramref name="group"/> reads for a shot fired in
    /// <paramref name="direction"/>, or null when the beams' group names no family (#1318).
    /// </summary>
    internal static ProjectileTrailPlacementList? List(ProjectileTrailFamilyGroup group, SamusBeamCombination beams, SamusProjectileDirection direction) =>
        ProjectileTrailPlacementDefinitions.Families[new(group, beams)] is { } family
            ? ProjectileTrailPlacementDefinitions.Lists[family].For(direction)
            : null;

    /// <summary>The list the Spazer special attack's trailing pair, fired in <paramref name="direction"/>, starts its trail from (#2348).</summary>
    internal static ProjectileTrailPlacementList SpazerAttackTrailingPairList(SamusProjectileDirection direction) =>
        ProjectileTrailPlacementDefinitions.SpazerAttackTrailingPairLists.For(direction);

    /// <summary>
    /// The placement <paramref name="list"/> gives a trail from a shot showing <paramref name="frame"/>, or null for a frame
    /// the list places no trail for, which leaves none: what lies past a list is not part of the game's design (#1625).
    /// </summary>
    internal static ProjectileTrailPlacement? Start(ProjectileTrailPlacementList list, SamusProjectileFrameId frame) =>
        ProjectileTrailPlacementDefinitions.PlacementsByList[list].TryGetValue(frame, out ProjectileTrailPlacement placement)
            ? placement
            : null;
}
