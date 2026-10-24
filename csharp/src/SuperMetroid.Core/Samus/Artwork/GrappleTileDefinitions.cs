namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>Grapple tile ownership and the pinned $9B:C342/C346 endpoint/angle selection tables.</summary>
public static class GrappleTileDefinitions
{
    /// <summary>Indexed PNG resource for the four endpoint-animation characters and three four-character rope-orientation groups; importing artwork leaves native angle selection and endpoint cadence unchanged.</summary>
    public const string FileName = "grapple-tiles.png";
    /// <summary>Four endpoint characters followed by three groups of four segment characters.</summary>
    public const int Width = 128;
    /// <summary>Sheet height in pixels: one row of sixteen 8-by-8 four-bit planar characters, totaling 512 encoded bytes.</summary>
    public const int Height = 8;
    /// <summary>OBJ character $20, where `$9B:BFA5` uploads the endpoint character; the four rope characters follow it.</summary>
    public const int FirstCharacter = 0x20;
    /// <summary>Installed tile identity for horizontal rope segments.</summary>
    private const VramAssetId Horizontal = VramAssetId.GrappleHorizontalSegmentTiles;
    /// <summary>Installed tile identity for diagonal rope segments.</summary>
    private const VramAssetId Diagonal = VramAssetId.GrappleDiagonalSegmentTiles;
    /// <summary>Installed tile identity for vertical rope segments.</summary>
    private const VramAssetId Vertical = VramAssetId.GrappleVerticalSegmentTiles;
    /// <summary>$9A:8200, Tiles_GrappleBeam_Horizontal_Beam, the first endpoint character.</summary>
    private const int FirstPointSource = 0x9a8200;
    /// <summary>Endpoint animation characters occupy $200-byte source strides.</summary>
    private const int PointSourceStride = 0x200;
    /// <summary>Horizontal, diagonal and vertical beam groups occupy $800-byte source strides.</summary>
    private const int SegmentSourceStride = 0x800;
    /// <summary>Calculates the seven endpoint and rope upload descriptors in native order.</summary>
    private static readonly TransferList transfers = new();
    /// <summary>$9A:8200/8A00/9200 Tiles_GrappleBeam groups; only endpoint and segment-owned characters are extracted.</summary>
    public static IReadOnlyList<GrappleTileTransfer> Transfers => transfers;

    /// <summary>
    /// The rope character set the grapple's angle selects: `$9B:C005` folds the unsigned angle into an even table byte
    /// offset, equivalent to angle / 1024.
    /// </summary>
    public static GrappleRopeOrientation OrientationFor(SnesAngle angle)
    {
        // The 64 sectors repeat after half a turn. Reflect each half-turn about
        // its midpoint, retaining the native three/eight/five sector widths.
        int halfTurnSector = (angle.RawValue >> 10) & 31;
        int distance = Math.Min(halfTurnSector, 31 - halfTurnSector);
        return distance < 3 ? GrappleRopeOrientation.Vertical : distance < 11 ? GrappleRopeOrientation.Diagonal : GrappleRopeOrientation.Horizontal;
    }

    /// <summary>The installed rope characters of an orientation.</summary>
    public static VramAssetId SegmentAssetFor(GrappleRopeOrientation orientation) => orientation switch
    {
        GrappleRopeOrientation.Horizontal => Horizontal,
        GrappleRopeOrientation.Diagonal => Diagonal,
        GrappleRopeOrientation.Vertical => Vertical,
        _ => throw new ArgumentOutOfRangeException(nameof(orientation), orientation, "Undefined rope orientation."),
    };
    /// <summary>$9B:BFBD cycles endpoint characters at $8200/$8400/$8600/$8800, independently of rope angle.</summary>
    public static VramAssetId PointAssetFor(int frame) => frame switch
    {
        0 => VramAssetId.GrapplePointFirstTiles,
        1 => VramAssetId.GrapplePointSecondTiles,
        2 => VramAssetId.GrapplePointThirdTiles,
        3 => VramAssetId.GrapplePointFourthTiles,
        _ => throw new ArgumentOutOfRangeException(nameof(frame)),
    };
    /// <summary>Resolves one of seven Grapple-owned upload identities to its native source and planar-sheet slice; this lookup neither advances endpoint animation nor enqueues a VRAM transfer.</summary>
    /// <param name="asset">One of the four endpoint-frame identities or the horizontal, diagonal, or vertical segment identity.</param>
    /// <returns>A 32-byte endpoint or 128-byte segment descriptor; destination VRAM words are separately defined by <c>PointDestination</c> and <c>SegmentDestination</c>.</returns>
    /// <exception cref="InvalidDataException">The asset identity is not owned by the Grapple tile catalog.</exception>
    public static GrappleTileTransfer TransferFor(VramAssetId asset)
    {
        int index = asset switch
        {
            VramAssetId.GrapplePointFirstTiles => 0,
            VramAssetId.GrapplePointSecondTiles => 1,
            VramAssetId.GrapplePointThirdTiles => 2,
            VramAssetId.GrapplePointFourthTiles => 3,
            Horizontal => 4,
            Diagonal => 5,
            Vertical => 6,
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
                VramAssetId.ProjectileIceWaveTrailTiles or
                VramAssetId.ProjectileMissileTrailTiles or
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
                VramAssetId.HudTopRowTilemap => throw new InvalidDataException($"Not a Grapple tile asset: {asset}."),
            _ => throw new InvalidDataException($"Not a Grapple tile asset: {asset}."),
        };
        return transfers[index];
    }

    /// <summary>Indexed transfer view over four endpoint frames and three rope orientations.</summary>
    private sealed class TransferList : IReadOnlyList<GrappleTileTransfer>
    {
        /// <summary>Four endpoint transfers plus three four-tile segment transfers.</summary>
        public int Count => 7;
        /// <summary>Calculates source address, atlas offset, and byte count for one native upload.</summary>
        public GrappleTileTransfer this[int index]
        {
            get
            {
                if ((uint)index >= Count)
                    throw new ArgumentOutOfRangeException(nameof(index));
                if (index < 4)
                    return new(PointAssetFor(index),
                        FirstPointSource + PointSourceStride * index, 32 * index, 32);
                int orientation = index - 4;
                VramAssetId asset = orientation switch
                {
                    0 => Horizontal,
                    1 => Diagonal,
                    _ => Vertical,
                };
                return new(asset, FirstPointSource + 32 + SegmentSourceStride * orientation,
                    128 * (orientation + 1), 128);
            }
        }
        /// <summary>Yields the native endpoint transfers before horizontal, diagonal, and vertical segments.</summary>
        public IEnumerator<GrappleTileTransfer> GetEnumerator()
        {
            for (int index = 0; index < Count; index++)
                yield return this[index];
        }
        /// <summary>Exposes the same transfer order through the non-generic collection interface.</summary>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
