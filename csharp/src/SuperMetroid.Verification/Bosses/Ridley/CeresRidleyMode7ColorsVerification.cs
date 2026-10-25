using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.Core.Samus.Palettes;
using System.Text.Json;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Bosses.Ridley;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Bosses.Ridley.Artwork;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
using SuperMetroid.Core.Gameplay;
using SuperMetroid.Core.World;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Bosses.Ridley;
using SuperMetroid.AssetExtraction.Frontend.Map;

namespace SuperMetroid.Verification.Bosses.Ridley;

/// <summary>Checks editable Ceres Ridley Mode 7 zoom palettes and their runtime CGRAM binding.</summary>
internal static class CeresRidleyMode7ColorsVerification
{
    /// <summary>Compares native zoom rows, all getaway frames, override repair, and source-read blocking.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native CGRAM slots")]
    internal static void VerifyCeresRidleyMode7ColorOverride(string stockDirectory,
        string overrideDirectory, AreaMapPresentationCatalog original, ISnesAddressSpace rom,
        GameplayBasePaletteCatalog initialPalettes)
    {
        byte[] extracted = SuperMetroid.AssetExtraction.Bosses.Ridley.CeresRidleyMode7ColorExtractor.Extract(rom);
        CeresRidleyFloorColorCatalog native = CeresRidleyMode7ColorFile.Read(
            new MemoryStream(extracted, writable: false));
        var forbidden = new HashSet<int>();
        for (int row = 0; row < CeresRidleyPaletteDefinitions.GetawayZoomRowCount; row++)
        {
            ushort nativePointer = unchecked((ushort)(
                row * RidleySourceDefinitions.Ceres.GetawayZoomRowBytes - 0x4ef9));
            int rowAddress = RidleySourceDefinitions.Ceres.GetawayZoomColors +
                row * RidleySourceDefinitions.Ceres.GetawayZoomRowBytes;
            AssertEqual(0xa60000 | nativePointer, rowAddress,
                $"Mode-7 row {row} matches cartridge wrapped source expression");
            for (int color = 0; color < CeresRidleyPaletteDefinitions.GetawayZoomColorCount; color++)
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
        for (int row = 0; row < document.ZoomRows.Length; row++)
        {
            PaletteRgb5 previous = document.ZoomRows[row][1];
            document.ZoomRows[row][1] = previous with
            {
                Blue = previous.Blue == 31 ? 30 : previous.Blue + 1,
            };
        }
        string replacement = Path.Combine(overrideDirectory, CeresRidleyFloorColorFormat.FileName);
        File.WriteAllBytes(replacement, CeresRidleyMode7ColorFile.Write(document));
        AreaMapPresentationCatalog edited = MapPresentationFiles.Load(stockDirectory, overrideDirectory);
        AssertTrue(edited.ContentIdentity != original.ContentIdentity,
            "Ceres Mode-7 color edit changes installed identity");
        var runtime = SuperMetroid.Core.Runtime.SuperMetroidRuntime.Create(HostGameplayOverrides.None, initialPaletteArt: initialPalettes);
        runtime.Artwork.MapPresentation = edited;
        AssertTrue(ReferenceEquals(edited.CeresRidleyFloorColors,
            runtime.BossArtwork.CeresRidleyFloorColors),
            "installed runtime binds Ceres Mode-7 colors to enemy owner");
        runtime.Artwork.MapPresentation = original;
        AssertTrue(ReferenceEquals(original.CeresRidleyFloorColors,
            runtime.BossArtwork.CeresRidleyFloorColors),
            "runtime rebind drops old Ceres Mode-7 colors");

        var guard = NewCeresRidleyMode7ColorReadGuard(rom, forbidden);
        var enemies = new RoomEnemySystem(new EnemySession(), definitions: CompiledRoomEnemyDefinitions.Instance) { BossArtworkBinding = { CeresRidleyFloorColors = edited.CeresRidleyFloorColors } };
        var cgram = new SamusColourRig();
        enemies.Presentation.BindColours(cgram.Gameplay);
        Action<ushort> select = zoom => enemies.CeresRidleyBehavior.Getaway.UpdateCeresRidleyFloorPalette(zoom);
        var visited = new HashSet<int>();
        for (int index = 0; index < 112; index++)
        {
            CeresRidleyGetawayFrame frame = CeresRidleyGetawayDefinitions.FromIndex(index);
            int row = SnesLittleEndian.HighByte(frame.Zoom);
            visited.Add(row);
            select(checked((ushort)frame.Zoom));
            for (int color = 0; color < CeresRidleyPaletteDefinitions.GetawayZoomColorCount; color++)
                AssertEqual(edited.CeresRidleyFloorColors.Resolve(row, color),
                    cgram.Colors[CeresRidleyPaletteDefinitionsConstants.Mode7ZoomCgramIndex + color],
                    $"Ceres Mode-7 frame {index}, row {row}, color {color}");
        }
        AssertEqual(CeresRidleyPaletteDefinitions.GetawayZoomRowCount, visited.Count,
            "native getaway selects every extracted zoom row");
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "installed Mode-7 zoom cycle avoids source-color ROM reads");
        foreach (ushort unusualZoom in new ushort[] { 0x0900, 0xff00 })
        {
            AssertThrows<InvalidOperationException>(() => select(unusualZoom),
                $"non-catalogued zoom ${unusualZoom:X4} fails without a cartridge fallback");
        }

        document.ZoomRows[0][0] = document.ZoomRows[0][0] with { Green = 32 };
        AssertThrows<InvalidDataException>(() => CeresRidleyMode7ColorFile.Write(document),
            "Ceres Mode-7 colors reject invalid RGB5 values");
        string replacementStock = Path.Combine(
            Path.GetDirectoryName(stockDirectory) ?? throw new InvalidOperationException("Stock maps have no parent."),
            "ceres-mode7-reextract");
        byte[] overrideBeforeRepair = File.ReadAllBytes(replacement);
        SuperMetroid.AssetExtraction.Frontend.Map.MapPresentationExtractor.Extract(rom, replacementStock, "test-provenance");
        AreaMapPresentationCatalog repaired = MapPresentationFiles.Load(replacementStock, overrideDirectory);
        AssertEqual(edited.ContentIdentity, repaired.ContentIdentity,
            "stock re-extraction preserves Ceres Mode-7 override");
        AssertTrue(overrideBeforeRepair.SequenceEqual(File.ReadAllBytes(replacement)),
            "stock re-extraction does not rewrite Ceres Mode-7 colors");
        File.Delete(replacement);
        AssertEqual(original.ContentIdentity,
            MapPresentationFiles.Load(stockDirectory, overrideDirectory).ContentIdentity,
            "removing Ceres Mode-7 override restores stock identity");
        Console.WriteLine("Ceres Mode-7 colors: 135 stock words and all 112 getaway frames match, with every source color guarded and override repair preserved.");
    }

    /// <summary>Forbids native color bytes while the installed getaway palette cycle runs.</summary>
    private static CartridgeReadGuard NewCeresRidleyMode7ColorReadGuard(ISnesAddressSpace source, HashSet<int> forbidden) =>
        new MutableCartridgeReadGuard(source)
            .Forbid(forbidden.Contains, address => $"Ceres Mode-7 color reread ${address:X6}.");

    /// <summary>Reads one native zoom color for the extracted-palette comparison.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for Ceres Ridley's native Mode-7 zoom colors")]
    private static ushort ReadNativeCeresRidleyMode7Color(ISnesAddressSpace rom, int address) =>
        RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(rom), address);
}
