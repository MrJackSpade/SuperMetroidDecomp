using SuperMetroid.TestSupport.MovieReplay;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.WorkRobots;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.World;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;

namespace SuperMetroid.Verification.Enemies.WorkRobots;

/// <summary>
/// Checks the unpowered Work Robot pose a placement decodes to against native words, and that the production
/// initializer holds it (#1519).
/// </summary>
internal static class WorkRobotInitialSelectionVerification
{
    /// <summary>
    /// The three retail table words name the three unpowered programs the codec decodes and the production initializer
    /// installs; the native overrun selector three fails to decode, and selectors the range check refuses hold the
    /// neutral pose.
    /// </summary>
    internal static void VerifyWorkRobotInitialSelection(CartridgeImportAddressSpace rom)
    {
        for (ushort parameter = 0; parameter < 3; parameter++)
        {
            int address = EnemyRomTablePointers.WorkRobot.InitialAnimationScriptWords + 2 * parameter;
            WorkRobotSegment expected = SuperMetroid.AssetExtraction.Enemies.SpawnWordDecodes.WorkRobotPose(parameter);
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord(address),
                NativeEnemyAnimationOracle.EntryOf(WorkRobotNativeListAddresses.Entries, expected),
                $"Work Robot original selection {parameter}");
            var system = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom());
            var slot = new RoomEnemySlot(0, EnemySlotFixtures.Registry);
            slot.SetSpawnParameter1(EnemyDefinitionId.Robot, parameter);
            var state = new WorkRobotEnemyState();
            system.WorkRobotBehavior.InitializeWorkRobotNoPower(slot, state);
            AssertEqual(new AnimationPlayback<WorkRobotSegment>(WorkRobotAnimations.StartingWith(expected), AnimationPlace.StartOfAnimation), state.Playback,
                $"Work Robot production selection {parameter} without cartridge access");
        }
        AssertThrows<InvalidDataException>(() => SuperMetroid.AssetExtraction.Enemies.SpawnWordDecodes.WorkRobotPose(3),
            "Work Robot selector three overruns the table into code");
        foreach (int refused in new[] { 4, 0x7fff, 0x8000, 0xffff })
        {
            AssertEqual(WorkRobotSegment.NoPowerNeutral, SuperMetroid.AssetExtraction.Enemies.SpawnWordDecodes.WorkRobotPose(refused),
                $"Work Robot selector {refused} outside the range check holds the neutral pose");
        }
        Console.WriteLine("Work Robot switch: three original programs, the overrun selector's loud failure, production initialization and bounds pass.");
    }
}
