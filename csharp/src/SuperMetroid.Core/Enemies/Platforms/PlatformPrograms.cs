namespace SuperMetroid.Core.Enemies.Platforms;

/// <summary>Chooses a Tripper's or Kamer's program for its motion and direction.</summary>
internal static class PlatformPrograms
{
    /// <summary>
    /// The program for the zero/nonzero direction branches at $A3:9ECB and $A3:9EF1: vertical motion selects the moving
    /// programs, and every nonzero direction word selects the right-moving program.
    /// </summary>
    internal static PlatformProgram SelectProgram(bool isKamer, Axis axis, PlatformHorizontalMovement direction) =>
        PlatformAnimationDefinitions.Programs[new PlatformPose(isKamer, axis, direction)];
}
