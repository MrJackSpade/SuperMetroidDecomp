using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Areas.Ceres;
using SuperMetroid.Core.Bosses.Ridley;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.VerificationImporters;
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.Bosses.Ridley.Artwork;
using SuperMetroid.AssetExtraction.Bosses.Ridley;

using SuperMetroid.AssetExtraction.Video;
namespace SuperMetroid.Verification.Bosses.Ridley;

/// <summary>Checks Ceres Ridley's getaway curve and Mode 7 transfer lists against retail data.</summary>
internal static class CeresRidleyGetawayDefinitionsVerification
{
    /// <summary>Compares zoom and both velocity components through the terminal frame.</summary>
    internal static void VerifyCompiledCeresRidleyGetaway(SuperMetroidAddressSpace rom)
    {
        for (int index = 0; index <= 112; index++)
        {
            int offset = index * 2;
            var frame = CeresRidleyGetaway.FromIndex(index);
            ushort nativeZoom = CartridgeImportSource.Require(rom).ReadCartridgeWord(0xa6ae4d + offset);
            if (offset == 224)
            {
                // The terminator is a finished frame in Core (#1413); the oracle confirms the cartridge ends its zoom
                // table with the -1 word there and nowhere earlier.
                AssertEqual(ushort.MaxValue, nativeZoom, "Ceres getaway native terminator");
                AssertTrue(frame.IsFinished, "Ceres getaway terminator ends the getaway");
                continue;
            }
            AssertTrue(!frame.IsFinished, "Ceres getaway authored frames do not end the getaway");
            AssertEqual(nativeZoom, frame.Zoom, "Ceres getaway exact native zoom");
            AssertEqual(unchecked((short)CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa6af2f + offset))), frame.Velocity.Y, "Ceres getaway exact native Y increment");
            AssertEqual(unchecked((short)CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa6b00f + offset))), frame.Velocity.X, "Ceres getaway exact native X subtraction");
        }
        foreach (int invalid in new[] { -1, 113, int.MaxValue })
            AssertThrows<InvalidDataException>(() => CeresRidleyGetaway.FromIndex(invalid), "Ceres invalid curve index");
        Suite(nameof(VerifyCompiledCeresRidleyMode7Transfers), () => VerifyCompiledCeresRidleyMode7Transfers(rom));
        Console.WriteLine("Ceres Ridley compiled getaway: all 337 native curve words match, including irregular zoom steps and terminal frame.");
    }

    /// <summary>Checks seven compiled transfer lists and their low-byte-only VRAM effects.</summary>
    internal static void VerifyCompiledCeresRidleyMode7Transfers(SuperMetroidAddressSpace rom)
    {
        int checkedTransfers = 0;
        CeresRidleyFloorUpdateSources installed = CeresRidleyFloorUpdateFiles.Read(new MemoryStream(CeresRidleyFloorUpdateFiles.Extract(rom)));
        // The sources the getaway's updates record are installed (#1415): the import decodes the cartridge's descriptors
        // to the values Core's catalog held before, the capsule's first run at $A6:AD1B and the wing's at $A6:ADB7.
        AssertEqual(0xad1b, installed.TileSource(CeresFloorAnimationFrame.BabyFrame0, 0), "the capsule's first run source the import decodes");
        AssertEqual(0xadb7, installed.TileSource(CeresFloorAnimationFrame.WingFrame0, 0), "the wing's first run source the import decodes");
        foreach (CeresFloorAnimationFrame pointer in Enum.GetValues<CeresFloorAnimationFrame>())
        {
            CeresFloorAnimation.FrameRuns compiled =
                CeresFloorAnimation.Get(pointer);
            int cursor = 0xa60000 | CeresNativeWords.FloorAnimationFrames.Word(pointer);
            bool getawayFrame = pointer is not (CeresFloorAnimationFrame.ElevatorLight or CeresFloorAnimationFrame.ElevatorDark);
            int run = 0;
            var expectedVram = new byte[SnesVram.ByteCount];
            Array.Fill(expectedVram, (byte)0x5a);
            var seed = new byte[FloorMap.Cells];
            for (int cell = 0; cell < seed.Length; cell++)
                seed[cell] = expectedVram[cell * 2];
            var actual = new FloorMap(seed, default, RoomBackgroundsFixtures.FloorTiles());
            foreach (CeresFloorTileRun transfer in compiled)
            {
                AssertEqual((byte)0x80, rom.ReadCartridgeByte(cursor),
                    "Ceres Mode 7 native transfer control");
                int source = (CartridgeImportSource.Require(rom).ReadCartridgeLong(cursor + 1));
                ushort size = CartridgeImportSource.Require(rom).ReadCartridgeWord(cursor + 4);
                ushort destination = CartridgeImportSource.Require(rom).ReadCartridgeWord(cursor + 6);
                AssertEqual((byte)0, rom.ReadCartridgeByte(cursor + 8),
                    "Ceres Mode 7 native VRAM increment mode");
                AssertEqual(size, transfer.TileNumbers.Length,
                    "Ceres Mode 7 compiled byte count");
                AssertEqual(destination, transfer.FloorCell,
                    "Ceres Mode 7 compiled VRAM destination");
                if (getawayFrame)
                    AssertEqual(source & ushort.MaxValue, installed.TileSource(pointer, run),
                        $"Ceres {pointer} run {run} installed floor update source");
                run++;
                for (int index = 0; index < size; index++)
                {
                    byte value = rom.ReadCartridgeByte(source + index);
                    AssertEqual(value, transfer.TileNumbers[index],
                        "Ceres Mode 7 compiled tilemap source byte");
                    expectedVram[((destination + index) & 0x7fff) * 2] = value;
                }
                cursor += 9;
                checkedTransfers++;
            }
            AssertEqual((byte)0, rom.ReadCartridgeByte(cursor),
                "Ceres Mode 7 compiled list reaches native terminator");
            // Ridley writes each transfer into the room's Mode 7 map, in list order (#1349).
            foreach (CeresFloorTileRun transfer in CeresFloorAnimation.Get(pointer))
                actual.Write(transfer.FloorCell, [.. transfer.TileNumbers]);
            byte[] cells = [.. actual.Publish().Cells.Select(cell => (byte)cell.Tile)];
            bool matches = true;
            for (int cell = 0; cell < cells.Length; cell++)
                matches &= cells[cell] == expectedVram[cell * 2];
            AssertTrue(matches, "Ceres Mode 7 compiled list writes its map cells in order");
        }
        AssertThrows<InvalidDataException>(() =>
            CeresFloorAnimation.Get((CeresFloorAnimationFrame)0xffff),
            "Ceres Mode 7 unknown transfer list fails loudly");
        Console.WriteLine($"Ceres compiled Mode 7: {checkedTransfers} native transfers across all seven lists match source bytes, destinations, and VRAM effects.");
    }
}
