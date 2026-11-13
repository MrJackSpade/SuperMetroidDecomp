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

    /// <summary>Whether the shooting stars cross the scene, drawn from its sheet.</summary>
    internal bool Stars { get; private set; }

    /// <summary>Records that the shooting stars cross the scene.</summary>
    internal void AddStars() => Stars = true;

    /// <summary>Records every pose <paramref name="program"/> shows, through every program it goes or falls into, as drawn in <paramref name="colouring"/>.</summary>
    internal void AddProgram(CinematicSpriteFamily family, CinematicSpriteSegment program, EndingSpriteColouring colouring)
    {
        foreach (CinematicSpritePose pose in CinematicSpriteAnimations.Segments.FramesOf(CinematicSpriteAnimations.StartingWith(program)))
            AddPose(family, pose, colouring);
    }

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
