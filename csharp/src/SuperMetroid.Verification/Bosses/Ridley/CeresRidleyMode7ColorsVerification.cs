using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.AssetExtraction.CartridgeTables;
using System.Text.Json;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Bosses.Ridley;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
using SuperMetroid.Core.Gameplay;
using SuperMetroid.AssetExtraction.Bosses.Ridley;
using SuperMetroid.AssetExtraction.Bosses.Ridley.Artwork;
using SuperMetroid.AssetExtraction;

namespace SuperMetroid.Verification.Bosses.Ridley;

/// <summary>Checks installed Ceres Ridley Mode 7 zoom palettes and their runtime CGRAM binding.</summary>
internal static class CeresRidleyMode7ColorsVerification
{
    /// <summary>Compares native zoom rows, all getaway frames, and source-read blocking.</summary>
    internal static void VerifyCeresRidleyMode7Colors(string stockDirectory,
        AreaMapPresentation original, ISnesAddressSpace rom)
    {
        byte[] extracted = SuperMetroid.AssetExtraction.Bosses.Ridley.CeresRidleyMode7ColorExtractor.Extract(rom);
        CeresRidleyFloorColorCatalog native = CeresRidleyMode7ColorFile.Read(
            new MemoryStream(extracted, writable: false));
        var forbidden = new HashSet<int>();
        for (int row = 0; row < CeresRidleyPaletteDefinitions.GetawayZoomStepCount; row++)
        {
            ushort nativePointer = unchecked((ushort)(
                row * RidleySourceDefinitions.Ceres.GetawayZoomRowBytes - 0x4ef9));
            int rowAddress = RidleySourceDefinitions.Ceres.GetawayZoomColors +
                row * RidleySourceDefinitions.Ceres.GetawayZoomRowBytes;
            AssertEqual(0xa60000 | nativePointer, rowAddress,
                $"Mode-7 row {row} matches cartridge wrapped source expression");
            for (int color = 0; color < CeresRidleyColorFormat.GetawayZoomColorCount; color++)
            {
                int address = rowAddress + color * sizeof(ushort);
                forbidden.Add(address);
                forbidden.Add(address + 1);
                AssertEqual(ReadNativeCeresRidleyMode7Color(rom, address), native.Resolve(row, color),
                    $"Ceres Ridley Mode-7 zoom row {row}, color {color} matches cartridge");
            }
        }

        CeresRidleyMode7ColorDocument document =
            JsonSerializer.Deserialize<CeresRidleyMode7ColorDocument>(
                File.ReadAllBytes(Path.Combine(stockDirectory, CeresRidleyFloorColorFormat.FileName)),
                ArtworkJson.Options)
            ?? throw new InvalidDataException("Stock Ceres Mode-7 colors are null.");
        var runtime = SuperMetroid.Core.Runtime.SuperMetroidRuntime.Create(GameplayAssists.None, InstalledFixtures.BareRuntimeArtwork);
        runtime.Artwork.MapPresentation = original;

        var guard = NewCeresRidleyMode7ColorReadGuard(rom, forbidden);
        var ridley = new RidleyEnemyState();
        Action<ushort> select = selectedZoom => CeresRidleyGetaway.UpdateCeresRidleyFloorZoomColours(ridley, selectedZoom);
        var visited = new HashSet<int>();
        for (int index = 0; index < 112; index++)
        {
            CeresRidleyGetawayFrame frame = CeresRidleyGetawayCurve.FromIndex(index);
            int row = LittleEndianBytes.HighByte(frame.Zoom);
            visited.Add(row);
            select(checked((ushort)frame.Zoom));
            // The colours the encounter shows are the import's shades of the installed colours, laid as the getaway laid them (#1512).
            RgbColor[] cgram = RidleyLaidColours.Ceres(ridley, original);
            for (int color = 0; color < CeresRidleyColorFormat.GetawayZoomColorCount; color++)
                AssertEqual(InstalledFixtures.ImportOf(original).CeresRidleyFloorColors.Resolve(row, color),
                    cgram[CeresRidleyPaletteDefinitionsConstants.Mode7ZoomCgramIndex + color],
                    $"Ceres Mode-7 frame {index}, row {row}, color {color}");
        }
        AssertEqual(CeresRidleyPaletteDefinitions.GetawayZoomStepCount, visited.Count,
            "native getaway selects every extracted zoom row");
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "installed Mode-7 zoom cycle avoids source-color ROM reads");
        foreach (ushort unusualZoom in new ushort[] { 0x0900, 0xff00 })
        {
            AssertThrows<InvalidOperationException>(() => select(unusualZoom),
                $"non-catalogued zoom ${unusualZoom:X4} fails without a cartridge fallback");
        }

        document.ZoomRows[0][0] = document.ZoomRows[0][0] with { Green = 32 };
        AssertThrows<InvalidDataException>(() => CeresRidleyMode7ColorFile.Read(new MemoryStream(CeresRidleyMode7ColorFile.Write(document), writable: false)),
            "reading Ceres Mode-7 colors rejects invalid RGB5 values");
        Console.WriteLine("Ceres Mode-7 colors: 135 stock words and all 112 getaway frames match, with every source color guarded.");
    }

    /// <summary>Forbids native color bytes while the installed getaway palette cycle runs.</summary>
    private static CartridgeReadGuard NewCeresRidleyMode7ColorReadGuard(ISnesAddressSpace source, HashSet<int> forbidden) =>
        new MutableCartridgeReadGuard(source)
            .Forbid(forbidden.Contains, address => $"Ceres Mode-7 color reread ${address:X6}.");

    /// <summary>Reads one native zoom color for the extracted-palette comparison.</summary>
    private static ushort ReadNativeCeresRidleyMode7Color(ISnesAddressSpace rom, int address) =>
        RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(rom), address);
}
