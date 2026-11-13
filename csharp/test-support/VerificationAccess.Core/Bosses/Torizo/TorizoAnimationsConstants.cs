using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Bosses.Torizo.Artwork;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>Cartridge values and helpers of <see cref="TorizoAnimations"/> that only verification reads.</summary>
internal static class TorizoAnimationsConstants
{
    /// <summary>
    /// The extended spritemaps <paramref name="programs"/> show, in authored order with repeats: the artwork schema's
    /// first-seen frame order.
    /// </summary>
    internal static IEnumerable<ushort> Spritemaps(params TorizoBodySegment[] programs) =>
        programs.SelectMany(program => TorizoAnimations.Segments.Beats(program))
            .OfType<AnimationBeat<EnemyFrameId, TorizoCue>.Frame>()
            .Select(frame => TorizoFrameDefinitionsConstants.Spritemap(frame.Shown));
    /// <summary>Every body program, in authored address order.</summary>
    internal static TorizoBodySegment[] AllPrograms { get; } = Enum.GetValues<TorizoBodySegment>();
}
