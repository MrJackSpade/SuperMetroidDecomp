using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Samus.Movement;
using static SuperMetroid.Core.Samus.Abilities.SamusDeathSequenceDefinitions;

namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>The first frame of the death sequence for each movement type.</summary>
internal static class SamusDeathSequenceFrames
{
    /// <summary>
    /// The death pose's first animation frame for the movement Samus had when she died: the Morph Ball and Spring Ball
    /// movements start at one, the glitch-ball movements at zero and every other movement at five. The animation's timer
    /// is set separately.
    /// </summary>
    /// <param name="movement">The movement type before the death pose.</param>
    /// <returns>The initial death-pose animation frame.</returns>
    internal static SamusAnimationTarget InitialFrame(SamusMovementType movement) =>
        InitialFrames.TryGetValue(movement, out int frame)
            ? new SamusAnimationTarget(SamusAnimationSegmentName.Main, frame)
            : throw new InvalidDataException($"Death-pose frame table has no movement type {movement}.");
}
