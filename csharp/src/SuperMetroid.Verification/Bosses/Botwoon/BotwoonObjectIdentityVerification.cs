using SuperMetroid.Core.Bosses.Botwoon;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Rooms;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.TestSupport.RepositoryInstallation;
namespace SuperMetroid.Verification.Bosses.Botwoon;
using SuperMetroid.Verification.Harness;

/// <summary>Checks the hardcoded Botwoon wall PLMs and their native slot lifecycle.</summary>
internal static class BotwoonObjectIdentityVerification
{
    /// <summary>
    /// Alternates Clear and Crumble Botwoon wall PLMs through the 40-slot pool, checking
    /// their identity, placement, timers and instruction lists, then full-pool and reset behavior.
    /// </summary>
    internal static void VerifyBotwoonObjectIdentity()
    {
        var bus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var room = SuperMetroid.TestSupport.RoomHeaderImport.CartridgeRoomHeaderImporter.Load(bus, RoomHeaderDefinitions.Botwoon.Pointer);
        var installation = RepositoryInstallation.Installation;
        var assets = LoadFixtureRoomAssets(room, installation.LoadRoomVisualLayouts());
        var roomObjects = RoomObjectFixtures.Unbound();
        for (int i = 0; i < 40; i++)
        {
            BotwoonWallMutation mutation = (i & 1) == 0 ? BotwoonWallMutation.Clear : BotwoonWallMutation.Crumble;
            RoomObjectHeaderId header = (i & 1) == 0 ? RoomObjectHeaderId.ClearBotwoonWall : RoomObjectHeaderId.CrumbleBotwoonWall;
            AssertTrue(roomObjects.BossRoomMutations.BotwoonWall.TrySpawn(assets.LevelData, mutation), "Botwoon slot allocation succeeds until full");
            var slot = roomObjects.PopulationSlots.Single(slot => slot.ProcessingPosition == i);
            AssertEqual(header, slot.HeaderPointer, "hardcoded Botwoon header identity survives allocation");
            AssertTrue(slot.Animation is null, "the Botwoon wall runs no program");
            AssertEqual(assets.LevelData.GetBlockIndex(new(15, 4)), slot.BlockIndex, "hardcoded Botwoon placement");
            TerrainChange expectedChange = (i & 1) == 0 ? BotwoonWallMutationDefinitions.Clear : BotwoonWallMutationDefinitions.Crumble;
            AssertTrue(slot.Behaviour is TerrainChangeObject { Change: var change } && change == expectedChange,
                "header-specific Botwoon change");
            AssertEqual((i & 1) == 0 ? 1 : 64, expectedChange.FirstFrameDelay, "header-specific Botwoon delay");
            AssertEqual((i & 1) != 0, slot.Behaviour is BotwoonWallCrumbleObject, "only the crumble opens the scrolls");
        }
        var before = roomObjects.PopulationSlots.ToArray();
        AssertTrue(!roomObjects.BossRoomMutations.BotwoonWall.TrySpawn(assets.LevelData, BotwoonWallMutation.Clear), "full native PLM pool declines spawn");
        AssertTrue(before.SequenceEqual(roomObjects.PopulationSlots), "failed spawn preserves every owner and timer");
        roomObjects = RoomObjectFixtures.Unbound();
        AssertTrue(!roomObjects.BombTorizoHand.HasActiveHeader(RoomObjectHeaderId.ClearBotwoonWall) && !roomObjects.BombTorizoHand.HasActiveHeader(RoomObjectHeaderId.CrumbleBotwoonWall),
            "room reset retires both Botwoon header identities");
        Console.WriteLine("Botwoon hardcoded PLMs: both header identities, slot order, placement, programs, timers and full-pool/reset behavior pass.");
    }

    /// <summary>Runs the Botwoon wall PLM identity and allocation checks.</summary>
    [VerificationCommand("--botwoon-plm-identity")]
    internal static int BotwoonObjectIdentityCommand()
    {
        Suite(nameof(VerifyBotwoonObjectIdentity), () => VerifyBotwoonObjectIdentity());
        return 0;
    }
}
