using SuperMetroid.Core.Assets;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>Twelve trail tiles in one indexed sheet, compiled into two noncontiguous VRAM transfers.</summary>
public sealed class ProjectileTrailAtlas : IInstalledArtworkContent
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
    public ReadOnlyMemory<byte> Resolve(InstalledArtworkId asset) => asset switch
    {
        InstalledArtworkId.ProjectileIceWaveTrailTiles => IceAndWave,
        InstalledArtworkId.ProjectileMissileTrailTiles => Missile,
        InstalledArtworkId.StandardHudTiles or
            InstalledArtworkId.EscapeTimerFirstTiles or
            InstalledArtworkId.EscapeTimerSecondTiles or
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
            InstalledArtworkId.GrapplePointFirstTiles or
            InstalledArtworkId.GrapplePointSecondTiles or
            InstalledArtworkId.GrapplePointThirdTiles or
            InstalledArtworkId.GrapplePointFourthTiles or
            InstalledArtworkId.GrappleHorizontalSegmentTiles or
            InstalledArtworkId.GrappleDiagonalSegmentTiles or
            InstalledArtworkId.GrappleVerticalSegmentTiles or
            InstalledArtworkId.KraidBg3RestoreQuarter0 or
            InstalledArtworkId.KraidBg3RestoreQuarter1 or
            InstalledArtworkId.KraidBg3RestoreQuarter2 or
            InstalledArtworkId.KraidBg3RestoreQuarter3 or
            InstalledArtworkId.GunshipLiftoffFirstTiles or
            InstalledArtworkId.GunshipLiftoffSecondTiles or
            InstalledArtworkId.GunshipLiftoffThirdTiles or
            InstalledArtworkId.GunshipLiftoffFourthTiles or
            InstalledArtworkId.GunshipLiftoffFifthTiles or
            InstalledArtworkId.BeamChainsawTiles or
            InstalledArtworkId.BeamSpacetimeTiles or
            InstalledArtworkId.StandardObjectTiles or
            InstalledArtworkId.HudTopRowTilemap => throw new InvalidDataException($"Trail atlas cannot resolve {asset}."),
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
