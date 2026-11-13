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
    /// Alternates Clear and Crumble Botwoon wall PLMs through the 40-slot pool, checking that each is known by what it does
    /// (its terrain change, the crumble its own object) and carries no header identity, its placement and timers, then
    /// full-pool and reset behavior.
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
            AssertTrue(roomObjects.BossRoomMutations.BotwoonWall.TrySpawn(assets.LevelData, mutation), "Botwoon slot allocation succeeds until full");
            var slot = roomObjects.PopulationSlots.Single(slot => slot.ProcessingPosition == i);
            AssertEqual(RoomObjectKind.None, slot.Kind, "a Botwoon wall carries no header identity");
            AssertEqual(new Position16<RoomBlocks>(15, 4), slot.Block, "hardcoded Botwoon placement");
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
        AssertEqual(0, roomObjects.PopulationSlots.Count, "room reset retires both Botwoon walls");
        Console.WriteLine("Botwoon hardcoded PLMs: no header identity, slot order, placement, programs, timers and full-pool/reset behavior pass.");
    }

    /// <summary>Runs the Botwoon wall PLM identity and allocation checks.</summary>
    [VerificationCommand("--botwoon-plm-identity")]
    internal static int BotwoonObjectIdentityCommand()
    {
        Suite(nameof(VerifyBotwoonObjectIdentity), () => VerifyBotwoonObjectIdentity());
        return 0;
    }
}
