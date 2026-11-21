using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Samus.Movement;
using static SuperMetroid.Core.Samus.Abilities.SamusDeathSequenceDefinitions;

namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>The first frame of the death sequence for each movement type.</summary>
internal static class SamusDeathSequenceFrames
{
    /// <summary>
    /// The death pose's first animation frame for the movement Samus had when she died: the Morph Ball and Spring Ball
    /// movements start on its ball frame, the glitch-ball movements on its first frame and every other movement on its
    /// upright frame. The animation's timer is set separately.
    /// </summary>
    /// <param name="movement">The movement type before the death pose.</param>
    /// <returns>The initial death-pose animation frame.</returns>
    internal static SamusAnimationFrame InitialFrame(SamusMovementType movement) => InitialFrames[movement];
}
