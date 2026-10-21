using P = SuperMetroid.Core.Enemies.Gunship.GunshipProgram;
using SuperMetroid.Core.Enemies.Gunship.Artwork;
using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Enemies.Gunship;

/// <summary>Cartridge values and helpers of <see cref="GunshipAnimations"/> that only verification reads.</summary>
internal static class GunshipAnimationsConstants
{
    /// <summary>The spritemaps <paramref name="programs"/> show, in authored order with repeats.</summary>
    internal static IEnumerable<ushort> Spritemaps(params GunshipProgram[] programs) =>
        programs.SelectMany(program => GunshipAnimations.Programs.Steps(program))
            .OfType<AnimationProgramStep<P, GunshipFrame, NoAnimationCue>.ShowStep>()
            .Select(step => GunshipFrameDefinitionsConstants.Spritemap(step.Frame));
}
