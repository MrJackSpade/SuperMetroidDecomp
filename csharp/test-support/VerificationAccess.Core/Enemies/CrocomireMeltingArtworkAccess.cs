using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.Core;
using SuperMetroid.Core.Bosses.Crocomire;
using SuperMetroid.Core.Bosses.Crocomire.Artwork;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Rooms.Artwork;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>Verification access to <see cref="CrocomireMeltingArtwork"/> members production does not use.</summary>
internal static class CrocomireMeltingArtworkAccess
{
    /// <summary>Exposes <see cref="CrocomireMeltingArtwork"/> state that only verification reads.</summary>
    extension(CrocomireMeltingArtwork self)
    {
        /// <summary>Canonical selected presentation data; no derived field is added to debugger states.</summary>
        public string ContentIdentity => SelectedPresentationHash.Create("enemy-crocomire-melt-v2", content =>
            {
                content.Append("first", self.first.Characters.Span);
                content.Append("second", self.second.Characters.Span);
                content.AppendTiles("first-map", self.firstTilemap);
                content.AppendTiles("second-map", self.secondTilemap);
            });
    }
}
