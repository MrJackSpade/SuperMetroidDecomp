using SuperMetroid.Core.Enemies.Shutters.Artwork;
using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Enemies.Shutters;

/// <summary>Cartridge values and helpers of <see cref="ShutterAnimations"/> that only verification reads.</summary>
internal static class ShutterAnimationsConstants
{
    /// <summary>The spritemaps <paramref name="programs"/> show, in authored order with repeats.</summary>
    internal static IEnumerable<ushort> Spritemaps(params ShutterProgram[] programs) =>
        programs.SelectMany(program => ShutterAnimations.Programs.Steps(program))
            .OfType<AnimationProgramStep<ShutterProgram, ShutterFrame, NoAnimationCue>.ShowStep>()
            .Select(step => ShutterFrameToolingDefinitions.Spritemap(step.Frame));
}
