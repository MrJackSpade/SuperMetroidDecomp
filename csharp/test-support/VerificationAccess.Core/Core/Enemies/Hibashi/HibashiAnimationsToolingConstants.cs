using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Hibashi;

/// <summary>Cartridge values and helpers of <see cref="HibashiAnimationsTooling"/> that only verification reads.</summary>
internal static class HibashiAnimationsToolingConstants
{
    /// <summary>The eruption steps the flame's rising cues carry, in the order they apply them (#2337).</summary>
    internal static HibashiEruptionStep[] EruptionSteps() =>
        [.. HibashiAnimations.Segments.Beats(HibashiSegment.Flame)
            .OfType<AnimationBeat<EnemyFrameId, HibashiCue>.Event>()
            .Select(raised => raised.Raised)
            .OfType<HibashiCue.EruptionStep>()
            .Select(rising => rising.Step)];
}
