using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.Core.Samus.Grapple;

/// <summary>Cartridge values of <see cref="SamusGrappleRomData.Rendering"/> that only verification reads.</summary>
internal static class SamusGrappleRomDataRenderingConstants
{
    /// <summary>Left-facing body offsets indexed by swing frame.</summary>
    public const int LeftPoseOffsetsByFrame = 0x9bc2c2;
    /// <summary>$9B:C342/C344: inclusive begin and exclusive end of the Grapple endpoint's strided character range.</summary>
    public const int PointTilePointers = 0x9bc342;
    /// <summary>Right-facing body offsets indexed by swing frame.</summary>
    public const int RightPoseOffsetsByFrame = 0x9bc302;
    /// <summary>Bytes uploaded for four Grapple segment characters.</summary>
    public const ushort SegmentTileByteCount = 0x80;
    /// <summary>Pointers to folded-angle rope-segment character data.</summary>
    public const int SegmentTilePointers = 0x9bc346;

    /// <summary>Provides grapple pose offsets and segment tile pointers for rendering comparisons.</summary>
    extension(SamusGrappleRomData.Rendering)
    {
        /// <inheritdoc cref="SamusGrappleRomDataRenderingConstants.LeftPoseOffsetsByFrame"/>
        internal static int LeftPoseOffsetsByFrame => SamusGrappleRomDataRenderingConstants.LeftPoseOffsetsByFrame;
        /// <inheritdoc cref="SamusGrappleRomDataRenderingConstants.PointTilePointers"/>
        internal static int PointTilePointers => SamusGrappleRomDataRenderingConstants.PointTilePointers;
        /// <inheritdoc cref="SamusGrappleRomDataRenderingConstants.RightPoseOffsetsByFrame"/>
        internal static int RightPoseOffsetsByFrame => SamusGrappleRomDataRenderingConstants.RightPoseOffsetsByFrame;
        /// <inheritdoc cref="SamusGrappleRomDataRenderingConstants.SegmentTileByteCount"/>
        internal static ushort SegmentTileByteCount => SamusGrappleRomDataRenderingConstants.SegmentTileByteCount;
        /// <inheritdoc cref="SamusGrappleRomDataRenderingConstants.SegmentTilePointers"/>
        internal static int SegmentTilePointers => SamusGrappleRomDataRenderingConstants.SegmentTilePointers;
    }
        /// <summary>Bytes uploaded for the single Grapple point character.</summary>
        public const ushort PointTileByteCount = 0x20;
}
