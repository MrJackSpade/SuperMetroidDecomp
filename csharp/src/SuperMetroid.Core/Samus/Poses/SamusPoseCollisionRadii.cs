namespace SuperMetroid.Core.Samus.Poses;

/// <summary>The vertical collision radius of each pose.</summary>
internal static class SamusPoseCollisionRadii
{
    /// <summary>The vertical collision radius of <paramref name="pose"/>, in pixels.</summary>
    /// <exception cref="InvalidOperationException">The pose is undefined.</exception>
    internal static int ReadVerticalRadius(SamusPoseId pose) => SamusPoseTraits.Of(pose).VerticalRadius;
}
