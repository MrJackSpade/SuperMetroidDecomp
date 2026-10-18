using P = SuperMetroid.Core.Enemies.Fireflea.FirefleaProgram;
using SuperMetroid.Core.Enemies.Fireflea.Artwork;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Enemies.Fireflea;

/// <summary>Cartridge values and helpers of <see cref="FirefleaAnimations"/> that only verification reads.</summary>
internal static class FirefleaAnimationsConstants
{
    /// <summary>The spritemaps <paramref name="programs"/> show, in authored order with repeats.</summary>
    internal static IEnumerable<ushort> Spritemaps(params FirefleaProgram[] programs) =>
        programs.SelectMany(program => FirefleaAnimations.Programs.Steps(program))
            .Where(step => step.Kind == AnimationProgramStepKind.Show)
            .Select(step => FirefleaFrameToolingDefinitions.Spritemap(step.Frame));
}
