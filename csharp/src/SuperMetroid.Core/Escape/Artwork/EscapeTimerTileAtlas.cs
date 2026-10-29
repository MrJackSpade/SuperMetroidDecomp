using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Escape.Artwork;

/// <summary>
/// The installed four-bit OBJ characters the Ceres and Zebes escape timers show (#1409), compiled by the import pipeline
/// from the editable timer typeface. They remain bytes here because the OBJ character sources resolve the timer's pages.
/// </summary>
public class EscapeTimerTileAtlas
{
    /// <summary>The 25 timer characters in native four-bit planar DMA order.</summary>
    private readonly byte[] characters;

    /// <summary>Retains the compiled timer characters.</summary>
    /// <param name="characters">The 25 timer characters in transfer order.</param>
    internal EscapeTimerTileAtlas(byte[] characters)
    {
        this.characters = characters;
        Artwork = new SpriteArtwork(new SpriteArtworkId(SpriteArtworkKind.EscapeTimerTiles), characters);
    }

    /// <summary>The 25 timer characters as owned sprite artwork, in transfer order: character 0 is OBJ character $1E0.</summary>
    public SpriteArtwork Artwork { get; }

    /// <summary>Resolves one native page without combining or retiming its original transfer record.</summary>
    public ReadOnlyMemory<byte> Resolve(InstalledArtworkId asset)
    {
        return asset switch
        {
            InstalledArtworkId.EscapeTimerFirstTiles => characters.AsMemory(0, EscapeTimerTileAtlasFormat.FirstByteCount),
            InstalledArtworkId.EscapeTimerSecondTiles => characters.AsMemory(EscapeTimerTileAtlasFormat.FirstByteCount, EscapeTimerTileAtlasFormat.SecondByteCount),
            InstalledArtworkId.StandardHudTiles or
                InstalledArtworkId.BeamPowerTiles or
                InstalledArtworkId.BeamWaveTiles or
                InstalledArtworkId.BeamIceTiles or
                InstalledArtworkId.BeamIceWaveTiles or
                InstalledArtworkId.BeamSpazerTiles or
                InstalledArtworkId.BeamSpazerWaveTiles or
                InstalledArtworkId.BeamSpazerIceTiles or
                InstalledArtworkId.BeamSpazerIceWaveTiles or
                InstalledArtworkId.BeamPlasmaTiles or
                InstalledArtworkId.BeamPlasmaWaveTiles or
                InstalledArtworkId.BeamPlasmaIceTiles or
                InstalledArtworkId.BeamPlasmaIceWaveTiles or
                InstalledArtworkId.ProjectileIceWaveTrailTiles or
                InstalledArtworkId.ProjectileMissileTrailTiles or
                InstalledArtworkId.GrapplePointFirstTiles or
                InstalledArtworkId.GrapplePointSecondTiles or
                InstalledArtworkId.GrapplePointThirdTiles or
                InstalledArtworkId.GrapplePointFourthTiles or
                InstalledArtworkId.GrappleHorizontalSegmentTiles or
                InstalledArtworkId.GrappleDiagonalSegmentTiles or
                InstalledArtworkId.GrappleVerticalSegmentTiles or
                InstalledArtworkId.GunshipLiftoffFirstTiles or
                InstalledArtworkId.GunshipLiftoffSecondTiles or
                InstalledArtworkId.GunshipLiftoffThirdTiles or
                InstalledArtworkId.GunshipLiftoffFourthTiles or
                InstalledArtworkId.GunshipLiftoffFifthTiles or
                InstalledArtworkId.BeamChainsawTiles or
                InstalledArtworkId.BeamSpacetimeTiles or
                InstalledArtworkId.StandardObjectTiles or
                InstalledArtworkId.HudTopRowTilemap => throw new InvalidDataException($"Escape timer artwork cannot resolve VRAM asset {asset}."),
            _ => throw new InvalidDataException($"Escape timer artwork cannot resolve VRAM asset {asset}."),
        };
    }

}
