using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Samus;

namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>Verification access: the rope characters an angle selects, as the cartridge's sector table names them.</summary>
internal static class GrappleTileDefinitionsAccess
{
    /// <summary>Selects rope characters by angle.</summary>
    extension(GrappleTileSelection)
    {
        /// <summary>The transfer of the installed rope characters the grapple's angle selects.</summary>
        internal static GrappleTileTransfer SegmentTransferFor(RotationAngle angle) =>
            GrappleTileSelection.SegmentTransferFor(GrappleRopeOrientations.For(angle));
    }
}
