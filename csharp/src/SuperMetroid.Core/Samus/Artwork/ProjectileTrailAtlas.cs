using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>Twelve trail tiles in one indexed sheet, compiled into two noncontiguous VRAM transfers.</summary>
public sealed class ProjectileTrailAtlas : IVramAssetProvider
{
    /// <summary>Encoded four-bit planar characters in ice/wave order followed by missile trail art, with no native VRAM gap in the file.</summary>
    private readonly byte[] tiles;
    /// <summary>Retains the encoded twelve-character sheet for its two distinct upload ranges.</summary>
    private ProjectileTrailAtlas(byte[] tiles) => this.tiles = tiles;
    /// <summary>First eight 4-bpp characters, $0100 bytes corresponding to $9A:D900 and standard OBJ tiles $38..$3F, for ice/wave trail frames.</summary>
    public ReadOnlyMemory<byte> IceAndWave => tiles.AsMemory(0, ProjectileTrailAtlasDefinitions.IceWaveByteCount);
    /// <summary>Last four 4-bpp characters, $0080 bytes corresponding to $9A:DB00 and standard OBJ tiles $48..$4B, for Missile and Super Missile trails.</summary>
    public ReadOnlyMemory<byte> Missile => tiles.AsMemory(ProjectileTrailAtlasDefinitions.IceWaveByteCount);
    /// <summary>Resolves one of the two trail-owned VRAM assets without uploading it or changing projectile/trail timing.</summary>
    /// <param name="asset">Ice/wave or missile trail tile asset identity.</param>
    /// <returns>The selected native planar character bytes.</returns>
    /// <exception cref="InvalidDataException">The requested identity is not owned by this trail atlas.</exception>
    public ReadOnlyMemory<byte> Resolve(VramAssetId asset) => asset switch
    {
        VramAssetId.ProjectileIceWaveTrailTiles => IceAndWave,
        VramAssetId.ProjectileMissileTrailTiles => Missile,
        VramAssetId.None or
            VramAssetId.StandardHudTiles or
            VramAssetId.EscapeTimerFirstTiles or
            VramAssetId.EscapeTimerSecondTiles or
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
            VramAssetId.HudTopRowTilemap => throw new InvalidDataException($"Trail atlas cannot resolve {asset}."),
        _ => throw new InvalidDataException($"Trail atlas cannot resolve {asset}."),
    };
    /// <summary>Loads the 96-by-8 indexed PNG as twelve left-to-right 8-pixel tiles and encodes their palette indices into four-bit SNES planar character bytes.</summary>
    /// <param name="png">Caller-owned image stream; indices 0..15 are pens rather than final CGRAM colors.</param>
    /// <returns>Installed character art split into eight ice/wave tiles followed by four missile tiles, excluding the native inter-range gap.</returns>
    public static ProjectileTrailAtlas Load(Stream png)
    {
        var image = IndexedPng.Read(png, new(ProjectileTrailAtlasDefinitions.Width, ProjectileTrailAtlasDefinitions.Height));
        return new(SnesPlanarTileEncoder.Encode(image.Pixels, image.Size, 4));
    }
}
