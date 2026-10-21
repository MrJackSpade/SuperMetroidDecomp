using SuperMetroid.Core.Programs;
using P = SuperMetroid.Core.Enemies.SpacePirates.NinjaSpacePirateProgram;
using SuperMetroid.Core.Enemies.SpacePirates.Artwork;

namespace SuperMetroid.Core.Enemies.SpacePirates;

/// <summary>Cartridge values and helpers of <see cref="NinjaSpacePirateAnimations"/> that only verification reads.</summary>
internal static class NinjaSpacePirateAnimationsConstants
{
    /// <summary>The spritemaps <paramref name="programs"/> show, in authored order with repeats.</summary>
    internal static IEnumerable<ushort> Spritemaps(params NinjaSpacePirateProgram[] programs) =>
        programs.SelectMany(program => NinjaSpacePirateAnimations.Programs.Steps(program))
            .OfType<AnimationProgramStep<P, NinjaSpacePirateFrame, NinjaSpacePirateCue>.ShowStep>()
            .Select(step => NinjaSpacePirateFrameToolingDefinitions.Spritemap(step.Frame));
}
