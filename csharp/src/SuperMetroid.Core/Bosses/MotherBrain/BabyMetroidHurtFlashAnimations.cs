using SuperMetroid.Core.Animation;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// The Baby Metroid's onion-ring hit flash (#2404) as a shade sequence played through the shared animation player: from
/// the update after a hit, it blinks (<see cref="Blinks"/>), the hurt-flash colours and then its own, each for
/// <see cref="LookUpdates"/>, and then
/// shows its own colours until the next hit.
/// </summary>
internal static partial class BabyMetroidHurtFlashAnimations
{
    /// <summary>How many times one hit makes it blink.</summary>
    private const int Blinks = 4;

    /// <summary>The updates each look of a blink shows for.</summary>
    private const int LookUpdates = 2;

    /// <summary>The flash's blink.</summary>
    internal static AnimationSegments<BabyMetroidHurtFlashSegment, BabyMetroidHurtLook, NoAnimationCue> Segments { get; } = new(
        new Dictionary<BabyMetroidHurtFlashSegment, AnimationBeat<BabyMetroidHurtLook, NoAnimationCue>[]>
        {
            [BabyMetroidHurtFlashSegment.Blink] =
            [
                new AnimationBeat<BabyMetroidHurtLook, NoAnimationCue>.Frame(BabyMetroidHurtLook.Flashing, LookUpdates),
                new AnimationBeat<BabyMetroidHurtLook, NoAnimationCue>.Frame(BabyMetroidHurtLook.OwnColours, LookUpdates),
            ],
        });

    /// <summary>One hit's flash: its blinks, and then it is over.</summary>
    internal static Animation<BabyMetroidHurtFlashSegment> Flash { get; } = new([Segments.Repeated(BabyMetroidHurtFlashSegment.Blink, Blinks)], AnimationEnd.Stops);

    /// <summary>A hit's flash, its first update showing the first blink's hurt-flash colours.</summary>
    internal static AnimationPlayer<BabyMetroidHurtFlashSegment, BabyMetroidHurtLook, NoAnimationCue> Starting() =>
        AnimationPlayer<BabyMetroidHurtFlashSegment, BabyMetroidHurtLook, NoAnimationCue>.Starting(Segments, Flash);
}
