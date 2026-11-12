using SuperMetroid.Core;
using SuperMetroid.Core.Areas.Ceres.Artwork;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Areas.Ceres;

/// <summary>Verification access to <see cref="CeresEscapeOverlays"/> members production does not use.</summary>
internal static class CeresEscapeOverlaysAccess
{
    /// <summary>Exposes <see cref="CeresEscapeOverlays"/> state that only verification reads.</summary>
    extension(CeresEscapeOverlays self)
    {
        /// <summary>Canonical selected presentation data; no derived field is added to debugger states.</summary>
        public string ContentIdentity => SelectedPresentationHash.Create("enemy-ceres-escape-overlay-v2", content =>
        {
            content.Append("shared", ReadOnlySpan<byte>.Empty);
            foreach (var page in CeresEscapeOverlayPages.All)
            {
                content.Append("source", Encoding.UTF8.GetBytes(page.Name));
                content.AppendTiles("tiles", self.PageTiles(page));
            }
        });
    }
}
