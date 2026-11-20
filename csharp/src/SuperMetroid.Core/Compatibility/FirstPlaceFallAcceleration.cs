namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// What Bomb Torizo's falling statue pieces gain in speed each update (#1470): not their own acceleration but that of
/// the piece in the first enemy-projectile place, and nothing while that place is free. The pieces are spawned into the
/// last free places, so in the game's fight the first place is free and the pieces fall at their launch speed.
/// </summary>
/// <remarks>
/// Consulted by the falling piece's update. Evidence: the piece's update at <c>$86:A8EF</c> adds the acceleration word
/// it reads without the place index, which is the first place's word; every piece's spawn sets its own word. Any other
/// projectile in the first place keeps no acceleration a piece could read in its terms, so that case is refused rather
/// than given an invented value.
/// </remarks>
[CompatibilityService("Bomb Torizo's falling statue pieces gain speed by the first projectile place's piece acceleration, so they fall at constant speed while that place is free")]
public static class FirstPlaceFallAcceleration
{
    /// <summary>The acceleration a falling piece adds this update.</summary>
    /// <param name="firstPlaceActive">Whether the first enemy-projectile place holds a live projectile.</param>
    /// <param name="firstPlacePieceAcceleration">The acceleration of the statue piece in the first place, or null when no piece is there.</param>
    /// <exception cref="InvalidOperationException">The first place holds a projectile that is not a statue piece.</exception>
    public static SignedEightEight Of(FirstPlaceHolding firstPlace) => firstPlace switch
    {
        FirstPlaceHolding.Free => SignedEightEight.Zero,
        FirstPlaceHolding.StatuePiece piece => piece.Acceleration,
        FirstPlaceHolding.OtherProjectile => throw new InvalidOperationException(
            "A falling statue piece read the first projectile place while it held a projectile that is not a statue piece."),
        _ => throw new InvalidOperationException($"Undefined first projectile place holding {firstPlace}."),
    };
}
