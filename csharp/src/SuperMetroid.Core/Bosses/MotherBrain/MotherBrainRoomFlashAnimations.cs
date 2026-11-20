using SuperMetroid.Core.Animation;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// Mother Brain's fake-death room flash (#641, #2259) as a shade sequence played through the shared animation player
/// (#2404): its fourteen images, each shown <see cref="MotherBrainRoomFlashDefinitions.FlashImageTicks"/> updates, looping
/// for as long as the flash runs.
/// </summary>
internal static class MotherBrainRoomFlashAnimations
{
    /// <summary>The flash's run of images.</summary>
    internal static AnimationSegments<MotherBrainRoomFlashSegment, MotherBrainRoomFlashImage, NoAnimationCue> Segments { get; } = new(
        new Dictionary<MotherBrainRoomFlashSegment, AnimationBeat<MotherBrainRoomFlashImage, NoAnimationCue>[]>
        {
            [MotherBrainRoomFlashSegment.Flash] = [.. MotherBrainRoomFlashImage.All
                .Select(image => new AnimationBeat<MotherBrainRoomFlashImage, NoAnimationCue>.Frame(image, MotherBrainRoomFlashDefinitions.FlashImageTicks))],
        });

    /// <summary>The flash, looping its images for good.</summary>
    private static Animation<MotherBrainRoomFlashSegment> Flash { get; } = Animation<MotherBrainRoomFlashSegment>.Looping([Segments.Whole(MotherBrainRoomFlashSegment.Flash)]);

    /// <summary>
    /// The flash started on its first image for its two updates, the update that starts it counting as the first: her room
    /// shows the first image on the flash's first run alone, then each image for two runs.
    /// </summary>
    internal static AnimationPlayer<MotherBrainRoomFlashSegment, MotherBrainRoomFlashImage, NoAnimationCue> Starting() =>
        AnimationPlayer<MotherBrainRoomFlashSegment, MotherBrainRoomFlashImage, NoAnimationCue>.StartShowing(
            Segments, Flash, MotherBrainRoomFlashDefinitions.FlashImageTicks);
}
