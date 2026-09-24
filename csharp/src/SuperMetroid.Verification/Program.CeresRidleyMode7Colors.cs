using System.Reflection;
using System.Text.Json;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;
using SuperMetroid.Core.Runtime;

internal static partial class Program
{
    private static void VerifyCeresRidleyMode7ColorOverride(string stockDirectory,
        string overrideDirectory, AreaMapPresentationCatalog original, ISnesAddressSpace rom)
    {
        byte[] extracted = SuperMetroid.AssetExtraction.CeresRidleyMode7ColorExtractor.Extract(rom);
        CeresRidleyMode7ColorCatalog native = CeresRidleyMode7ColorCatalog.Load(
            new MemoryStream(extracted, writable: false));
        var forbidden = new HashSet<int>();
        for (int row = 0; row < CeresRidleyPaletteRomData.Mode7ZoomRowCount; row++)
        {
            ushort nativePointer = unchecked((ushort)(
                row * CeresRidleyPaletteRomData.Mode7ZoomRowByteStride - 0x4ef9));
            int rowAddress = CeresRidleyPaletteRomData.Mode7ZoomColors +
                row * CeresRidleyPaletteRomData.Mode7ZoomRowByteStride;
            AssertEqual(0xa60000 | nativePointer, rowAddress,
                $"Mode-7 row {row} matches cartridge wrapped source expression");
            for (int color = 0; color < CeresRidleyPaletteRomData.Mode7ZoomColorCount; color++)
            {
                int address = rowAddress + color * sizeof(ushort);
                forbidden.Add(address);
                forbidden.Add(address + 1);
                AssertEqual(RomDataReader.ReadWordFixedBank(rom, address), native.Resolve(row, color),
                    $"Ceres Ridley Mode-7 zoom row {row}, color {color} matches cartridge");
            }
        }

        CeresRidleyMode7ColorDocument document =
            JsonSerializer.Deserialize<CeresRidleyMode7ColorDocument>(
                File.ReadAllBytes(Path.Combine(stockDirectory, CeresRidleyMode7ColorFormat.FileName)),
                MapPresentationFormat.JsonOptions)
            ?? throw new InvalidDataException("Stock Ceres Mode-7 colors are null.");
        for (int row = 0; row < document.ZoomRows.Length; row++)
        {
            PaletteRgb5 previous = document.ZoomRows[row][1];
            document.ZoomRows[row][1] = previous with
            {
                Blue = previous.Blue == 31 ? 30 : previous.Blue + 1,
            };
        }
        string replacement = Path.Combine(overrideDirectory, CeresRidleyMode7ColorFormat.FileName);
        File.WriteAllBytes(replacement, CeresRidleyMode7ColorCatalog.Write(document));
        AreaMapPresentationCatalog edited = AreaMapPresentationCatalog.Load(stockDirectory, overrideDirectory);
        AssertTrue(edited.ContentIdentity != original.ContentIdentity,
            "Ceres Mode-7 color edit changes installed identity");
        var runtime = new SuperMetroidRuntime(rom) { MapPresentation = edited };
        AssertTrue(ReferenceEquals(edited.CeresRidleyMode7Colors,
            runtime.Enemies.CeresRidleyMode7Colors),
            "installed runtime binds Ceres Mode-7 colors to enemy owner");
        runtime.MapPresentation = original;
        AssertTrue(ReferenceEquals(original.CeresRidleyMode7Colors,
            runtime.Enemies.CeresRidleyMode7Colors),
            "runtime rebind drops old Ceres Mode-7 colors");

        var guard = new CeresRidleyMode7ColorReadGuard(rom, forbidden);
        var enemies = new RoomEnemySystem { CeresRidleyMode7Colors = edited.CeresRidleyMode7Colors };
        var cgram = new SnesCgram();
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, guard);
        typeof(RoomEnemySystem).GetField("_cgram", flags)!.SetValue(enemies, cgram);
        var select = typeof(RoomEnemySystem).GetMethod("UpdateCeresRidleyMode7Palette", flags)!
            .CreateDelegate<Action<ushort>>(enemies);
        var visited = new HashSet<int>();
        for (ushort byteIndex = 0; byteIndex < 224; byteIndex += 2)
        {
            CeresRidleyGetawayFrame frame = CeresRidleyGetawayDefinitions.FromByteIndex(byteIndex);
            int row = frame.Zoom >> 8;
            visited.Add(row);
            select(frame.Zoom);
            for (int color = 0; color < CeresRidleyPaletteRomData.Mode7ZoomColorCount; color++)
                AssertEqual(edited.CeresRidleyMode7Colors.Resolve(row, color),
                    cgram.Colors[CeresRidleyPaletteRomData.Mode7ZoomCgramIndex + color],
                    $"Ceres Mode-7 frame {byteIndex / 2}, row {row}, color {color}");
        }
        AssertEqual(CeresRidleyPaletteRomData.Mode7ZoomRowCount, visited.Count,
            "native getaway selects every extracted zoom row");
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "installed Mode-7 zoom cycle avoids source-color ROM reads");
        foreach (ushort unusualZoom in new ushort[] { 0x0900, 0xff00 })
        {
            select(unusualZoom);
            ushort nativePointer = unchecked((ushort)(
                (unusualZoom >> 8) * CeresRidleyPaletteRomData.Mode7ZoomRowByteStride -
                0x4ef9));
            for (int color = 0; color < CeresRidleyPaletteRomData.Mode7ZoomColorCount; color++)
            {
                int source = 0xa60000 | unchecked((ushort)(nativePointer + color * sizeof(ushort)));
                AssertEqual(RomDataReader.ReadWordFixedBank(rom, source),
                    cgram.Colors[CeresRidleyPaletteRomData.Mode7ZoomCgramIndex + color],
                    $"non-catalogued zoom ${unusualZoom:X4} retains native wrapped read");
            }
        }

        document.ZoomRows[0][0] = document.ZoomRows[0][0] with { Green = 32 };
        AssertThrows<InvalidDataException>(() => CeresRidleyMode7ColorCatalog.Write(document),
            "Ceres Mode-7 colors reject invalid RGB5 values");
        string replacementStock = Path.Combine(
            Path.GetDirectoryName(stockDirectory) ?? throw new InvalidOperationException("Stock maps have no parent."),
            "ceres-mode7-reextract");
        byte[] overrideBeforeRepair = File.ReadAllBytes(replacement);
        SuperMetroid.AssetExtraction.MapPresentationExtractor.Extract(rom, replacementStock, "test-provenance");
        AreaMapPresentationCatalog repaired = AreaMapPresentationCatalog.Load(replacementStock, overrideDirectory);
        AssertEqual(edited.ContentIdentity, repaired.ContentIdentity,
            "stock re-extraction preserves Ceres Mode-7 override");
        AssertTrue(overrideBeforeRepair.AsSpan().SequenceEqual(File.ReadAllBytes(replacement)),
            "stock re-extraction does not rewrite Ceres Mode-7 colors");
        File.Delete(replacement);
        AssertEqual(original.ContentIdentity,
            AreaMapPresentationCatalog.Load(stockDirectory, overrideDirectory).ContentIdentity,
            "removing Ceres Mode-7 override restores stock identity");
        Console.WriteLine("Ceres Mode-7 colors: 135 stock words and all 112 getaway frames match, with every source color guarded and override repair preserved.");
    }

    private sealed class CeresRidleyMode7ColorReadGuard(ISnesAddressSpace inner,
        HashSet<int> forbidden) : ISnesAddressSpace
    {
        public int ForbiddenReadAttempts { get; private set; }
        public byte ReadByte(int address)
        {
            if (forbidden.Contains(address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException($"Ceres Mode-7 color reread ${address:X6}.");
            }
            return inner.ReadByte(address);
        }
        public void WriteByte(int address, byte value) => inner.WriteByte(address, value);
    }
}
