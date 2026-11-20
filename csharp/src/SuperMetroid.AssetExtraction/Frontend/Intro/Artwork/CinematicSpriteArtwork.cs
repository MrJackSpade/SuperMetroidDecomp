using SuperMetroid.AssetExtraction.Frontend.Cinematics;
using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Intro.Artwork;

/// <summary>Content identity shared by the cinematic sprite presentations, whose compositions are keyed by pose.</summary>
internal static class CinematicSpriteArtwork
{
    /// <summary>The content identity of the family's compositions, hashed in a fixed pose order; each composition's ordered parts are part of it.</summary>
    internal static string ContentIdentity(string domain, IReadOnlyDictionary<CinematicSpritePose, SpriteComposition> frames) =>
        SelectedPresentationHash.Create(domain, content =>
        {
            foreach ((CinematicSpritePose pose, SpriteComposition frame) in frames.OrderBy(pair => CinematicSpritemapSources.CartridgeOrder(pair.Key)))
            {
                content.Append("frame", CinematicSpritemapSources.CartridgeOrder(pose));
                frame.AppendIdentity(content);
            }
        });
}
