using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.WorkRobots.Artwork;

namespace SuperMetroid.Core.Enemies.WorkRobots;

/// <summary>Cartridge values and helpers of <see cref="WorkRobotAnimations"/> that only verification reads.</summary>
internal static class WorkRobotAnimationsConstants
{
    /// <summary>The spritemaps <paramref name="programs"/> show, in authored order with repeats.</summary>
    internal static IEnumerable<ushort> Spritemaps(params WorkRobotProgram[] programs) =>
        programs.SelectMany(program => WorkRobotAnimations.Programs.Steps(program))
            .OfType<AnimationProgramStep<WorkRobotProgram, WorkRobotFrame, WorkRobotCue>.ShowStep>()
            .Select(step => WorkRobotFrameToolingDefinitions.Spritemap(step.Frame));
}
