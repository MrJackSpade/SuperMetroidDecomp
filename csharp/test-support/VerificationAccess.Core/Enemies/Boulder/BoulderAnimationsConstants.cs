using P = SuperMetroid.Core.Enemies.Boulder.BoulderProgram;
using SuperMetroid.Core.Enemies.Boulder.Artwork;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Enemies.Boulder;

/// <summary>Cartridge values and helpers of <see cref="BoulderAnimations"/> that only verification reads.</summary>
internal static class BoulderAnimationsConstants
{
    /// <summary>The spritemaps <paramref name="programs"/> show, in authored order with repeats.</summary>
    internal static IEnumerable<ushort> Spritemaps(params BoulderProgram[] programs) =>
        programs.SelectMany(program => BoulderAnimations.Programs.Steps(program))
            .Where(step => step.Kind == AnimationProgramStepKind.Show)
            .Select(step => BoulderFrameDefinitions.Spritemap(step.Frame));
}
