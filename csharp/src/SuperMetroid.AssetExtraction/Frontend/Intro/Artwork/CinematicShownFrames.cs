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
internal sealed record CinematicShownFrames(CinematicSpriteFamily Family, IReadOnlyDictionary<CinematicSpritePose, SpriteComposition> Frames)
    : ICinematicSpriteFrames
{
    /// <summary>The poses <paramref name="programs"/> show, through every program they go, fall or repeat into; the blank pose draws nothing.</summary>
    internal static HashSet<CinematicSpritePose> PosesOf(IEnumerable<CinematicSpriteSegment> programs)
    {
        var shown = new HashSet<CinematicSpritePose>();
        foreach (CinematicSpriteSegment program in programs)
        {
            foreach (CinematicSpritePose pose in CinematicSpriteAnimations.Segments.FramesOf(CinematicSpriteAnimations.StartingWith(program)))
            {
                if (pose != CinematicSpritePose.Blank)
                    shown.Add(pose);
            }
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

    /// <summary>Requires every pose of <paramref name="shown"/> composed by one of <paramref name="families"/>.</summary>
    /// <exception cref="InvalidDataException">A shown pose has no composition.</exception>
    internal static void RequireComposed(string scene, IReadOnlySet<CinematicSpritePose> shown, IEnumerable<ICinematicSpriteFrames> families)
    {
        var composed = new HashSet<CinematicSpritePose>();
        foreach (ICinematicSpriteFrames family in families)
            composed.UnionWith(family.Frames.Keys);
        foreach (CinematicSpritePose pose in shown)
        {
            if (!composed.Contains(pose))
                throw new InvalidDataException($"{scene} shows pose {pose}, which none of its families composes.");
        }
    }
}
