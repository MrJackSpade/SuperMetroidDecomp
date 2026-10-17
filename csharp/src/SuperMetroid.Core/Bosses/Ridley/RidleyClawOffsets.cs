namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>Signed claw displacements used by Ridley's grab collision and carried-Samus placement.</summary>
public static class RidleyClawOffsets
{
    /// <summary>$A6:B9D5 HoldingSamusXDispacement: left/turning/right displacements decrease twelve pixels per facing.</summary>
    public static short ReadX(RidleyFacing facing) => facing switch
    {
        RidleyFacing.Left or RidleyFacing.Turning or RidleyFacing.Right => (short)(12 - 12 * (int)facing),
        _ => throw new InvalidOperationException($"Undefined {nameof(RidleyFacing)} {(int)facing}."),
    };

    /// <summary>
    /// $A6:B9DB HoldingSamusYDispacement: the claw's height below Ridley's origin for each claw-height pose (zero
    /// standing, one legs half extended, two legs extended), advancing 10.5 pixels per pose rounded upward.
    /// </summary>
    /// <remarks>
    /// Ridley's body programs select only these three poses, so no other pose reaches the claw. The cartridge table is
    /// followed by unrelated machine code that a larger index would read; that read is unreachable and is rejected.
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="footPose"/> is not one of the three poses.</exception>
    public static short ReadY(int footPose) => (uint)footPose < 3
        ? (short)(35 + (21 * footPose + 1) / 2)
        : throw new ArgumentOutOfRangeException(nameof(footPose), footPose, "Ridley has three claw-height poses.");
}
