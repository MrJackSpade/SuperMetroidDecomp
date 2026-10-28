using SuperMetroid.AssetExtraction.Samus;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>Verification access: the rope characters an angle selects, as the cartridge's sector table names them.</summary>
internal static class GrappleTileDefinitionsAccess
{
    /// <summary>Selects rope characters by angle.</summary>
    extension(GrappleTileSelection)
    {
        /// <summary>The installed rope characters the grapple's angle selects.</summary>
        internal static InstalledArtworkId SegmentAssetFor(RotationAngle angle) =>
            GrappleTileSelection.SegmentAssetFor(GrappleRopeOrientations.For(angle));
    }
}
