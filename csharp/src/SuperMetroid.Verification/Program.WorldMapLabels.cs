using System.Text.Json;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rom;

internal static partial class Program
{
    private static void VerifyWorldMapLabels(ISnesAddressSpace bus, string stock, string overrides, AreaMapPresentationCatalog original)
    {
        var guard = new WorldLabelReadGuard(bus);
        ushort[] used = Enumerable.Repeat(ushort.MaxValue, FileSelectMapRomData.AreaCount).ToArray();
        var native = new FileSelectAreaMapGraphics(bus, 0);
        var installed = new FileSelectAreaMapGraphics(guard, 0);
        installed.BindLabels(original.Labels);
        for (int area = 0; area < FileSelectMapRomData.AreaCount; area++)
        {
            int address = FileSelectMapRomData.LabelPositions + area * 4;
            AssertEqual(RomDataReader.ReadWordFixedBank(bus, address), original.Labels.Get(area).X, "stock label X");
            AssertEqual(RomDataReader.ReadWordFixedBank(bus, address + 2), original.Labels.Get(area).Y, "stock label Y");
            native.SelectArea(area); installed.SelectArea(area);
            AssertTrue(native.Render(used).AsSpan().SequenceEqual(installed.Render(used)), "stock label pixels with position ROM blocked");
            var nativeWindow = new FileSelectMapWindow(bus, area, ReadCartridgeMapWindowMotion(bus, area));
            var installedWindow = new FileSelectMapWindow(guard, area, original.Labels);
            while (!nativeWindow.IsComplete)
            {
                AssertEqual((nativeWindow.Left, nativeWindow.Right, nativeWindow.Top, nativeWindow.Bottom),
                    (installedWindow.Left, installedWindow.Right, installedWindow.Top, installedWindow.Bottom), "stock window edges");
                AssertEqual(nativeWindow.Step(), installedWindow.Step(), "stock window completion timing");
            }
        }
        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        var document = JsonSerializer.Deserialize<WorldMapLabelDocument>(File.ReadAllBytes(Path.Combine(stock, WorldMapLabelFormat.FileName)), options)!;
        var areas = new Dictionary<string, MapLabelPoint>(document.Areas);
        areas["Crateria"] = areas["Crateria"] with { X = areas["Crateria"].X + 8 };
        Directory.CreateDirectory(overrides);
        string replacement = Path.Combine(overrides, WorldMapLabelFormat.FileName);
        using (var stream = File.Create(replacement)) WorldMapLabelLayout.Write(stream, document with { Areas = areas });
        byte[] bytes = File.ReadAllBytes(replacement);
        var edited = AreaMapPresentationCatalog.Load(stock, overrides);
        AssertTrue(original.ContentIdentity != edited.ContentIdentity, "label edit invalidates content identity");
        installed.SelectArea(0); native.SelectArea(0);
        installed.BindLabels(edited.Labels);
        AssertTrue(!native.Render(used).AsSpan().SequenceEqual(installed.Render(used)), "label edit changes real rendered pixels");
        AssertTrue(native.Render(new ushort[6]).AsSpan().SequenceEqual(installed.Render(new ushort[6])), "layout edit does not reveal unavailable areas");
        var navigation = new FileSelectMapNavigation(guard, 0);
        navigation.BindLabels(edited.Labels);
        navigation.Step((ushort)SnesButton.A); navigation.Step(0);
        AssertEqual(areas["Crateria"].X, navigation.Window!.Left, "normal navigation uses edited label/window anchor");
        AssertEqual(areas["Crateria"].Y, navigation.Window.Top, "normal navigation uses edited vertical anchor");
        installed.BindLabels(original.Labels);
        AssertTrue(native.Render(used).AsSpan().SequenceEqual(installed.Render(used)), "rebind restores stock label pixels");
        string rebuilt = Path.Combine(overrides, "rebuilt");
        SuperMetroid.AssetExtraction.MapPresentationExtractor.Extract(bus, rebuilt, "test-provenance");
        AssertEqual(edited.ContentIdentity, AreaMapPresentationCatalog.Load(rebuilt, overrides).ContentIdentity, "label override survives stock replacement");
        AssertTrue(bytes.AsSpan().SequenceEqual(File.ReadAllBytes(replacement)), "label override bytes preserved");
        foreach (var invalid in new[] { document with { Version = 2 }, document with { Areas = new() },
            document with { Areas = areas.ToDictionary(pair => pair.Key, pair => pair.Value with { X = -1 }) } })
            AssertThrows<System.IO.InvalidDataException>(() => WorldMapLabelLayout.Write(new MemoryStream(), invalid), "invalid label schema/area/coordinate rejected");
        File.WriteAllText(replacement, "{bad labels");
        AssertThrows<System.IO.InvalidDataException>(() => AreaMapPresentationCatalog.Load(stock, overrides), "bad label override cannot silently fall back");
        AssertEqual("{bad labels", File.ReadAllText(replacement), "bad label override remains for repair");
        Console.WriteLine("World-map labels: native pixels/window timing, blocked ROM reads, visible edits, availability isolation and override preservation pass.");
    }

    private sealed class WorldLabelReadGuard(ISnesAddressSpace source) : ISnesAddressSpace
    {
        public byte ReadByte(int address) =>
            (address >= FileSelectMapRomData.LabelPositions && address < FileSelectMapRomData.LabelPositions + FileSelectMapRomData.AreaCount * 4) ||
            (address >= FileSelectMapRomData.WindowVelocities && address < FileSelectMapRomData.WindowVelocities + FileSelectMapRomData.AreaCount * FileSelectMapRomData.VelocityRecordBytes) ||
            (address >= FileSelectMapRomData.WindowTimers && address < FileSelectMapRomData.WindowTimers + FileSelectMapRomData.AreaCount * 2)
            ? throw new InvalidOperationException("Installed world labels/windows read cartridge coordinate or motion tables.") : source.ReadByte(address);
        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
