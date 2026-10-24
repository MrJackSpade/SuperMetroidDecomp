namespace SuperMetroid.Core.Samus.Grapple;

/// <summary>Cartridge values of <see cref="GrappleBeamDefinitions.Rendering"/> that only verification reads.</summary>
internal static class GrappleBeamDefinitionsRenderingConstants
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
    extension(GrappleBeamDefinitions.Rendering)
    {
        /// <inheritdoc cref="GrappleBeamDefinitionsRenderingConstants.LeftPoseOffsetsByFrame"/>
        internal static int LeftPoseOffsetsByFrame => GrappleBeamDefinitionsRenderingConstants.LeftPoseOffsetsByFrame;
        /// <inheritdoc cref="GrappleBeamDefinitionsRenderingConstants.PointTilePointers"/>
        internal static int PointTilePointers => GrappleBeamDefinitionsRenderingConstants.PointTilePointers;
        /// <inheritdoc cref="GrappleBeamDefinitionsRenderingConstants.RightPoseOffsetsByFrame"/>
        internal static int RightPoseOffsetsByFrame => GrappleBeamDefinitionsRenderingConstants.RightPoseOffsetsByFrame;
        /// <inheritdoc cref="GrappleBeamDefinitionsRenderingConstants.SegmentTileByteCount"/>
        internal static ushort SegmentTileByteCount => GrappleBeamDefinitionsRenderingConstants.SegmentTileByteCount;
        /// <inheritdoc cref="GrappleBeamDefinitionsRenderingConstants.SegmentTilePointers"/>
        internal static int SegmentTilePointers => GrappleBeamDefinitionsRenderingConstants.SegmentTilePointers;
    }
        /// <summary>Bytes uploaded for the single Grapple point character.</summary>
        public const ushort PointTileByteCount = 0x20;
}
