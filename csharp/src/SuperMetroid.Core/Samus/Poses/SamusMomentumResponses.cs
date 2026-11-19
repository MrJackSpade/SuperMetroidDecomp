using SuperMetroid.Core.Samus.Movement;

namespace SuperMetroid.Core.Samus.Poses;

/// <summary>The momentum response each movement type runs (<see cref="SamusMovementTypeDefinitions"/>).</summary>
public static class SamusMomentumResponses
{
    /// <summary>Reads the momentum response a movement type runs when its input finds no new pose.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The movement type has no definition.</exception>
    public static SamusMomentumResponse ForNoTransition(SamusMovementType movementType) =>
        SamusMovementTypeDefinitions.Movements.TryGetValue(movementType, out SamusMovementTypeDefinition? definition)
            ? definition.NoTransitionResponse
            : throw new ArgumentOutOfRangeException(nameof(movementType), movementType,
                "Movement type names no momentum response for input that chooses no new pose.");
}
