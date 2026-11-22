namespace SuperMetroid.Core.Samus.Grapple;

/// <summary>Where the swing's sweep of Samus's body line struck terrain, if it did.</summary>
/// <param name="StruckPointFromHand">
/// The first swept body point that struck terrain, counted from the hand: 0 is the point
/// <see cref="GrappleBeamDefinitions.Swing.BodyPointSpacing"/> pixels beyond it. Null when no point struck.
/// </param>
internal readonly partial record struct GrappleSwingCollisionResult(
    int? StruckPointFromHand)
{
    /// <summary>A sweep in which no body point struck terrain.</summary>
    internal static GrappleSwingCollisionResult Clear => new(null);

    /// <summary>A sweep whose body point <paramref name="pointFromHand"/>, counted from the hand, struck terrain first.</summary>
    internal static GrappleSwingCollisionResult Struck(int pointFromHand) => new(pointFromHand);

    /// <summary>Whether a body point struck terrain.</summary>
    internal bool Collided => StruckPointFromHand is not null;
}
