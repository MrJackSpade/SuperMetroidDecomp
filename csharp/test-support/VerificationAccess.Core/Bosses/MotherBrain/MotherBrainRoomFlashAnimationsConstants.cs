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
    internal static ShowingAnimationPlayer<MotherBrainRoomFlashSegment, MotherBrainRoomFlashImage, NoAnimationCue> ShowingFor(MotherBrainRoomFlashImage image, int updates)
    {
        ShowingAnimationPlayer<MotherBrainRoomFlashSegment, MotherBrainRoomFlashImage, NoAnimationCue> flash = MotherBrainRoomFlashAnimations.Starting();
        flash.ShowFrom(MotherBrainRoomFlashAnimations.Flash, new AnimationPlace(0, image.Step));
        flash.HoldShownFrameFor(updates);
        return flash;
    }
}
