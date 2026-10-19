using P = SuperMetroid.Core.Enemies.Corpses.DeadTorizoProgram;
using SuperMetroid.Core.Enemies.Corpses.Artwork;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Enemies.Corpses;

/// <summary>Cartridge values and helpers of <see cref="DeadTorizoAnimations"/> that only verification reads.</summary>
internal static class DeadTorizoAnimationsConstants
{
    /// <summary>The spritemaps <paramref name="programs"/> show, in authored order with repeats.</summary>
    internal static IEnumerable<ushort> Spritemaps(params DeadTorizoProgram[] programs) =>
        programs.SelectMany(program => DeadTorizoAnimations.Programs.Steps(program))
            .Where(step => step.Kind == AnimationProgramStepKind.Show)
            .Select(step => DeadTorizoFrameToolingDefinitions.Spritemap(step.Frame));
}
