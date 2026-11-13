namespace SuperMetroid.Core.Enemies.Platforms;

/// <summary>Chooses a Tripper's or Kamer's program for its motion and direction.</summary>
internal static class PlatformPrograms
{
    /// <summary>
    /// The program for the zero/nonzero direction branches: vertical motion selects the moving
    /// programs, and every nonzero direction value selects the right-moving program.
    /// </summary>
    internal static PlatformProgram SelectProgram(bool isKamer, Axis axis, PlatformHorizontalMovement direction) =>
        // The test is zero against nonzero: any direction value other than left's moves right.
        PlatformAnimationDefinitions.Programs[new PlatformPose(isKamer, axis,
            direction == PlatformHorizontalMovement.Left ? PlatformHorizontalMovement.Left : PlatformHorizontalMovement.Right)];
}
