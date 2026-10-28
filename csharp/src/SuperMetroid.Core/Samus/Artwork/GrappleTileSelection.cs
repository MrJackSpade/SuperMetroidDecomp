using SuperMetroid.Core.Assets;
using static SuperMetroid.Core.Samus.Artwork.GrappleTileDefinitions;

namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>The orientation, segment and point artwork and transfer each grapple angle selects.</summary>
public static class GrappleTileSelection
{
    /// <summary>
    /// The rope character set the grapple's angle selects: `$9B:C005` folds the unsigned angle into an even table byte
    /// offset, equivalent to angle / 1024.
    /// </summary>
    public static GrappleRopeOrientation OrientationFor(RotationAngle angle)
    {
        // The 64 sectors repeat after half a turn. Reflect each half-turn about
        // its midpoint, retaining the native three/eight/five sector widths.
        int halfTurnSector = (angle.RawValue >> 10) & 31;
        int distance = Math.Min(halfTurnSector, 31 - halfTurnSector);
        return distance < 3 ? GrappleRopeOrientation.Vertical : distance < 11 ? GrappleRopeOrientation.Diagonal : GrappleRopeOrientation.Horizontal;
    }

    /// <summary>The installed rope characters of an orientation.</summary>
    public static InstalledArtworkId SegmentAssetFor(GrappleRopeOrientation orientation) => orientation switch
    {
        GrappleRopeOrientation.Horizontal => Horizontal,
        GrappleRopeOrientation.Diagonal => Diagonal,
        GrappleRopeOrientation.Vertical => Vertical,
        _ => throw new ArgumentOutOfRangeException(nameof(orientation), orientation, "Undefined rope orientation."),
    };

    /// <summary>$9B:BFBD cycles endpoint characters at $8200/$8400/$8600/$8800, independently of rope angle.</summary>
    public static InstalledArtworkId PointAssetFor(int frame) => frame switch
    {
        0 => InstalledArtworkId.GrapplePointFirstTiles,
        1 => InstalledArtworkId.GrapplePointSecondTiles,
        2 => InstalledArtworkId.GrapplePointThirdTiles,
        3 => InstalledArtworkId.GrapplePointFourthTiles,
        _ => throw new ArgumentOutOfRangeException(nameof(frame)),
    };

    /// <summary>Resolves one of seven Grapple-owned upload identities to its native source and planar-sheet slice; this lookup neither advances endpoint animation nor enqueues a VRAM transfer.</summary>
    /// <param name="asset">One of the four endpoint-frame identities or the horizontal, diagonal, or vertical segment identity.</param>
    /// <returns>A 32-byte endpoint or 128-byte segment descriptor; destination VRAM words are separately defined by <c>PointDestination</c> and <c>SegmentDestination</c>.</returns>
    /// <exception cref="InvalidDataException">The asset identity is not owned by the Grapple tile catalog.</exception>
    public static GrappleTileTransfer TransferFor(InstalledArtworkId asset)
    {
        int index = asset switch
        {
            InstalledArtworkId.GrapplePointFirstTiles => 0,
            InstalledArtworkId.GrapplePointSecondTiles => 1,
            InstalledArtworkId.GrapplePointThirdTiles => 2,
            InstalledArtworkId.GrapplePointFourthTiles => 3,
            Horizontal => 4,
            Diagonal => 5,
            Vertical => 6,
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
                InstalledArtworkId.ProjectileIceWaveTrailTiles or
                InstalledArtworkId.ProjectileMissileTrailTiles or
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
                InstalledArtworkId.HudTopRowTilemap => throw new InvalidDataException($"Not a Grapple tile asset: {asset}."),
            _ => throw new InvalidDataException($"Not a Grapple tile asset: {asset}."),
        };
        return Transfers[index];
    }
}
