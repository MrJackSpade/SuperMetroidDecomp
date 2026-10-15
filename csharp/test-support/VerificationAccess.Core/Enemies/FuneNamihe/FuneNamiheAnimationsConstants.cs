using SuperMetroid.Core.Programs;
using K = SuperMetroid.Core.Enemies.FuneNamihe.FuneNamiheCueKind;
using P = SuperMetroid.Core.Enemies.FuneNamihe.FuneNamiheProgram;
using SuperMetroid.Core.Enemies.FuneNamihe.Artwork;
using SuperMetroid.Core.Fx;

namespace SuperMetroid.Core.Enemies.FuneNamihe;

/// <summary>Cartridge values and helpers of <see cref="FuneNamiheAnimations"/> that only verification reads.</summary>
internal static class FuneNamiheAnimationsConstants
{
    /// <summary>The spritemaps <paramref name="programs"/> show, in authored order with repeats.</summary>
    internal static IEnumerable<ushort> Spritemaps(params FuneNamiheProgram[] programs) =>
        programs.SelectMany(program => FuneNamiheAnimations.Programs.Steps(program))
            .Where(step => step.Kind == AnimationProgramStepKind.Show)
            .Select(step => FuneNamiheFrameDefinitions.Spritemap(step.Frame));
}
