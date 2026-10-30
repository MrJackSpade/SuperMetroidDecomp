namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// What Bomb Torizo's falling statue pieces gain in speed each update (#1470): not their own acceleration but that of
/// the piece in the first enemy-projectile slot, and nothing while that slot is free. The pieces are spawned into the
/// last free slots, so in the retail fight the first slot is free and the pieces fall at their launch speed.
/// </summary>
/// <remarks>
/// Consulted by the falling piece's update. Evidence: the piece's update at <c>$86:A8EF</c> adds the acceleration word
/// it reads without the slot index, which is the first slot's word; every piece's spawn sets its own word. Any other
/// projectile in the first slot keeps no acceleration a piece could read in its terms, so that case is refused rather
/// than given an invented value.
/// </remarks>
[CompatibilityService("Bomb Torizo's falling statue pieces gain speed by the first projectile slot's piece acceleration, so they fall at constant speed while that slot is free")]
public static class FirstSlotFallAcceleration
{
    /// <summary>The acceleration a falling piece adds this update.</summary>
    /// <param name="firstSlotActive">Whether the first enemy-projectile slot holds a live projectile.</param>
    /// <param name="firstSlotPieceAcceleration">The acceleration of the statue piece in the first slot, or null when no piece is there.</param>
    /// <exception cref="InvalidOperationException">The first slot holds a projectile that is not a statue piece.</exception>
    public static int Of(bool firstSlotActive, int? firstSlotPieceAcceleration) =>
        firstSlotPieceAcceleration ?? (firstSlotActive
            ? throw new InvalidOperationException(
                "A falling statue piece read the first projectile slot while it held a projectile that is not a statue piece.")
            : 0);
}
