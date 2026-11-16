using SuperMetroid.Core;
using SuperMetroid.Core.Frontend.Ending;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core.Frontend.Ending.Artwork;
using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.Core.Frontend.Intro.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Ending;

/// <summary>
/// The poses an ending scene's actors draw from one sheet, each with the colourings they draw it in, first drawn first
/// (#1513), and whether the shooting stars cross the scene. The sheet pictures exactly these poses, each once in its first
/// colouring.
/// </summary>
internal sealed class EndingSheetDraws
{
    /// <summary>Each family's drawn poses, with their colourings in the order first drawn.</summary>
    private readonly Dictionary<CinematicSpriteFamily, Dictionary<CinematicSpritePose, List<EndingSpriteColouring>>> families = [];

    /// <summary>The texts the scene types, with their colourings in the order first drawn (#2195).</summary>
    private readonly Dictionary<EndingTypedText, List<EndingSpriteColouring>> texts = [];

    /// <summary>Whether the shooting stars cross the scene, drawn from its sheet.</summary>
    internal bool Stars { get; private set; }

    /// <summary>Records that the shooting stars cross the scene.</summary>
    internal void AddStars() => Stars = true;

    /// <summary>
    /// Records every pose <paramref name="program"/> of <paramref name="animations"/> shows, through every segment it plays,
    /// as drawn in <paramref name="colouring"/>.
    /// </summary>
    internal void AddProgram<TAnimation, TCue>(CinematicSpriteFamily family, CinematicAnimations<TAnimation, CinematicSpritePose, TCue> animations,
        TAnimation program, EndingSpriteColouring colouring)
        where TAnimation : struct, Enum
        where TCue : notnull
    {
        Ensure.NotNull(animations);
        foreach (CinematicSpritePose pose in animations.Segments.FramesOf(animations.StartingWith(program)))
            AddPose(family, pose, colouring);
    }

    /// <summary>Records that the scene types <paramref name="text"/> in <paramref name="colouring"/> (#2195).</summary>
    internal void AddText(EndingTypedText text, EndingSpriteColouring colouring)
    {
        if (!texts.TryGetValue(text, out List<EndingSpriteColouring>? colourings))
        {
            colourings = [];
            texts.Add(text, colourings);
        }
        if (!colourings.Contains(colouring))
            colourings.Add(colouring);
    }

    /// <summary>The texts the scene types, each with its colourings in the order first drawn, in the order first typed.</summary>
    internal IReadOnlyDictionary<EndingTypedText, List<EndingSpriteColouring>> Texts => texts;

    /// <summary>Records <paramref name="pose"/> as drawn in <paramref name="colouring"/>; the blank pose draws nothing.</summary>
    internal void AddPose(CinematicSpriteFamily family, CinematicSpritePose pose, EndingSpriteColouring colouring)
    {
        if (pose == CinematicSpritePose.Blank)
            return;
        if (!families.TryGetValue(family, out Dictionary<CinematicSpritePose, List<EndingSpriteColouring>>? poses))
        {
            poses = [];
            families.Add(family, poses);
        }
        if (!poses.TryGetValue(pose, out List<EndingSpriteColouring>? colourings))
        {
            colourings = [];
            poses.Add(pose, colourings);
        }
        if (!colourings.Contains(colouring))
            colourings.Add(colouring);
    }

    /// <summary>The poses the scene draws from <paramref name="family"/>, with their colourings in the order first drawn.</summary>
    /// <exception cref="InvalidDataException">The scene draws nothing from the family.</exception>
    internal Dictionary<CinematicSpritePose, List<EndingSpriteColouring>> Poses(CinematicSpriteFamily family) =>
        families.TryGetValue(family, out Dictionary<CinematicSpritePose, List<EndingSpriteColouring>>? poses)
            ? poses
            : throw new InvalidDataException($"The ending scene draws nothing from the {family} family.");
}
