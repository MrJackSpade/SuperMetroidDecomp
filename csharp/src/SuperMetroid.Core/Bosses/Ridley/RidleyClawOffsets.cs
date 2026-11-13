namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>Signed claw displacements used by Ridley's grab collision and carried-Samus placement.</summary>
public static class RidleyClawOffsets
{
    /// <summary>The held Samus's horizontal offset: left/turning/right displacements decrease twelve pixels per facing.</summary>
    public static int ReadX(RidleyFacing facing) => facing switch
    {
        RidleyFacing.Left => 12,
        RidleyFacing.Turning => 0,
        RidleyFacing.Right => -12,
        _ => throw new InvalidOperationException($"Undefined {nameof(RidleyFacing)} {facing}."),
    };

    /// <summary>
    /// The held Samus's vertical offset: the claw's height below Ridley's origin for each claw-height pose (zero
    /// standing, one legs half extended, two legs extended), advancing 10.5 pixels per pose rounded upward.
    /// </summary>
    /// <remarks>
    /// Ridley's body programs select only these three poses, so no other pose reaches the claw. A larger index has no
    /// offset; it is unreachable and is rejected.
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="footPose"/> is not one of the three poses.</exception>
    public static int ReadY(int footPose) => (uint)footPose < 3
        ? 35 + (21 * footPose + 1) / 2
        : throw new ArgumentOutOfRangeException(nameof(footPose), footPose, "Ridley has three claw-height poses.");
}
