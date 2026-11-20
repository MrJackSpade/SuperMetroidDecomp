using SuperMetroid.Core.Samus.Movement;

namespace SuperMetroid.Core.Samus.Poses;

/// <summary>Which setup the pose update runs when Samus's input chooses no new pose (#2506).</summary>
internal static class SamusNoTransitionSetups
{
    /// <summary>
    /// The setup of <paramref name="pose"/>, or, for a pose that names none, the setup of
    /// <paramref name="movementInForce"/>, the movement her last pose setup left her in. Only the ground turn and the
    /// normal jump name theirs on their movement, so a pose stored without its setup, as the drained poses are, keeps the
    /// one of the movement it was stored over.
    /// </summary>
    /// <exception cref="InvalidOperationException">The pose is undefined.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The movement type has no definition.</exception>
    internal static SamusNoTransitionSetup For(SamusPoseId pose, SamusMovementType movementInForce)
    {
        if (SamusPoseNoTransitionSetupDefinitions.Own.TryGetValue(pose, out SamusNoTransitionSetup own))
            return own;
        return SamusMovementTypeDefinitions.Movements.TryGetValue(movementInForce, out SamusMovementTypeDefinition? movement)
            ? movement.NoTransitionSetup
            : throw new ArgumentOutOfRangeException(nameof(movementInForce), movementInForce,
                "Movement type names no setup for input that chooses no new pose.");
    }
}
