using SuperMetroid.Core.Enemies.WorkRobots.Artwork;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.WorkRobots;

/// <summary>Cartridge values and helpers of <see cref="WorkRobotAnimations"/> that only verification reads.</summary>
internal static class WorkRobotAnimationsConstants
{
    /// <summary>The spritemaps <paramref name="programs"/> show, in authored order with repeats.</summary>
    internal static IEnumerable<ushort> Spritemaps(params WorkRobotSegment[] programs) =>
        programs.SelectMany(program => WorkRobotAnimations.Segments.Beats(program))
            .Select(beat => beat.ShownFrame).OfType<EnemyFrameId>()
            .Select(frame => WorkRobotFrameToolingDefinitions.Spritemap(frame));
}
