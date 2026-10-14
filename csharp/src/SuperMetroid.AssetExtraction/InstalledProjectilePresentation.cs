using SuperMetroid.Core.Assets;

namespace SuperMetroid.AssetExtraction;

/// <summary>Loaded content and separate original/selected byte identities for diagnostics.</summary>
public sealed record InstalledProjectilePresentation(ProjectileSpriteCatalog Catalog, string StockSha256, string SelectedSha256, BeamTileCatalog BeamTiles, ProjectileTrailCatalog Trails, ChargeFlarePlacementCatalog FlarePlacement, ChargeFlareSpriteCatalog FlareCompositions, GrappleTileAtlas GrappleTiles, ProjectileFrameBindingCatalog FrameBindings);
