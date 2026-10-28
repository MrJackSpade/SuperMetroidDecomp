using SuperMetroid.AssetExtraction.Areas.Ceres;

namespace SuperMetroid.Core.Frontend.CeresCinematics.Artwork;

/// <summary>Verification access to the characters a <see cref="CeresFlightArtwork"/> was read with (#1409).</summary>
internal static class CeresFlightArtworkCatalogAccess
{
    /// <summary>Reads the installed flight artwork's characters.</summary>
    extension(CeresFlightArtwork self)
    {
        /// <summary>The chunky 8-bpp Mode 7 floor characters.</summary>
        internal ReadOnlyMemory<byte> FloorCharacters => CeresFlightCharacters.Of(self).FloorCharacters;

        /// <summary>The planar 4-bpp OBJ characters.</summary>
        internal ReadOnlyMemory<byte> ObjectCharacters => CeresFlightCharacters.Of(self).ObjectCharacters;
    }
}
