namespace SuperMetroid.Core.Enemies.MorphBallEye;

/// <summary>
/// What a Morph Ball eye placement is (#1461, #1472): the eye body, which waits for Samus, or one of the beam's wall
/// mounts, with the wall it is mounted on.
/// </summary>
/// <param name="IsBeamMount">Whether the slot is a beam mount rather than the eye body.</param>
/// <param name="MountDirection">The wall a mount is mounted on, 0 through 15; the eye body ignores it.</param>
internal readonly record struct MorphBallEyePlacement(bool IsBeamMount, int MountDirection)
{
    /// <summary>The placement a placement's spawn settings name.</summary>
    internal static MorphBallEyePlacement Of(MorphBallEyeSpawnSettings settings) =>
        new(Ensure.NotNull(settings).IsBeamMount, settings.MountDirection);
}
