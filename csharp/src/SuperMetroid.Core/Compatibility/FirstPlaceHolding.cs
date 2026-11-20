namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// What the first enemy projectile place holds as a falling statue piece reads it (#2560): a closed choice of a free place,
/// a statue piece with its acceleration, and another projectile.
/// </summary>
public abstract record FirstPlaceHolding
{
    /// <summary>The place holds nothing.</summary>
    public sealed record Free : FirstPlaceHolding;

    /// <summary>The place holds one of Bomb Torizo's breaking statue pieces.</summary>
    /// <param name="Acceleration">The piece's fall acceleration.</param>
    public sealed record StatuePiece(SignedEightEight Acceleration) : FirstPlaceHolding;

    /// <summary>The place holds a projectile that is not a statue piece.</summary>
    public sealed record OtherProjectile : FirstPlaceHolding;
}
