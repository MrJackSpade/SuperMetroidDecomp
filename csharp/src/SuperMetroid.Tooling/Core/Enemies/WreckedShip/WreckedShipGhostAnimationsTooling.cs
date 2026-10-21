using P = SuperMetroid.Core.Enemies.WreckedShip.WreckedShipGhostProgram;
using SuperMetroid.Core.Enemies.WreckedShip.Artwork;
using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Enemies.WreckedShip;

/// <summary>Development-tool members of <see cref="WreckedShipGhostAnimations"/>; never linked by player hosts.</summary>
internal static class WreckedShipGhostAnimationsTooling
{
    /// <summary>The spritemaps <paramref name="programs"/> show, in authored order with repeats.</summary>
    internal static IEnumerable<ushort> Spritemaps(params WreckedShipGhostProgram[] programs) =>
        programs.SelectMany(program => WreckedShipGhostAnimations.Programs.Steps(program))
            .OfType<AnimationProgramStep<P, WreckedShipGhostFrame, NoAnimationCue>.ShowStep>()
            .Select(step => WreckedShipGhostFrameToolingDefinitions.Spritemap(step.Frame));
}
