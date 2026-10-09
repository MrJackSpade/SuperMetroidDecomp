using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using SuperMetroid.AssetExtraction;

internal static partial class Program
{
    /// <summary>Verifies the compiled Crocomire arena PLM programs, physical draw mappings, and bridge or wall mutations.</summary>
    private static void VerifyCompiledCrocomireArenaPlms()
    {
        var rom = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(
            Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SuperMetroid.AssetExtraction.SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Crocomire native oracle revision");
        Suite(nameof(VerifyCrocomireProgramControls), () => VerifyCrocomireProgramControls(rom));
        Suite(nameof(VerifyCrocomireProgramDraws), () => VerifyCrocomireProgramDraws(rom));

        Suite(nameof(VerifyCrocomirePhysicalDrawMapping), () => VerifyCrocomirePhysicalDrawMapping(rom));

        Suite(nameof(VerifyCrocomireMutation), () => VerifyCrocomireMutation(RoomPlmHeaders.ClearCrocomireBridge,
            (x, y) => y == 0 && x < 10 ? (ushort)0x0080 : (ushort)0x8123));
        Suite(nameof(VerifyCrocomireMutation), () => VerifyCrocomireMutation(RoomPlmHeaders.CrumbleCrocomireBridgeBlock,
            (x, y) => x == 0 && y == 0 ? (ushort)0x810b : (ushort)0x8123));
        Suite(nameof(VerifyCrocomireMutation), () => VerifyCrocomireMutation(RoomPlmHeaders.ClearCrocomireBridgeBlock,
            (x, y) => x == 0 && y == 0 ? (ushort)0x0080 : (ushort)0x8123));
        Suite(nameof(VerifyCrocomireMutation), () => VerifyCrocomireMutation(RoomPlmHeaders.ClearCrocomireInvisibleWall,
            (x, y) => x < 3 && y < 8 ? WallWord(x, y, false) : (ushort)0x8123));
        Suite(nameof(VerifyCrocomireMutation), () => VerifyCrocomireMutation(RoomPlmHeaders.CreateCrocomireInvisibleWall,
            (x, y) => x < 3 && y < 8 ? WallWord(x, y, true) : (ushort)0x8123));
        Console.WriteLine("Crocomire PLMs: five programs and physical draws match ROM; all mutations execute without source reads.");

    }

    /// <summary>Builds the expected tile word for one cell in the Crocomire invisible-wall pattern.</summary>
    /// <param name="x">Zero-based column within the three-block-wide wall.</param>
    /// <param name="y">Zero-based row within the eight-block-high wall.</param>
    /// <param name="solid">Whether to set the collision bit on the selected tile word.</param>
    /// <returns>The tile index and optional solid-collision flag for the requested cell.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The row is outside the wall pattern.</exception>
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

    /// <summary>Runs one Crocomire arena mutation PLM and checks every affected physical room word and its source-read guard.</summary>
    /// <param name="header">Compiled Crocomire mutation PLM header to spawn.</param>
    /// <param name="expectedWord">Oracle returning the expected level word for each block offset from the mutation origin.</param>
    private static void VerifyCrocomireMutation(ushort header,
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
            $"Crocomire header ${header:X4} allocates");
        var guard = new CrocomireSourceGuard(new TestAddressSpace());
        BackgroundTilemapStreamer streamer = level.CreateBackgroundStreamer();
        plms.Step(guard, level, streamer, 0, 0, 0);
        AssertEqual(1, plms.ActiveCount,
            $"Crocomire header ${header:X4} persists for native draw frame");
        for (int y = 0; y < 9; y++)
        for (int x = 0; x < 11; x++)
            AssertEqual(expectedWord(x, y),
                level.GetCollisionBlockByIndex((3 + y) * width + 5 + x).LevelWord,
                $"Crocomire header ${header:X4} physical block ({x},{y})");
        plms.Step(guard, level, streamer, 0, 0, 0);
        AssertEqual(0, plms.ActiveCount,
            $"Crocomire header ${header:X4} deletes on next frame");
        AssertEqual(0, guard.ForbiddenReadAttempts,
            $"Crocomire header ${header:X4} reads no migrated ROM records");
    }

    /// <summary>Blocks cartridge reads from the compiled Crocomire program and draw-data ranges.</summary>
    /// <param name="source">Address space used for all reads and writes outside the guarded bank-$84 ranges.</param>
    private sealed class CrocomireSourceGuard(ISnesAddressSpace source)
        : ISnesAddressSpace, IImportCartridgeSource
    {
        /// <summary>Counts attempted reads of compiled Crocomire PLM program or draw records.</summary>
        internal int ForbiddenReadAttempts { get; private set; }

        /// <summary>Routes cartridge access through the compiled-source guard.</summary>
        /// <param name="address">Cartridge address requested by the PLM system.</param>
        /// <returns>The wrapped source byte when the address is permitted.</returns>
        public byte ReadCartridgeByte(int address) => ReadByte(address);

        /// <summary>Rejects reads in the compiled Crocomire program and draw ranges, forwarding other addresses.</summary>
        /// <param name="address">Address requested during PLM execution.</param>
        /// <returns>The wrapped source byte for an allowed address.</returns>
        /// <exception cref="InvalidOperationException">The address targets compiled Crocomire PLM source data.</exception>
        public byte ReadByte(int address)
        {
            int pointer = address & 0xffff;
            if ((address >> 16) == 0x84 &&
                ((pointer >= CrocomireArenaPlmProgramDefinitions.ClearBridge &&
                  pointer < CrocomireArenaPlmProgramDefinitions.EndExclusive) ||
                 (pointer >= CrocomireArenaPlmDrawDefinitions.ClearBridge &&
                  pointer < CrocomireArenaPlmDrawDefinitions.EndExclusive)))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Crocomire PLM reread compiled source ${address:X6}.");
            }
            return source.ReadByte(address);
        }

        /// <summary>Forwards writes to the wrapped address space.</summary>
        /// <param name="address">Address to update.</param>
        /// <param name="value">Byte to store at that address.</param>
        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
