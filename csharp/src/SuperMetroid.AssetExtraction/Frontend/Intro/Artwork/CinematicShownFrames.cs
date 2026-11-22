using SuperMetroid.Core;
using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.Core.Frontend.Intro.Artwork;
using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Intro.Artwork;

/// <summary>
/// A cinematic family's compositions of only the poses one scene's actors show (#1513), so the scene's sheet holds no
/// frame its actors never draw.
/// </summary>
/// <param name="Family">The family the compositions belong to.</param>
/// <param name="Frames">Each shown composition, by the pose that shows it.</param>
internal sealed partial record CinematicShownFrames(CinematicSpriteFamily Family, IReadOnlyDictionary<CinematicSpritePose, SpriteComposition> Frames)
    : ICinematicSpriteFrames
{
    /// <summary>
    /// The poses <paramref name="programs"/> of <paramref name="animations"/> show, through every segment they play; the
    /// blank pose draws nothing.
    /// </summary>
    internal static HashSet<CinematicSpritePose> PosesOf<TAnimation, TCue>(CinematicAnimations<TAnimation, CinematicSpritePose, TCue> animations,
        IEnumerable<TAnimation> programs)
        where TAnimation : struct, Enum
        where TCue : notnull
    {
        Ensure.NotNull(animations);
        var shown = new HashSet<CinematicSpritePose>();
        foreach (TAnimation program in Ensure.NotNull(programs))
        {
            foreach (CinematicSpritePose pose in animations.Segments.FramesOf(animations.StartingWith(program)))
                shown.Add(pose);
        }
        return shown;
    }

    /// <summary><paramref name="family"/>'s compositions of the poses <paramref name="shown"/> holds that <paramref name="drawsFromFamily"/> draws from it.</summary>
    internal static CinematicShownFrames Of(ICinematicSpriteFrames family, IReadOnlySet<CinematicSpritePose> shown,
        Func<CinematicSpritePose, bool> drawsFromFamily)
    {
        Ensure.NotNull(family);
        Ensure.NotNull(shown);
        Ensure.NotNull(drawsFromFamily);
        var frames = new Dictionary<CinematicSpritePose, SpriteComposition>();
        foreach ((CinematicSpritePose pose, SpriteComposition composition) in family.Frames)
        {
            if (shown.Contains(pose) && drawsFromFamily(pose))
                frames.Add(pose, composition);
        }
        return new CinematicShownFrames(family.Family, frames);
    }
}
