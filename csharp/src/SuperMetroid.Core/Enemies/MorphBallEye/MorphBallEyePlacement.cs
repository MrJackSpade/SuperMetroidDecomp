namespace SuperMetroid.Core.Enemies.MorphBallEye;

/// <summary>
/// What a Morph Ball eye's population parameter two places (#1461): the eye body, which waits for Samus, or one of the
/// beam's wall mounts, with the wall it is mounted on. A negative word is a mount, and its low nibble names the wall.
/// This is the one place that word is decoded.
/// </summary>
/// <param name="IsBeamMount">Whether the slot is a beam mount rather than the eye body.</param>
/// <param name="MountDirection">The wall a mount is mounted on, 0 through 15; the eye body ignores it.</param>
[EncodingOwner("Morph Ball eye population parameter two")]
internal readonly record struct MorphBallEyePlacement(bool IsBeamMount, int MountDirection)
{
    /// <summary>The low nibble the mount direction occupies.</summary>
    private const int MountDirectionBits = 0x000f;

    /// <summary>The placement an authored parameter two holds.</summary>
    /// <param name="parameter2">The population record's parameter two.</param>
    internal static MorphBallEyePlacement Of(int parameter2) =>
        new(unchecked((short)parameter2) < 0, parameter2 & MountDirectionBits);
}
