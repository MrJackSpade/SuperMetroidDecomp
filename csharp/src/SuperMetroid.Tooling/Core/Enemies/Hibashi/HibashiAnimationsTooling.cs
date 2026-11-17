using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Hibashi.Artwork;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Hibashi;

/// <summary>Development-tool members of <see cref="HibashiAnimations"/>; never linked by player hosts.</summary>
internal static class HibashiAnimationsTooling
{
    /// <summary>The spritemaps <paramref name="programs"/> show, in authored order with repeats.</summary>
    internal static IEnumerable<ushort> Spritemaps(params HibashiSegment[] programs) =>
        programs.SelectMany(program => HibashiAnimations.Segments.Beats(program))
            .Select(beat => beat.ShownFrame).OfType<EnemyFrameId>()
            .Select(frame => HibashiFrameToolingDefinitions.Spritemap(frame));

    /// <summary>The eruption steps the flame's rising cues carry, in the order they apply them (#2337).</summary>
    internal static HibashiEruptionStep[] EruptionSteps() =>
        [.. HibashiAnimations.Segments.Beats(HibashiSegment.Flame)
            .OfType<AnimationBeat<EnemyFrameId, HibashiCue>.Event>()
            .Select(raised => raised.Raised)
            .OfType<HibashiCue.EruptionStep>()
            .Select(rising => rising.Step)];
}
