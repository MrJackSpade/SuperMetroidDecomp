using P = SuperMetroid.Core.Enemies.Shutters.ShutterProgram;
using SuperMetroid.Core.Enemies.Shutters.Artwork;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Enemies.Shutters;

/// <summary>Cartridge values and helpers of <see cref="ShutterAnimations"/> that only verification reads.</summary>
internal static class ShutterAnimationsConstants
{
    /// <summary>The spritemaps <paramref name="programs"/> show, in authored order with repeats.</summary>
    internal static IEnumerable<ushort> Spritemaps(params ShutterProgram[] programs) =>
        programs.SelectMany(program => ShutterAnimations.Programs.Steps(program))
            .Where(step => step.Kind == AnimationProgramStepKind.Show)
            .Select(step => ShutterFrameToolingDefinitions.Spritemap(step.Frame));
}
