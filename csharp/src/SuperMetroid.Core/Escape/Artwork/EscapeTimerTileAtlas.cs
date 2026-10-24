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
    public ReadOnlyMemory<byte> Resolve(VramAssetId asset)
    {
        return asset switch
        {
            VramAssetId.EscapeTimerFirstTiles => characters.AsMemory(0, EscapeTimerTileAtlasFormat.FirstByteCount),
            VramAssetId.EscapeTimerSecondTiles => characters.AsMemory(EscapeTimerTileAtlasFormat.FirstByteCount, EscapeTimerTileAtlasFormat.SecondByteCount),
            VramAssetId.None or
                VramAssetId.StandardHudTiles or
                VramAssetId.BeamPowerTiles or
                VramAssetId.BeamWaveTiles or
                VramAssetId.BeamIceTiles or
                VramAssetId.BeamIceWaveTiles or
                VramAssetId.BeamSpazerTiles or
                VramAssetId.BeamSpazerWaveTiles or
                VramAssetId.BeamSpazerIceTiles or
                VramAssetId.BeamSpazerIceWaveTiles or
                VramAssetId.BeamPlasmaTiles or
                VramAssetId.BeamPlasmaWaveTiles or
                VramAssetId.BeamPlasmaIceTiles or
                VramAssetId.BeamPlasmaIceWaveTiles or
                VramAssetId.ProjectileIceWaveTrailTiles or
                VramAssetId.ProjectileMissileTrailTiles or
                VramAssetId.GrapplePointFirstTiles or
                VramAssetId.GrapplePointSecondTiles or
                VramAssetId.GrapplePointThirdTiles or
                VramAssetId.GrapplePointFourthTiles or
                VramAssetId.GrappleHorizontalSegmentTiles or
                VramAssetId.GrappleDiagonalSegmentTiles or
                VramAssetId.GrappleVerticalSegmentTiles or
                VramAssetId.KraidBg3RestoreQuarter0 or
                VramAssetId.KraidBg3RestoreQuarter1 or
                VramAssetId.KraidBg3RestoreQuarter2 or
                VramAssetId.KraidBg3RestoreQuarter3 or
                VramAssetId.GunshipLiftoffFirstTiles or
                VramAssetId.GunshipLiftoffSecondTiles or
                VramAssetId.GunshipLiftoffThirdTiles or
                VramAssetId.GunshipLiftoffFourthTiles or
                VramAssetId.GunshipLiftoffFifthTiles or
                VramAssetId.BeamChainsawTiles or
                VramAssetId.BeamSpacetimeTiles or
                VramAssetId.StandardObjectTiles or
                VramAssetId.HudTopRowTilemap => throw new InvalidDataException($"Escape timer artwork cannot resolve VRAM asset {asset}."),
            _ => throw new InvalidDataException($"Escape timer artwork cannot resolve VRAM asset {asset}."),
        };
    }

}
