using System.Buffers.Binary;
using System.Runtime.InteropServices;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Runtime;
using static SuiteTimingVerification;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using static RetailRuntimeFixtureVerification;

/// <summary>Checks the hardcoded Botwoon wall PLMs and their native slot lifecycle.</summary>
internal static class BotwoonPlmIdentityVerification
{
    /// <summary>
    /// Alternates Clear and Crumble Botwoon wall PLMs through the 40-slot pool, checking
    /// their identity, placement, timers and instruction lists, then full-pool and reset behavior.
    /// </summary>
    internal static void VerifyBotwoonPlmIdentity()
    {
        var bus = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var room = SuperMetroid.AssetExtraction.CartridgeRoomHeaderImporter.Load(bus, RoomHeaderPointers.Botwoon);
        var installation = RepositoryInstallation.Installation;
        var assets = LoadFixtureRoomAssets(bus, room, installation.LoadRoomCharacters(),
            installation.LoadRoomPalettes(), installation.LoadRoomMetatiles(), installation.LoadRoomVisualLayouts());
        var plms = new RoomPlmSystem();
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
        plms.Reset();
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
