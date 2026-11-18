using SuperMetroid.Core.Animation;

namespace SuperMetroid.Core.Fx;

/// <summary>The Fireflea room's flash (#2367): its looks, each shown for a flash's updates, which it loops.</summary>
internal static class FirefleaFlashAnimations
{
    /// <summary>Every run's beats.</summary>
    internal static AnimationSegments<FirefleaFlashSegment, FirefleaFlashShade, NoAnimationCue> Segments { get; } = new(
        new Dictionary<FirefleaFlashSegment, AnimationBeat<FirefleaFlashShade, NoAnimationCue>[]>
    {
        [FirefleaFlashSegment.Flashing] =
        [
            Show(0),
            Show(1),
            Show(2),
            Show(3),
            Show(4),
            Show(5),
            Show(6),
            Show(5),
            Show(4),
            Show(3),
            Show(2),
            Show(1),
        ],
    });

    /// <summary>The room's flash: its looks, over and over.</summary>
    internal static Animation<FirefleaFlashSegment> Flashing { get; } =
        Animation<FirefleaFlashSegment>.Looping([Segments.Whole(FirefleaFlashSegment.Flashing)]);

    /// <summary>Shows the look adding <paramref name="level"/> fade levels of grey for a flash's updates.</summary>
    private static AnimationBeat<FirefleaFlashShade, NoAnimationCue> Show(int level) =>
        new AnimationBeat<FirefleaFlashShade, NoAnimationCue>.Frame(new FirefleaFlashShade(level), FirefleaFxDefinitions.FlashDuration);
}
