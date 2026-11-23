using SuperMetroid.Core.Animation;

namespace SuperMetroid.Core.Enemies.FaceBlocks;

/// <summary>
/// The Blue Brinstar face blocks' room-wide glow (#2477): its steps, each showing a level of the glow for
/// <see cref="FaceBlockGlowDefinitions.FrameDuration"/> updates, which the room loops once a block has started it. The glow
/// brightens through each level and dims back through them again. A room starts it on its last step, so its first step shows
/// once that step's updates have run.
/// </summary>
internal static partial class FaceBlockGlowAnimations
{
    /// <summary>Every run's beats.</summary>
    internal static AnimationSegments<FaceBlockGlowSegment, FaceBlockGlowLevel, NoAnimationCue> Segments { get; } = new(
        new Dictionary<FaceBlockGlowSegment, AnimationBeat<FaceBlockGlowLevel, NoAnimationCue>[]>
    {
        [FaceBlockGlowSegment.Glowing] =
        [
            Show(FaceBlockGlowLevel.Dimmest),
            Show(FaceBlockGlowLevel.Dim),
            Show(FaceBlockGlowLevel.Bright),
            Show(FaceBlockGlowLevel.Brightest),
            Show(FaceBlockGlowLevel.Brightest),
            Show(FaceBlockGlowLevel.Bright),
            Show(FaceBlockGlowLevel.Dim),
            Show(FaceBlockGlowLevel.Dimmest),
        ],
        [FaceBlockGlowSegment.Restarting] =
        [
            Show(FaceBlockGlowLevel.Dimmest),
        ],
    });

    /// <summary>The levels the glow's steps show, in the order it shows them, from its first step.</summary>
    internal static IReadOnlyList<FaceBlockGlowLevel> Cycle { get; } =
        Segments.FramesShownBy(FaceBlockGlowSegment.Glowing);

    /// <summary>
    /// Each level the glow shows, once, in the order its cycle first shows them: the import installs one shade for each,
    /// named by the level (<see cref="Artwork.FaceBlockShades.Glow"/>).
    /// </summary>
    internal static IReadOnlyList<FaceBlockGlowLevel> Levels { get; } = [.. Cycle.Distinct()];

    /// <summary>
    /// The glow as a room starts it: resting on its last step's level for that step's updates, then its steps from the first,
    /// over and over.
    /// </summary>
    internal static Animation<FaceBlockGlowSegment> Restarted { get; } = Animation<FaceBlockGlowSegment>.LeadingInto(
        [Segments.Whole(FaceBlockGlowSegment.Restarting)],
        [Segments.Whole(FaceBlockGlowSegment.Glowing)]);

    /// <summary>Shows <paramref name="level"/> for a step's updates.</summary>
    private static AnimationBeat<FaceBlockGlowLevel, NoAnimationCue> Show(FaceBlockGlowLevel level) =>
        new AnimationBeat<FaceBlockGlowLevel, NoAnimationCue>.Frame(level, FaceBlockGlowDefinitions.FrameDuration);
}
