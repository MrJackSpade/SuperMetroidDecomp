using SuperMetroid.Core.Samus.Movement;

namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>What one update of the drained falling movement did.</summary>
/// <param name="Landed">Whether this update's downward displacement encountered a solid collision; ceiling collisions may end the drained fall without setting this witness.</param>
/// <param name="ImpactVelocity">Her vertical velocity after shared speed calculations but before the drained collision response clears its speed; retained for landing effects.</param>
public readonly record struct DrainedSamusMovementResult(
    bool Landed,
    SamusVerticalVelocity ImpactVelocity);
