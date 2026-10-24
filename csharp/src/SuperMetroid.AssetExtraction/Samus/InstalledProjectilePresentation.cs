using SuperMetroid.Core;
using SuperMetroid.Core.Samus.Artwork;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>Loaded content and separate original/selected byte identities for diagnostics.</summary>
/// <param name="Catalog">Projectile, bomb and flare compositions.</param>
/// <param name="StockSha256">Identity of the stock projectile files.</param>
/// <param name="SelectedSha256">Identity of the selected projectile files.</param>
/// <param name="BeamTiles">Beam characters by combination, with the beam palettes and Hyper Beam colours.</param>
/// <param name="Trails">Trail frame appearances.</param>
/// <param name="TrailTiles">The ice/wave and missile trail characters the trail appearances name.</param>
/// <param name="FlarePlacement">Charge-flare placement.</param>
/// <param name="FlareCompositions">Charge-flare compositions.</param>
/// <param name="GrappleTiles">Grapple characters, with the grapple's sprite, flare and swing catalogs.</param>
/// <param name="FrameBindings">Timed projectile frame sprite choices.</param>
[ArchitectureReview("context-bag", "The installed projectile presentation the extraction read; every member is loaded artwork data or its identity, not a service.")]
public sealed record InstalledProjectilePresentation(ProjectileSpriteCatalog Catalog, string StockSha256, string SelectedSha256, BeamTileCatalog BeamTiles, ProjectileTrailCatalog Trails, ProjectileTrailAtlas TrailTiles, ChargeFlarePlacementCatalog FlarePlacement, ChargeFlareSpriteCatalog FlareCompositions, GrappleTileAtlas GrappleTiles, ProjectileFrameBindingCatalog FrameBindings);
