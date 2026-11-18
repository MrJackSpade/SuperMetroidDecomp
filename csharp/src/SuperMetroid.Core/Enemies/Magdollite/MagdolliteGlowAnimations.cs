using SuperMetroid.Core.Animation;

namespace SuperMetroid.Core.Enemies.Magdollite;

/// <summary>
/// The room's lava glow (#2343, #2439): every frame of its cycle in order, each shown for
/// <see cref="MagdolliteMotionDefinitions.GlowFrameDuration"/> updates, which the room loops once a Magdollite has started
/// it. Every Magdollite and the lava it throws show the glow's frame.
/// </summary>
internal static class MagdolliteGlowAnimations
{
    /// <summary>
    /// The glow's frames in the order it shows them, from its first (#2474): the installed glow colours hold one row for
    /// each in this order, and the import installs one shade for each, named by the frame.
    /// </summary>
    internal static IReadOnlyList<MagdolliteGlow> Cycle { get; } =
    [
        MagdolliteGlow.OnBrightestColour,
        MagdolliteGlow.OnDarkestColour,
        MagdolliteGlow.OnSecondDarkestColour,
        MagdolliteGlow.OnSecondBrightestColour,
    ];

    /// <summary>Every run's beats.</summary>
    internal static AnimationSegments<MagdolliteGlowSegment, MagdolliteGlow, NoAnimationCue> Segments { get; } = new(
        new Dictionary<MagdolliteGlowSegment, AnimationBeat<MagdolliteGlow, NoAnimationCue>[]>
    {
        [MagdolliteGlowSegment.Glowing] = [.. Cycle.Select(Show)],
    });

    /// <summary>The glow as a part's placement restarts it: its first frame at once, then each in turn, over and over.</summary>
    internal static Animation<MagdolliteGlowSegment> Glowing { get; } =
        Animation<MagdolliteGlowSegment>.Looping([Segments.Whole(MagdolliteGlowSegment.Glowing)]);

    /// <summary>Shows <paramref name="frame"/> for a glow frame's updates.</summary>
    private static AnimationBeat<MagdolliteGlow, NoAnimationCue> Show(MagdolliteGlow frame) =>
        new AnimationBeat<MagdolliteGlow, NoAnimationCue>.Frame(frame, MagdolliteMotionDefinitions.GlowFrameDuration);
}
