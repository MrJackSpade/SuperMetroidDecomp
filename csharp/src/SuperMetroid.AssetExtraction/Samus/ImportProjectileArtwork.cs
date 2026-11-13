using SuperMetroid.AssetExtraction.Samus.Artwork;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>
/// The projectile presentation as the import reads it (#1636): the installed documents with the beam, trail and grapple
/// characters, the beam colours (#1637), the Hyper Beam's colour cycle (#1693) and the compositions (#1638) from the
/// component's import data, which the projectile, grapple and Samus sheets are composed and pre-coloured from. The game
/// reads only <see cref="Presentation"/>.
/// </summary>
/// <param name="Presentation">The installed documents, as the game reads them.</param>
/// <param name="BeamTiles">Beam characters by combination.</param>
/// <param name="BeamPalettes">The beam and grapple colours the projectile and frozen-enemy sheets are pre-coloured in.</param>
/// <param name="HyperBeamFxColors">The Hyper Beam effect's colour cycle, which Samus's sheets are pre-coloured in (#1693).</param>
/// <param name="TrailTiles">The ice/wave and missile trail characters the trail appearances name.</param>
/// <param name="GrappleTiles">Grapple characters.</param>
/// <param name="Compositions">The projectile, flare, trail and grapple compositions the sheets' pictures are composed from.</param>
public sealed record ImportProjectileArtwork(InstalledProjectilePresentation Presentation, BeamTileCatalog BeamTiles, InstalledBeamPalettes BeamPalettes,
    HyperBeamFxColours HyperBeamFxColors, ProjectileTrailAtlas TrailTiles,
    GrappleTileAtlas GrappleTiles, ProjectileCompositions Compositions);
