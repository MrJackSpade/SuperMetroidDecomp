namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>One-frame debugger witness from the translated `$90:94CB` handler.</summary>
/// <param name="Landed">Whether this update's downward displacement encountered a solid collision; ceiling collisions may end the falling handler without setting this witness.</param>
/// <param name="ImpactYSpeed">Whole pixels per update, 0 through 65535, of the 16.16 Y-speed magnitude after shared speed calculations but before the drained collision handler clears it; retained for landing effects.</param>
/// <param name="ImpactYSubspeed">Fractional half of the same Y-speed magnitude, 0 through 65535 in 1/65536-pixel-per-update units, retained alongside ImpactYSpeed even when live speed is cleared.</param>
public readonly record struct DrainedSamusMovementResult(
    bool Landed,
    int ImpactYSpeed,
    int ImpactYSubspeed);
