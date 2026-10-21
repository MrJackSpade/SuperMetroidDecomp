using SuperMetroid.Core.Programs;
using P = SuperMetroid.Core.Enemies.SpacePirates.WallSpacePirateProgram;
using SuperMetroid.Core.Enemies.SpacePirates.Artwork;

namespace SuperMetroid.Core.Enemies.SpacePirates;

/// <summary>Cartridge values and helpers of <see cref="WallSpacePirateAnimations"/> that only verification reads.</summary>
internal static class WallSpacePirateAnimationsConstants
{
    /// <summary>The spritemaps <paramref name="programs"/> show, in authored order with repeats.</summary>
    internal static IEnumerable<ushort> Spritemaps(params WallSpacePirateProgram[] programs) =>
        programs.SelectMany(program => WallSpacePirateAnimations.Programs.Steps(program))
            .OfType<AnimationProgramStep<P, WallSpacePirateFrame, WallSpacePirateCue>.ShowStep>()
            .Select(step => WallSpacePirateFrameToolingDefinitions.Spritemap(step.Frame));
}
