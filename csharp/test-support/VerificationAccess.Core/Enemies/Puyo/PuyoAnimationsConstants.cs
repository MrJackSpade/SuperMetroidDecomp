using SuperMetroid.Core.Enemies.Puyo.Artwork;
using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Enemies.Puyo;

/// <summary>Cartridge values and helpers of <see cref="PuyoAnimations"/> that only verification reads.</summary>
internal static class PuyoAnimationsConstants
{
    /// <summary>The spritemaps <paramref name="programs"/> show, in authored order with repeats.</summary>
    internal static IEnumerable<ushort> Spritemaps(params PuyoProgram[] programs) =>
        programs.SelectMany(program => PuyoAnimations.Programs.Steps(program))
            .OfType<AnimationProgramStep<PuyoProgram, PuyoFrame, NoAnimationCue>.ShowStep>()
            .Select(step => PuyoFrameToolingDefinitions.Spritemap(step.Frame));
}
