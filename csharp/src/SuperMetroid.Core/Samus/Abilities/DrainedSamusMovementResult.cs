namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>What one update of the drained falling movement did.</summary>
/// <param name="Landed">Whether this update's downward displacement encountered a solid collision; ceiling collisions may end the falling handler without setting this witness.</param>
/// <param name="ImpactYSpeed">The 1/65536-pixel Y-speed magnitude after shared speed calculations but before the drained collision handler clears it; retained for landing effects.</param>
public readonly record struct DrainedSamusMovementResult(
    bool Landed,
    UnsignedSixteenSixteen ImpactYSpeed);
