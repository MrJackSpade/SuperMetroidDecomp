using SuperMetroid.AssetExtraction.Samus.Artwork;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>
/// The projectile presentation as the import reads it (#1636): the installed documents with the beam, trail and grapple
/// characters and the beam colours (#1637) from the component's import data, which the projectile and grapple sheets are
/// composed from. The game reads only <see cref="Presentation"/>.
/// </summary>
/// <param name="Presentation">The installed documents, as the game reads them.</param>
/// <param name="BeamTiles">Beam characters by combination.</param>
/// <param name="BeamPalettes">The beam and grapple colours the projectile and frozen-enemy sheets are pre-coloured in.</param>
/// <param name="TrailTiles">The ice/wave and missile trail characters the trail appearances name.</param>
/// <param name="GrappleTiles">Grapple characters.</param>
public sealed record ImportProjectileArtwork(InstalledProjectilePresentation Presentation, BeamTileCatalog BeamTiles, InstalledBeamPalettes BeamPalettes,
    ProjectileTrailAtlas TrailTiles,
    GrappleTileAtlas GrappleTiles);
