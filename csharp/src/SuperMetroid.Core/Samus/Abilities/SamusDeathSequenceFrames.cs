using SuperMetroid.Core.Samus.Movement;
using static SuperMetroid.Core.Samus.Abilities.SamusDeathSequenceDefinitions;

namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>The first frame of the death sequence for each movement type.</summary>
public static class SamusDeathSequenceFrames
{
    /// <summary>
    /// $9B:B420: the initial visible death-pose animation frame the movement type in effect before fatal-damage pose
    /// setup selects, without replacing the separately initialized animation timer: ball movement starts at one, the
    /// two unused glitch-ball types at zero, every other movement type at five.
    /// </summary>
    /// <param name="movement">The movement type before the death pose.</param>
    /// <returns>The initial death-pose animation frame.</returns>
    public static byte InitialFrame(SamusMovementType movement) =>
        InitialFrames.ContainsKey(movement) ? InitialFrames[movement] : throw new InvalidDataException($"Death-pose frame table has no movement type {movement}.");
}
