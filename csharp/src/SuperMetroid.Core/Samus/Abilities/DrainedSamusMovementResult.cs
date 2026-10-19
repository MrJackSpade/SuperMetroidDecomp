namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>One-frame debugger witness from the translated `$90:94CB` handler.</summary>
/// <param name="Landed">Whether this update's downward displacement encountered a solid collision; ceiling collisions may end the falling handler without setting this witness.</param>
/// <param name="ImpactYSpeed">The 16.16 Y-speed magnitude after shared speed calculations but before the drained collision handler clears it; retained for landing effects.</param>
public readonly record struct DrainedSamusMovementResult(
    bool Landed,
    SnesUnsignedSixteenSixteen ImpactYSpeed);
