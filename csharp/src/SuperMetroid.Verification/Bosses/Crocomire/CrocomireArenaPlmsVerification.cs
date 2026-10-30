using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.Verification.Rooms.Plms;
using static SuperMetroid.Verification.Bosses.Crocomire.LookupCrocomireDrawsVerification;
using static SuperMetroid.Verification.Rooms.Plms.BossRoomPlmProgramsVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using static SuperMetroid.Verification.Harness.VerificationHelpersVerification;
using SuperMetroid.Core.Bosses.Crocomire;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
namespace SuperMetroid.Verification.Bosses.Crocomire;
using SuperMetroid.Verification.Harness;

/// <summary>Checks compiled Crocomire arena PLM programs, draws, and physical block mutations.</summary>
internal static class CrocomireArenaPlmsVerification
{
    /// <summary>Compares five native programs and checks their room mutations without rereading source ROM.</summary>
    internal static void VerifyCompiledCrocomireArenaPlms()
    {
        var rom = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(
            Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SuperMetroid.AssetExtraction.Cartridge.SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Crocomire native oracle revision");
        Suite(nameof(VerifyCrocomireArenaPrograms), () => VerifyCrocomireArenaPrograms(rom));

        Suite(nameof(VerifyCrocomirePhysicalDrawMapping), () => VerifyCrocomirePhysicalDrawMapping(rom));

        Suite(nameof(VerifyCrocomireMutation), () => VerifyCrocomireMutation(PlmHeaderId.ClearCrocomireBridge,
            cell => cell.Y == 0 && cell.X < 10 ? Block(0x80, RoomCollisionType.Air) : Untouched));
        Suite(nameof(VerifyCrocomireMutation), () => VerifyCrocomireMutation(PlmHeaderId.CrumbleCrocomireBridgeBlock,
            cell => cell == Position16<RoomBlocks>.Zero ? Block(0x10b, RoomCollisionType.SolidBlock) : Untouched));
        Suite(nameof(VerifyCrocomireMutation), () => VerifyCrocomireMutation(PlmHeaderId.ClearCrocomireBridgeBlock,
            cell => cell == Position16<RoomBlocks>.Zero ? Block(0x80, RoomCollisionType.Air) : Untouched));
        Suite(nameof(VerifyCrocomireMutation), () => VerifyCrocomireMutation(PlmHeaderId.ClearCrocomireInvisibleWall,
            cell => cell.X < 3 && cell.Y < 8 ? WallBlock(cell, RoomCollisionType.Air) : Untouched));
        Suite(nameof(VerifyCrocomireMutation), () => VerifyCrocomireMutation(PlmHeaderId.CreateCrocomireInvisibleWall,
            cell => cell.X < 3 && cell.Y < 8 ? WallBlock(cell, RoomCollisionType.SolidBlock) : Untouched));
        Console.WriteLine("Crocomire PLMs: five programs and physical draws match ROM; all mutations execute without source reads.");

    }

    /// <summary>The solid block every room cell starts as, which a mutation outside its draw leaves alone.</summary>
    private static readonly RoomTerrainBlock Untouched = Block(0x123, RoomCollisionType.SolidBlock);

    /// <summary>An unmirrored block of <paramref name="kind"/> showing tileset block <paramref name="block"/>.</summary>
    private static RoomTerrainBlock Block(int block, RoomCollisionType kind) => RoomTerrainBlock.Of(block, BlockMirroring.None, kind);

    /// <summary>Calculates the expected invisible-wall block: its column's tiles down three tileset rows, air or solid.</summary>
    private static RoomTerrainBlock WallBlock(Position16<RoomBlocks> cell, RoomCollisionType kind)
    {
        int block = cell.Y switch
        {
            0 or 6 or 7 => 0x80,
            1 or 3 => 0x107 + cell.X,
            2 or 4 => 0x127 + cell.X,
            5 => 0x147 + cell.X,
            _ => throw new ArgumentOutOfRangeException(nameof(cell)),
        };
        return Block(block, kind);
    }

    /// <summary>Spawns one arena PLM and compares its first-frame block changes and next-frame deletion.</summary>
    private static void VerifyCrocomireMutation(PlmHeaderId header,
        Func<Position16<RoomBlocks>, RoomTerrainBlock> expectedBlock)
    {
        const int width = 32;
        const int height = 16;
        RoomTerrainBlock[] terrain = Enumerable.Repeat(Untouched, width * height).ToArray();
        RoomLevelData level = CreateRoom(new(width, height), terrain, new RoomBlockBehavior[terrain.Length]);
        var plms = PlmFixtures.Unbound();
        AssertTrue(plms.BossRoomMutations.TrySpawnCrocomireArenaMutation(level, new(5, 3), header),
            $"Crocomire header {header} allocates");
        var guard = NewCrocomireSourceGuard(new TestAddressSpace());
        plms.Step(level);
        AssertEqual(1, plms.SlotTable.ActiveCount,
            $"Crocomire header {header} persists for native draw frame");
        for (int y = 0; y < 9; y++)
        for (int x = 0; x < 11; x++)
            AssertEqual(expectedBlock(new((ushort)x, (ushort)y)),
                level.GetCollisionBlockByIndex((3 + y) * width + 5 + x).Terrain,
                $"Crocomire header {header} physical block ({x},{y})");
        plms.Step(level);
        AssertEqual(0, plms.SlotTable.ActiveCount,
            $"Crocomire header {header} deletes on next frame");
        AssertEqual(0, guard.ForbiddenReadAttempts,
            $"Crocomire header {header} reads no migrated ROM records");
    }

    /// <summary>Rejects live reads of the bank-$84 program and draw records now supplied by compiled definitions.</summary>
    internal static CartridgeReadGuard NewCrocomireSourceGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source)
            .Forbid(address =>
            {
                int pointer = SnesAddressMath.OffsetInBank(address);
                return (SnesAddressMath.BankNumber(address)) == 0x84 &&
                    (BossRoomPlmNativeListAddresses.IsTypedProgramByte(pointer) ||
                     (pointer >= PlmDrawAddressCodec.Of(PlmDraw.Of(CrocomireArenaDraw.ClearBridge)) &&
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
