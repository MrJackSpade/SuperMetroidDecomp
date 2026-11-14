using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Bosses.Torizo.Artwork;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>Cartridge values and helpers of <see cref="TorizoAnimations"/> that only verification reads.</summary>
internal static class TorizoAnimationsConstants
{
    /// <summary>
    /// The extended spritemaps <paramref name="animations"/> show, in step order with repeats: the artwork schema's
    /// first-seen frame order.
    /// </summary>
    internal static IEnumerable<ushort> Spritemaps(params TorizoAnimation[] animations) =>
        animations.SelectMany(animation => TorizoAnimations.Segments.Beats(animation))
            .OfType<AnimationBeat<EnemyFrameId, TorizoCue>.Frame>()
            .Select(frame => TorizoFrameDefinitionsConstants.Spritemap(frame.Shown));

    /// <summary>Every body animation.</summary>
    internal static TorizoAnimation[] AllAnimations { get; } = Enum.GetValues<TorizoAnimation>();
}
