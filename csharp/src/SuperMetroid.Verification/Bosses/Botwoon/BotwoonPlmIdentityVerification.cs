using SuperMetroid.Core.Bosses.Botwoon;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Rooms;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.TestSupport.RepositoryInstallation;
namespace SuperMetroid.Verification.Bosses.Botwoon;
using SuperMetroid.Verification.Harness;

/// <summary>Checks the hardcoded Botwoon wall PLMs and their native slot lifecycle.</summary>
internal static class BotwoonPlmIdentityVerification
{
    /// <summary>
    /// Alternates Clear and Crumble Botwoon wall PLMs through the 40-slot pool, checking
    /// their identity, placement, timers and instruction lists, then full-pool and reset behavior.
    /// </summary>
    internal static void VerifyBotwoonPlmIdentity()
    {
        var bus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var room = SuperMetroid.TestSupport.RoomHeaderImport.CartridgeRoomHeaderImporter.Load(bus, RoomHeaderDefinitions.Botwoon.Pointer);
        var installation = RepositoryInstallation.Installation;
        var assets = LoadFixtureRoomAssets(room, installation.LoadRoomCharacters(),
            installation.LoadRoomPalettes(), installation.LoadRoomMetatiles(), installation.LoadRoomVisualLayouts());
        var plms = PlmFixtures.Unbound();
        for (int i = 0; i < 40; i++)
        {
            PlmHeaderId header = (i & 1) == 0 ? PlmHeaderId.ClearBotwoonWall : PlmHeaderId.CrumbleBotwoonWall;
            AssertTrue(plms.BossRoomMutations.TrySpawnBotwoonWall(assets.LevelData, header), "Botwoon native slot allocation succeeds until full");
            var slot = plms.PopulationSlots.Single(slot => slot.NativeSlotIndex == 39 - i);
            AssertEqual(header, slot.HeaderPointer, "hardcoded Botwoon header identity survives allocation");
            AssertEqual(assets.LevelData.GetBlockIndex(new(15, 4)), slot.BlockIndex, "hardcoded Botwoon placement");
            AssertEqual((ushort)((i & 1) == 0 ? 1 : 64), slot.InstructionTimer, "header-specific Botwoon delay");
            AssertTrue(slot.Animation is BotwoonWallPlmAnimation { Program: var program } &&
                program == ((i & 1) == 0 ? BotwoonWallProgram.Clear : BotwoonWallProgram.Crumble),
                "header-specific Botwoon program");
        }
        var before = plms.PopulationSlots.ToArray();
        AssertTrue(!plms.BossRoomMutations.TrySpawnBotwoonWall(assets.LevelData, PlmHeaderId.ClearBotwoonWall), "full native PLM pool declines spawn");
        AssertTrue(before.SequenceEqual(plms.PopulationSlots), "failed spawn preserves every owner and timer");
        plms = PlmFixtures.Unbound();
        AssertTrue(!plms.BombTorizoHand.HasActiveHeader(PlmHeaderId.ClearBotwoonWall) && !plms.BombTorizoHand.HasActiveHeader(PlmHeaderId.CrumbleBotwoonWall),
            "room reset retires both Botwoon header identities");
        Console.WriteLine("Botwoon hardcoded PLMs: both header identities, slot order, placement, programs, timers and full-pool/reset behavior pass.");
    }

    /// <summary>Runs the Botwoon wall PLM identity and allocation checks.</summary>
    [VerificationCommand("--botwoon-plm-identity")]
    internal static int BotwoonPlmIdentityCommand()
    {
        Suite(nameof(VerifyBotwoonPlmIdentity), () => VerifyBotwoonPlmIdentity());
        return 0;
    }
}
