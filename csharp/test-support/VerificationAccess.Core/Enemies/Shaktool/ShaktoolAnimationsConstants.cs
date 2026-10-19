using SuperMetroid.Core.Programs;
using K = SuperMetroid.Core.Enemies.Shaktool.ShaktoolCueKind;
using P = SuperMetroid.Core.Enemies.Shaktool.ShaktoolProgram;
using SuperMetroid.Core.Enemies.Shaktool.Artwork;
using SuperMetroid.Core.Fx;

namespace SuperMetroid.Core.Enemies.Shaktool;

/// <summary>Cartridge values and helpers of <see cref="ShaktoolAnimations"/> that only verification reads.</summary>
internal static class ShaktoolAnimationsConstants
{
    /// <summary>The spritemaps <paramref name="programs"/> show, in authored order with repeats.</summary>
    internal static IEnumerable<ushort> Spritemaps(params ShaktoolProgram[] programs) =>
        programs.SelectMany(program => ShaktoolAnimations.Programs.Steps(program))
            .Where(step => step.Kind == AnimationProgramStepKind.Show)
            .Select(step => ShaktoolFrameToolingDefinitions.Spritemap(step.Frame));
}
