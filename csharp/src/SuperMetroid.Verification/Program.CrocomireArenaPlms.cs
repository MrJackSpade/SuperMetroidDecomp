using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using SuperMetroid.AssetExtraction;

internal static partial class Program
{
    private static void VerifyCompiledCrocomireArenaPlms()
    {
        var rom = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(
            Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SuperMetroid.AssetExtraction.SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Crocomire native oracle revision");
        Suite(nameof(VerifyCrocomireProgramControls), () => VerifyCrocomireProgramControls(rom));
        Suite(nameof(VerifyCrocomireProgramDraws), () => VerifyCrocomireProgramDraws(rom));

        Suite(nameof(VerifyCrocomirePhysicalDrawMapping), () => VerifyCrocomirePhysicalDrawMapping(rom));

        Suite(nameof(VerifyCrocomireMutation), () => VerifyCrocomireMutation(PlmHeaderId.ClearCrocomireBridge,
            (x, y) => y == 0 && x < 10 ? (ushort)0x0080 : (ushort)0x8123));
        Suite(nameof(VerifyCrocomireMutation), () => VerifyCrocomireMutation(PlmHeaderId.CrumbleCrocomireBridgeBlock,
            (x, y) => x == 0 && y == 0 ? (ushort)0x810b : (ushort)0x8123));
        Suite(nameof(VerifyCrocomireMutation), () => VerifyCrocomireMutation(PlmHeaderId.ClearCrocomireBridgeBlock,
            (x, y) => x == 0 && y == 0 ? (ushort)0x0080 : (ushort)0x8123));
        Suite(nameof(VerifyCrocomireMutation), () => VerifyCrocomireMutation(PlmHeaderId.ClearCrocomireInvisibleWall,
            (x, y) => x < 3 && y < 8 ? WallWord(x, y, false) : (ushort)0x8123));
        Suite(nameof(VerifyCrocomireMutation), () => VerifyCrocomireMutation(PlmHeaderId.CreateCrocomireInvisibleWall,
            (x, y) => x < 3 && y < 8 ? WallWord(x, y, true) : (ushort)0x8123));
        Console.WriteLine("Crocomire PLMs: five programs and physical draws match ROM; all mutations execute without source reads.");

    }

    private static ushort WallWord(int x, int y, bool solid)
    {
        ushort value = y switch
        {
            0 or 6 or 7 => 0x0080,
            1 or 3 => checked((ushort)(0x0107 + x)),
            2 or 4 => checked((ushort)(0x0127 + x)),
            5 => checked((ushort)(0x0147 + x)),
            _ => throw new ArgumentOutOfRangeException(nameof(y)),
        };
        return solid ? (ushort)(value | 0x8000) : value;
    }

    private static void VerifyCrocomireMutation(PlmHeaderId header,
        Func<int, int, ushort> expectedWord)
    {
        const int width = 32;
        const int height = 16;
        ushort[] sourceWords = Enumerable.Repeat((ushort)0x8123,
            width * height).ToArray();
        RoomLevelData level = CreateRoom(width, height, sourceWords,
            new byte[sourceWords.Length], blockDefinitions: new byte[0x400 * 8]);
        var plms = new RoomPlmSystem();
        AssertTrue(plms.TrySpawnCrocomireArenaMutation(level, 5, 3, header),
            $"Crocomire header ${(int)header:X4} allocates");
        var guard = new CrocomireSourceGuard(new TestAddressSpace());
        BackgroundTilemapStreamer streamer = level.CreateBackgroundStreamer();
        plms.Step(guard, level, streamer, 0, 0, 0);
        AssertEqual(1, plms.ActiveCount,
            $"Crocomire header ${(int)header:X4} persists for native draw frame");
        for (int y = 0; y < 9; y++)
        for (int x = 0; x < 11; x++)
            AssertEqual(expectedWord(x, y),
                level.GetCollisionBlockByIndex((3 + y) * width + 5 + x).LevelWord,
                $"Crocomire header ${(int)header:X4} physical block ({x},{y})");
        plms.Step(guard, level, streamer, 0, 0, 0);
        AssertEqual(0, plms.ActiveCount,
            $"Crocomire header ${(int)header:X4} deletes on next frame");
        AssertEqual(0, guard.ForbiddenReadAttempts,
            $"Crocomire header ${(int)header:X4} reads no migrated ROM records");
    }

    private sealed class CrocomireSourceGuard(ISnesAddressSpace source)
        : ISnesAddressSpace, IImportCartridgeSource
    {
        internal int ForbiddenReadAttempts { get; private set; }

        public byte ReadCartridgeByte(int address) => ReadByte(address);

        public byte ReadByte(int address)
        {
            int pointer = address & 0xffff;
            if ((address >> 16) == 0x84 &&
                ((pointer >= CrocomireArenaPlmProgramDefinitions.ClearBridge &&
                  pointer < CrocomireArenaPlmProgramDefinitions.EndExclusive) ||
                 (pointer >= (ushort)CrocomireArenaDraw.ClearBridge &&
                  pointer < CrocomireArenaPlmDrawDefinitions.EndExclusive)))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Crocomire PLM reread compiled source ${address:X6}.");
            }
            return source.ReadByte(address);
        }

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
