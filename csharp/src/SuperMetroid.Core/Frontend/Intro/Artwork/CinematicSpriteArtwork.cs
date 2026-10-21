using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend.Intro.Artwork;

/// <summary>Drawing and content identity shared by the cinematic sprite presentations, whose compositions are keyed by pose.</summary>
internal static class CinematicSpriteArtwork
{
    /// <summary>
    /// Draws the installed composition <paramref name="pose"/> shows, choosing the native on-screen or off-screen origin
    /// clipping; <paramref name="owner"/> names the presentation in a rejection.
    /// </summary>
    /// <exception cref="InvalidDataException">No composition is installed for <paramref name="pose"/>.</exception>
    internal static void DrawInstalled(IReadOnlyDictionary<CinematicSpritePose, SpriteComposition> frames, string owner,
        CinematicSpritePose pose, ScreenSprites objects, Position16<OamPixels> origin, ushort paletteBits, bool originIsOnScreen)
    {
        if (!frames.TryGetValue(pose, out SpriteComposition? frame))
            throw new InvalidDataException($"{owner} frame for pose {pose.Index} is not installed.");
        if (originIsOnScreen)
            frame.Draw(objects, origin, paletteBits);
        else
            frame.DrawOffScreen(objects, origin, paletteBits);
    }

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
