using SuperMetroid.Verification.Bosses.MotherBrain;
using SuperMetroid.Core.Enemies.WorkRobots;

namespace SuperMetroid.Verification.Enemies.WorkRobots;

/// <summary>
/// The native bank-$86 lists the Work Robot laser ROM oracle (#641) walks: each program's entry point. Only verification
/// reads them; the port runs typed programs.
/// </summary>
internal static class WorkRobotLaserNativeListAddresses
{
    /// <summary>Each typed program's native entry point, in authored order.</summary>
    internal static readonly NativeProgramEntry<WorkRobotLaserProgram>[] Entries =
    [
        new NativeProgramEntry<WorkRobotLaserProgram>(WorkRobotLaserProgram.Initial, 0xd2ec),
        new NativeProgramEntry<WorkRobotLaserProgram>(WorkRobotLaserProgram.Loop, 0xd2f8),
    ];

    /// <summary>The bank-$86 bytes the lists occupy, $86:D2EC-D30B, which production must never read.</summary>
    internal static readonly MotherBrainEffectNativeListAddressesExtent Extent = new MotherBrainEffectNativeListAddressesExtent(0xd2ec, 0xd30c);
}
