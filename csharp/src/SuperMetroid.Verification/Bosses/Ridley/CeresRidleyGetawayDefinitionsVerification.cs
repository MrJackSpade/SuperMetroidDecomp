using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Areas.Ceres;
using SuperMetroid.Core.Bosses.Ridley;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.VerificationImporters;
using SuperMetroid.AssetExtraction.Areas.Ceres;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.Core.Rooms.Backgrounds;

namespace SuperMetroid.Verification.Bosses.Ridley;

/// <summary>Checks Ceres Ridley's getaway curve and Mode 7 transfer lists against retail data.</summary>
internal static class CeresRidleyGetawayDefinitionsVerification
{
    /// <summary>Compares the zoom and drift of every update of the getaway, and its duration, with the cartridge's tables.</summary>
    internal static void VerifyCompiledCeresRidleyGetaway(CartridgeImportAddressSpace rom)
    {
        // The cartridge ends its zoom table with a -1 word after the getaway's last update and nowhere earlier; the
        // curves last exactly that long (#2247).
        AssertEqual(112, CeresRidleyGetawayCurve.Duration, "Ceres getaway lasts its curves' 112 updates");
        for (int index = 0; index <= CeresRidleyGetawayCurve.Duration; index++)
        {
            int offset = index * 2;
            ushort nativeZoom = CartridgeImportSource.Require(rom).ReadCartridgeWord(0xa6ae4d + offset);
            if (index == CeresRidleyGetawayCurve.Duration)
            {
                AssertEqual(ushort.MaxValue, nativeZoom, "Ceres getaway native terminator follows the last update");
                continue;
            }
            AssertTrue(nativeZoom != ushort.MaxValue, "Ceres getaway native table holds no terminator before the last update");
            AssertEqual(FloorZoomConstants.FromTwoHundredFiftySixths(nativeZoom), CeresRidleyGetawayCurve.Steps[index].Zoom, "Ceres getaway exact native zoom");
            Offset16<FloorPixels> drift = CeresRidleyGetawayCurve.Steps[index].Drift;
            AssertEqual(unchecked((short)CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa6af2f + offset))), drift.Y, "Ceres getaway exact native Y increment");
            AssertEqual(unchecked((short)CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa6b00f + offset))), drift.X, "Ceres getaway exact native X subtraction");
        }
        Suite(nameof(VerifyCompiledCeresRidleyMode7Transfers), () => VerifyCompiledCeresRidleyMode7Transfers(rom));
        Console.WriteLine("Ceres Ridley compiled getaway: all 337 native curve words match the zoom and drift curves, including irregular zoom steps and the terminator after the last update.");
    }

    /// <summary>
    /// Checks the seven floor animation lists (#1467): the import's runs write exactly the cells and tiles the cartridge's
    /// lists write. The delays the wing frames leave behind are checked by the stage-five residue suite (#1479).
    /// </summary>
    internal static void VerifyCompiledCeresRidleyMode7Transfers(CartridgeImportAddressSpace rom)
    {
        int checkedTransfers = 0;
        foreach (CeresFloorActorPicture pointer in Enum.GetValues<CeresFloorActorPicture>())
        {
            int cursor = 0xa60000 | CeresNativeWords.FloorAnimationFrames.Word(pointer);
            var expected = new Dictionary<int, byte>();
            while (rom.ReadCartridgeByte(cursor) != 0)
            {
                AssertEqual((byte)0x80, rom.ReadCartridgeByte(cursor), "Ceres floor native transfer control");
                int source = CartridgeImportSource.Require(rom).ReadCartridgeLong(cursor + 1);
                ushort size = CartridgeImportSource.Require(rom).ReadCartridgeWord(cursor + 4);
                ushort destination = CartridgeImportSource.Require(rom).ReadCartridgeWord(cursor + 6);
                AssertEqual((byte)0, rom.ReadCartridgeByte(cursor + 8), "Ceres floor native VRAM increment mode");
                for (int index = 0; index < size; index++)
                    expected[destination + index] = rom.ReadCartridgeByte(source + index);
                cursor += 9;
                checkedTransfers++;
            }
            Dictionary<int, int> drawn = CeresFloorDrawings.Cells(pointer);
            AssertTrue(drawn.Count == expected.Count && expected.All(cell => drawn.TryGetValue(cell.Key, out int tile) && tile == cell.Value),
                $"Ceres {pointer}'s picture writes exactly the cells and tiles its list writes");
        }
        AssertThrows<InvalidDataException>(() =>
            RoomFloorSheet.PictureNameOf((CeresFloorActorPicture)0xffff),
            "Ceres unknown floor animation frame fails loudly");
        Console.WriteLine($"Ceres floor animation: {checkedTransfers} native transfers across all seven lists match the pictured cells and installed updates.");
    }
}