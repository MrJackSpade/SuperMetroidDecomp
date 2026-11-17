using SuperMetroid.Core.Samus.Movement;

namespace SuperMetroid.Core.Samus.Poses;

/// <summary>The momentum response each movement type runs (<see cref="SamusMovementTypeDefinitions"/>).</summary>
public static class SamusMomentumResponses
{
    /// <summary>Reads the momentum response a movement type runs when its input finds no new pose.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The movement type has no definition.</exception>
    public static SamusMomentumResponse ForLookupFailure(SamusMovementType movementType) =>
        SamusMovementTypeDefinitions.Movements.TryGetValue(movementType, out SamusMovementTypeDefinition? definition)
            ? definition.LookupFailureResponse
            : throw new ArgumentOutOfRangeException(nameof(movementType), movementType,
                "Movement type has no lookup-failure momentum response.");
}
