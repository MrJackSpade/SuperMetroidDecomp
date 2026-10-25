using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>Verification access: the rope characters an angle selects, as the cartridge's sector table names them.</summary>
internal static class GrappleTileDefinitionsAccess
{
    /// <summary>Selects rope characters by angle.</summary>
    extension(GrappleTileDefinitions)
    {
        /// <summary>The installed rope characters the grapple's angle selects.</summary>
        internal static VramAssetId SegmentAssetFor(RotationAngle angle) =>
            GrappleTileDefinitions.SegmentAssetFor(GrappleTileDefinitions.OrientationFor(angle));
    }
}
