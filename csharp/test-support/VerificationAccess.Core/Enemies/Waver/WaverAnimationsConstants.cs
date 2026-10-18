using SuperMetroid.Core.Programs;
using K = SuperMetroid.Core.Enemies.Waver.WaverCueKind;
using P = SuperMetroid.Core.Enemies.Waver.WaverProgram;
using SuperMetroid.Core.Enemies.Waver.Artwork;
using SuperMetroid.Core.Fx;

namespace SuperMetroid.Core.Enemies.Waver;

/// <summary>Cartridge values and helpers of <see cref="WaverAnimations"/> that only verification reads.</summary>
internal static class WaverAnimationsConstants
{
    /// <summary>The spritemaps <paramref name="programs"/> show, in authored order with repeats.</summary>
    internal static IEnumerable<ushort> Spritemaps(params WaverProgram[] programs) =>
        programs.SelectMany(program => WaverAnimations.Programs.Steps(program))
            .Where(step => step.Kind == AnimationProgramStepKind.Show)
            .Select(step => WaverFrameToolingDefinitions.Spritemap(step.Frame));
}
