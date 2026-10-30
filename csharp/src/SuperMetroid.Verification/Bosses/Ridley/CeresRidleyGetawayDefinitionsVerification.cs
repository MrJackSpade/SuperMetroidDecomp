using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Areas.Ceres;
using SuperMetroid.Core.Bosses.Ridley;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.VerificationImporters;
using SuperMetroid.Core.Bosses.Ridley.Artwork;
using SuperMetroid.AssetExtraction.Areas.Ceres;
using SuperMetroid.AssetExtraction.Bosses.Ridley;

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
            var frame = CeresRidleyGetawayCurve.FromIndex(index);
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
            AssertThrows<InvalidDataException>(() => CeresRidleyGetawayCurve.FromIndex(invalid), "Ceres invalid curve index");
        Suite(nameof(VerifyCompiledCeresRidleyMode7Transfers), () => VerifyCompiledCeresRidleyMode7Transfers(rom));
        Console.WriteLine("Ceres Ridley compiled getaway: all 337 native curve words match, including irregular zoom steps and terminal frame.");
    }

    /// <summary>
    /// Checks the seven floor animation lists (#1467): the import's runs write exactly the cells and tiles the cartridge's
    /// lists write, and the installed floor updates record each getaway run's source, count and floor cell.
    /// </summary>
    internal static void VerifyCompiledCeresRidleyMode7Transfers(SuperMetroidAddressSpace rom)
    {
        int checkedTransfers = 0;
        CeresRidleyFloorUpdateSources installed = CeresRidleyFloorUpdateFiles.Read(new MemoryStream(CeresRidleyFloorUpdateFiles.Extract(rom)));
        // The sources the getaway's updates record are installed (#1415): the import decodes the cartridge's descriptors
        // to the values Core's catalog held before, the capsule's first run at $A6:AD1B and the wing's at $A6:ADB7.
        AssertEqual(0xad1b, installed.Updates(CeresFloorAnimationFrame.BabyFrame0)[0].TileSource, "the capsule's first run source the import decodes");
        AssertEqual(0xadb7, installed.Updates(CeresFloorAnimationFrame.WingFrame0)[0].TileSource, "the wing's first run source the import decodes");
        foreach (CeresFloorAnimationFrame pointer in Enum.GetValues<CeresFloorAnimationFrame>())
        {
            int cursor = 0xa60000 | CeresNativeWords.FloorAnimationFrames.Word(pointer);
            bool getawayFrame = pointer is not (CeresFloorAnimationFrame.ElevatorLight or CeresFloorAnimationFrame.ElevatorDark);
            int run = 0;
            var expected = new Dictionary<int, byte>();
            while (rom.ReadCartridgeByte(cursor) != 0)
            {
                AssertEqual((byte)0x80, rom.ReadCartridgeByte(cursor), "Ceres floor native transfer control");
                int source = CartridgeImportSource.Require(rom).ReadCartridgeLong(cursor + 1);
                ushort size = CartridgeImportSource.Require(rom).ReadCartridgeWord(cursor + 4);
                ushort destination = CartridgeImportSource.Require(rom).ReadCartridgeWord(cursor + 6);
                AssertEqual((byte)0, rom.ReadCartridgeByte(cursor + 8), "Ceres floor native VRAM increment mode");
                if (getawayFrame)
                    AssertEqual(new FloorUpdate(source & ushort.MaxValue, size, destination), installed.Updates(pointer)[run],
                        $"Ceres {pointer} run {run} installed floor update");
                run++;
                for (int index = 0; index < size; index++)
                    expected[destination + index] = rom.ReadCartridgeByte(source + index);
                cursor += 9;
                checkedTransfers++;
            }
            if (getawayFrame)
                AssertEqual(run, installed.Updates(pointer).Count, $"Ceres {pointer} installs every run's floor update");
            IReadOnlyDictionary<int, int> drawn = CeresFloorDrawings.Cells(pointer);
            AssertTrue(drawn.Count == expected.Count && expected.All(cell => drawn.TryGetValue(cell.Key, out int tile) && tile == cell.Value),
                $"Ceres {pointer}'s picture writes exactly the cells and tiles its list writes");
        }
        AssertThrows<InvalidDataException>(() =>
            CeresFloorAnimation.PictureOf((CeresFloorAnimationFrame)0xffff),
            "Ceres unknown floor animation frame fails loudly");
        Console.WriteLine($"Ceres floor animation: {checkedTransfers} native transfers across all seven lists match the pictured cells and installed updates.");
    }
}