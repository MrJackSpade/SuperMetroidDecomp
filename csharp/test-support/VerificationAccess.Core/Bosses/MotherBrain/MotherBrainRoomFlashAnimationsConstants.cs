using SuperMetroid.Core.Animation;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Cartridge values and helpers of <see cref="MotherBrainRoomFlashAnimations"/> that only verification reads.</summary>
internal static class MotherBrainRoomFlashAnimationsConstants
{
    /// <summary>
    /// The flash standing on <paramref name="image"/> with <paramref name="updates"/> more updates of it to show, as a
    /// recording resumes it partway.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="updates"/> is below one.</exception>
    internal static AnimationPlayer<MotherBrainRoomFlashSegment, MotherBrainRoomFlashImage, NoAnimationCue> ShowingFor(MotherBrainRoomFlashImage image, int updates)
    {
        AnimationPlayer<MotherBrainRoomFlashSegment, MotherBrainRoomFlashImage, NoAnimationCue> flash = MotherBrainRoomFlashAnimations.Starting();
        flash.ShowAt(MotherBrainRoomFlashAnimations.Flash, new AnimationPlace(0, image.Step), updates);
        return flash;
    }
}
