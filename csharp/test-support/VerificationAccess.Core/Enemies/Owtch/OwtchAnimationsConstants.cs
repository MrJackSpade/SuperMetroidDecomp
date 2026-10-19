using SuperMetroid.Core.Programs;
using K = SuperMetroid.Core.Enemies.Owtch.OwtchCueKind;
using P = SuperMetroid.Core.Enemies.Owtch.OwtchProgram;
using SuperMetroid.Core.Enemies.Owtch.Artwork;
using SuperMetroid.Core.Fx;

namespace SuperMetroid.Core.Enemies.Owtch;

/// <summary>Cartridge values and helpers of <see cref="OwtchAnimations"/> that only verification reads.</summary>
internal static class OwtchAnimationsConstants
{
    /// <summary>The spritemaps <paramref name="programs"/> show, in authored order with repeats.</summary>
    internal static IEnumerable<ushort> Spritemaps(params OwtchProgram[] programs) =>
        programs.SelectMany(program => OwtchAnimations.Programs.Steps(program))
            .Where(step => step.Kind == AnimationProgramStepKind.Show)
            .Select(step => OwtchFrameToolingDefinitions.Spritemap(step.Frame));
}
