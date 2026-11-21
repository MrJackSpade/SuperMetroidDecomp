namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// What the first enemy projectile place holds as a falling statue piece reads it (#2560): a closed choice of a free place,
/// a statue piece with its acceleration, and another projectile.
/// </summary>
/// <remarks>
/// Read by <see cref="FirstPlaceFallAcceleration"/>. Evidence: the piece's update at <c>$86:A8EF</c> adds the acceleration
/// word it reads without the place index, the first place's word, which a free place leaves at zero.
/// </remarks>
[CompatibilityService("what the first enemy projectile place holds decides a falling statue piece's acceleration")]
public abstract partial record FirstPlaceHolding
{
    /// <summary>The place holds nothing.</summary>
    public sealed partial record Free : FirstPlaceHolding;

    /// <summary>The place holds one of Bomb Torizo's breaking statue pieces.</summary>
    /// <param name="Acceleration">The piece's fall acceleration.</param>
    public sealed partial record StatuePiece(SignedEightEight Acceleration) : FirstPlaceHolding;

    /// <summary>The place holds a projectile that is not a statue piece.</summary>
    public sealed partial record OtherProjectile : FirstPlaceHolding;
}
