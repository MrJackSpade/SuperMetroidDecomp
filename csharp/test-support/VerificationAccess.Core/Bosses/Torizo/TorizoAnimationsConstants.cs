using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Bosses.Torizo.Artwork;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>Cartridge values and helpers of <see cref="TorizoAnimations"/> that only verification reads.</summary>
internal static class TorizoAnimationsConstants
{
    /// <summary>
    /// The extended spritemaps <paramref name="segments"/> show, in step order with repeats: the artwork schema's
    /// first-seen frame order.
    /// </summary>
    internal static IEnumerable<ushort> Spritemaps(params TorizoBodySegment[] segments) =>
        segments.SelectMany(segment => TorizoAnimations.Segments.Beats(segment))
            .OfType<AnimationBeat<EnemyFrameId, TorizoCue>.Frame>()
            .Select(frame => TorizoFrameDefinitionsConstants.Spritemap(frame.Shown));

    /// <summary>Every body animation segment.</summary>
    internal static TorizoBodySegment[] AllSegments { get; } = Enum.GetValues<TorizoBodySegment>();
}
