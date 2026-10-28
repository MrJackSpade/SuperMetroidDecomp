using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Intro;

namespace SuperMetroid.AssetExtraction.Frontend.Intro.Artwork;

/// <summary>Content identity shared by the cinematic sprite presentations, whose compositions are keyed by pose.</summary>
internal static class CinematicSpriteArtwork
{
    /// <summary>Frame lookup order is irrelevant; ordered parts remain significant for OAM drawing.</summary>
    internal static string ContentIdentity(string domain, IReadOnlyDictionary<CinematicSpritePose, SpriteComposition> frames) =>
        SelectedPresentationHash.Create(domain, content =>
        {
            foreach ((CinematicSpritePose pose, SpriteComposition frame) in frames.OrderBy(pair => pair.Key.Index))
            {
                content.Append("frame", pose.Index);
                frame.AppendIdentity(content);
            }
        });
}
