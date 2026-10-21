namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>
/// The 27 ordinary OAM compositions shared by powered and unpowered Work Robots, shown by their typed programs;
/// walking, wall checks, laser events, and sound timing remain game mechanics.
/// </summary>
internal static class WorkRobotVisualDefinitions
{
    /// <summary>Work Robot instruction and spritemap bank, $A8.</summary>
    internal const byte Bank = 0xa8;

    /// <summary>$A8:D1F1: first powered Work Robot spritemap. Every powered header through $D783 declares twelve five-byte OAM parts.</summary>
    private const ushort FirstPoweredFrame = 0xd1f1;
    /// <summary>$A8:D1F1-D7C0: twelve native poses per facing, followed immediately by the six-part unpowered maps.</summary>
    private const int PoweredPosesPerFacing = 12;
    /// <summary>$A8:D1F1-D783: two-byte count plus twelve five-byte parts per powered record.</summary>
    private const int PoweredRecordBytes = sizeof(ushort) + 12 * 5;
    /// <summary>$A8:D7C1/D7E1/D801: each unpowered record declares six five-byte parts after its two-byte count.</summary>
    private const int UnpoweredRecordBytes = sizeof(ushort) + 6 * 5;
    /// <summary>$A8:D7C1: unpowered-left record immediately follows both complete powered-facing groups.</summary>
    private const ushort UnpoweredLeft = FirstPoweredFrame + 2 * PoweredPosesPerFacing * PoweredRecordBytes;

    /// <summary>Yields twelve powered poses for each facing, then unpowered neutral, left, and right identities in installed compatibility order.</summary>
    internal static IEnumerable<NativeEnemySpritemapDefinition> Frames()
    {
        for (int facing = 0; facing < 2; facing++)
        for (int pose = 0; pose < PoweredPosesPerFacing; pose++)
        {
            string direction = facing == 0 ? "left" : "right";
            ushort pointer = (ushort)(FirstPoweredFrame + (facing * PoweredPosesPerFacing + pose) * PoweredRecordBytes);
            yield return new(Bank, pointer, $"work_robot_{direction}_{pose.ToString("00", System.Globalization.CultureInfo.InvariantCulture)}");
        }
        yield return new(Bank, UnpoweredLeft + UnpoweredRecordBytes, "work_robot_unpowered_neutral");
        yield return new(Bank, UnpoweredLeft, "work_robot_unpowered_left");
        yield return new(Bank, UnpoweredLeft + 2 * UnpoweredRecordBytes, "work_robot_unpowered_right");
    }
}
