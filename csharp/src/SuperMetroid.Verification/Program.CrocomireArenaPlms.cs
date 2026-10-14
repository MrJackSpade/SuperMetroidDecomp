using System.Buffers.Binary;
using System.Runtime.InteropServices;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using SuperMetroid.AssetExtraction;
using static AssertionsVerification;
using static LookupCrocomireDrawsVerification;
using static BossRoomPlmProgramsVerification;
using static SuiteTimingVerification;
using static VerificationHelpersVerification;

/// <summary>Checks compiled Crocomire arena PLM programs, draws, and physical block mutations.</summary>
internal static class CrocomireArenaPlmsVerification
{
    /// <summary>Compares five native programs and checks their room mutations without rereading source ROM.</summary>
    internal static void VerifyCompiledCrocomireArenaPlms()
    {
        var rom = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(
            Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SuperMetroid.AssetExtraction.SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Crocomire native oracle revision");
        Suite(nameof(VerifyCrocomireArenaPrograms), () => VerifyCrocomireArenaPrograms(rom));

        Suite(nameof(VerifyCrocomirePhysicalDrawMapping), () => VerifyCrocomirePhysicalDrawMapping(rom));

        Suite(nameof(VerifyCrocomireMutation), () => VerifyCrocomireMutation(PlmHeaderId.ClearCrocomireBridge,
            cell => cell.Y == 0 && cell.X < 10 ? (ushort)0x0080 : (ushort)0x8123));
        Suite(nameof(VerifyCrocomireMutation), () => VerifyCrocomireMutation(PlmHeaderId.CrumbleCrocomireBridgeBlock,
            cell => cell == Position16<RoomBlocks>.Zero ? (ushort)0x810b : (ushort)0x8123));
        Suite(nameof(VerifyCrocomireMutation), () => VerifyCrocomireMutation(PlmHeaderId.ClearCrocomireBridgeBlock,
            cell => cell == Position16<RoomBlocks>.Zero ? (ushort)0x0080 : (ushort)0x8123));
        Suite(nameof(VerifyCrocomireMutation), () => VerifyCrocomireMutation(PlmHeaderId.ClearCrocomireInvisibleWall,
            cell => cell.X < 3 && cell.Y < 8 ? WallWord(cell, false) : (ushort)0x8123));
        Suite(nameof(VerifyCrocomireMutation), () => VerifyCrocomireMutation(PlmHeaderId.CreateCrocomireInvisibleWall,
            cell => cell.X < 3 && cell.Y < 8 ? WallWord(cell, true) : (ushort)0x8123));
        Console.WriteLine("Crocomire PLMs: five programs and physical draws match ROM; all mutations execute without source reads.");

    }

    /// <summary>Calculates the expected native invisible-wall metatile, with its solid bit when built.</summary>
    private static ushort WallWord(Position16<RoomBlocks> cell, bool solid)
    {
        ushort value = cell.Y switch
        {
            0 or 6 or 7 => 0x0080,
            1 or 3 => checked((ushort)(0x0107 + cell.X)),
            2 or 4 => checked((ushort)(0x0127 + cell.X)),
            5 => checked((ushort)(0x0147 + cell.X)),
            _ => throw new ArgumentOutOfRangeException(nameof(cell)),
        };
        return solid ? (ushort)(value | 0x8000) : value;
    }

    /// <summary>Spawns one arena PLM and compares its first-frame block changes and next-frame deletion.</summary>
    private static void VerifyCrocomireMutation(PlmHeaderId header,
        Func<Position16<RoomBlocks>, ushort> expectedWord)
    {
        const int width = 32;
        const int height = 16;
        ushort[] sourceWords = Enumerable.Repeat((ushort)0x8123,
            width * height).ToArray();
        RoomLevelData level = CreateRoom(new(width, height), sourceWords,
            new byte[sourceWords.Length], blockDefinitions: new byte[0x400 * 8]);
        var plms = new RoomPlmSystem();
        AssertTrue(plms.BossRoomMutations.TrySpawnCrocomireArenaMutation(level, new(5, 3), header),
            $"Crocomire header ${(int)header:X4} allocates");
        var guard = NewCrocomireSourceGuard(new TestAddressSpace());
        BackgroundTilemapStreamer streamer = level.CreateBackgroundStreamer();
        plms.Step(guard, level, streamer, new PlmView { Layer1Position = new(0, 0), Bg1XOffset = 0 });
        AssertEqual(1, plms.SlotTable.ActiveCount,
            $"Crocomire header ${(int)header:X4} persists for native draw frame");
        for (int y = 0; y < 9; y++)
        for (int x = 0; x < 11; x++)
            AssertEqual(expectedWord(new((ushort)x, (ushort)y)),
                level.GetCollisionBlockByIndex((3 + y) * width + 5 + x).LevelWord,
                $"Crocomire header ${(int)header:X4} physical block ({x},{y})");
        plms.Step(guard, level, streamer, new PlmView { Layer1Position = new(0, 0), Bg1XOffset = 0 });
        AssertEqual(0, plms.SlotTable.ActiveCount,
            $"Crocomire header ${(int)header:X4} deletes on next frame");
        AssertEqual(0, guard.ForbiddenReadAttempts,
            $"Crocomire header ${(int)header:X4} reads no migrated ROM records");
    }

    /// <summary>Rejects live reads of the bank-$84 program and draw records now supplied by compiled definitions.</summary>
    internal static CartridgeReadGuard NewCrocomireSourceGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source)
            .Forbid(address =>
            {
                int pointer = SnesAddressMath.OffsetInBank(address);
                return (SnesAddressMath.BankNumber(address)) == 0x84 &&
                    (BossRoomPlmNativeListAddresses.IsTypedProgramByte(pointer) ||
                     (pointer >= (ushort)CrocomireArenaDraw.ClearBridge &&
                      pointer < CrocomireArenaPlmDrawDefinitions.EndExclusive));
            }, address => $"Crocomire PLM reread compiled source ${address:X6}.");

    /// <summary>Runs the Crocomire arena PLM comparison with the retail ROM oracle.</summary>
    [VerificationCommand("--crocomire-arena-plms")]
    internal static int CrocomireArenaPlmsCommand()
    {
        Suite(nameof(VerifyCompiledCrocomireArenaPlms), () => VerifyCompiledCrocomireArenaPlms());
        return 0;
    }
}
