using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Tooling;
using System.Text.Json;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Input;
using static SuperMetroid.Verification.Frontend.Map.MapWindowMotionDefinitionsVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Frontend.FileSelect;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
using SuperMetroid.AssetExtraction.Frontend.Map;
using SuperMetroid.AssetExtraction.CartridgeTables;

namespace SuperMetroid.Verification.Frontend.Map;

/// <summary>Checks installed world-map labels and opening windows against native pixels and motion.</summary>
internal static class WorldMapLabelsVerification
{
    /// <summary>Verifies stock output with ROM coordinates blocked, then checks editable anchors and override preservation.</summary>
    internal static void VerifyWorldMapLabels(ISnesAddressSpace bus, string stock, string overrides, AreaMapPresentationCatalog original)
    {
        Suite(nameof(VerifyMapWindowOriginX), () => VerifyMapWindowOriginX(bus, original.Labels));
        Suite(nameof(VerifyMapWindowOriginY), () => VerifyMapWindowOriginY(bus, original.Labels));
        var guard = NewWorldLabelReadGuard(bus);
        ushort[] used = Enumerable.Repeat(ushort.MaxValue, FileSelectMapDefinitions.AreaCount).ToArray();
        var native = new FileSelectAreaMapGraphics(0, original.Tiles,
            original.Palettes, original.Screens, original.WorldArtwork, original.Sprites, MenuSheetFixtures.For(original));
        var installed = new FileSelectAreaMapGraphics(0, original.Tiles,
            original.Palettes, original.Screens, original.WorldArtwork, original.Sprites, MenuSheetFixtures.For(original));
        native.BindLabels(original.Labels);
        installed.BindLabels(original.Labels);
        for (int area = 0; area < FileSelectMapDefinitions.AreaCount; area++)
        {
            native.SelectArea(area); installed.SelectArea(area);
            AssertTrue(native.Render(used).SequenceEqual(installed.Render(used)), "stock label pixels with position ROM blocked");
            var nativeWindow = new FileSelectMapWindow(area,
                ReadCartridgeMapWindowMotion(bus, area), original.Labels);
            var installedWindow = new FileSelectMapWindow(area, original.Labels);
            while (!nativeWindow.IsComplete)
            {
                AssertEqual(new VerifyWorldMapLabelsCase(nativeWindow.TopLeft, nativeWindow.BottomRight),
                    new VerifyWorldMapLabelsCase(installedWindow.TopLeft, installedWindow.BottomRight), "stock window edges");
                AssertEqual(nativeWindow.Step(), installedWindow.Step(), "stock window completion timing");
            }
        }
        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        var document = RequiredJson.Deserialize<WorldMapLabelDocument>(File.ReadAllBytes(Path.Combine(stock, WorldMapLabelFormat.FileName)), options);
        var areas = new Dictionary<string, Position16<SpritePixels>>(document.Areas);
        areas["Crateria"] = areas["Crateria"] with { X = (ushort)(areas["Crateria"].X + 8) };
        Directory.CreateDirectory(overrides);
        string replacement = Path.Combine(overrides, WorldMapLabelFormat.FileName);
        using (var stream = File.Create(replacement)) WorldMapLabelLayout.Write(stream, document with { Areas = areas });
        byte[] bytes = File.ReadAllBytes(replacement);
        var edited = MapPresentationFiles.Load(stock, overrides);
        AssertTrue(original.ContentIdentity != edited.ContentIdentity, "label edit invalidates content identity");
        installed.SelectArea(0); native.SelectArea(0);
        installed.BindLabels(edited.Labels);
        AssertTrue(!native.Render(used).SequenceEqual(installed.Render(used)), "label edit changes real rendered pixels");
        AssertTrue(native.Render(new ushort[6]).SequenceEqual(installed.Render(new ushort[6])), "layout edit does not reveal unavailable areas");
        var navigation = new FileSelectMapNavigation(0);
        navigation.BindLabels(edited.Labels);
        navigation.Step(ControllerButton.A); navigation.Step(0);
        AssertEqual(areas["Crateria"].X, (navigation.Window ?? throw new InvalidOperationException("Verification requires navigation.Window.")).TopLeft.X, "normal navigation uses edited label/window anchor");
        AssertEqual(areas["Crateria"].Y, navigation.Window.TopLeft.Y, "normal navigation uses edited vertical anchor");
        installed.BindLabels(original.Labels);
        AssertTrue(native.Render(used).SequenceEqual(installed.Render(used)), "rebind restores stock label pixels");
        string rebuilt = Path.Combine(overrides, "rebuilt");
        SuperMetroid.AssetExtraction.Frontend.Map.MapPresentationExtractor.Extract(bus, rebuilt, "test-provenance");
        AssertEqual(edited.ContentIdentity, MapPresentationFiles.Load(rebuilt, overrides).ContentIdentity, "label override survives stock replacement");
        AssertTrue(bytes.SequenceEqual(File.ReadAllBytes(replacement)), "label override bytes preserved");
        foreach (var invalid in new[] { document with { Version = 2 }, document with { Areas = new() },
            document with { Areas = areas.ToDictionary(pair => pair.Key, pair => pair.Value with { X = ushort.MaxValue }) } })
            AssertThrows<System.IO.InvalidDataException>(() => WorldMapLabelLayout.Write(new MemoryStream(), invalid), "invalid label schema/area/coordinate rejected");
        File.WriteAllText(replacement, "{bad labels");
        AssertThrows<System.IO.InvalidDataException>(() => MapPresentationFiles.Load(stock, overrides), "bad label override cannot silently fall back");
        AssertEqual("{bad labels", File.ReadAllText(replacement), "bad label override remains for repair");
        Console.WriteLine("World-map labels: native pixels/window timing, blocked ROM reads, visible edits, availability isolation and override preservation pass.");
    }

    /// <summary>Blocks migrated label positions and window motion tables while allowing unrelated cartridge reads.</summary>
    private static MutableCartridgeReadGuard NewWorldLabelReadGuard(ISnesAddressSpace source)
    {
        var guard = new MutableCartridgeReadGuard(source);
        guard.Forbid(address =>
                address is (>= MapSourceDefinitions.FileSelectMap.LabelPositions
                        and < MapSourceDefinitions.FileSelectMap.LabelPositions + FileSelectMapDefinitions.AreaCount * 4)
                    or (>= FileSelectMapNativeSources.WindowVelocities
                        and < FileSelectMapNativeSources.WindowVelocities + FileSelectMapDefinitions.AreaCount * FileSelectMapNativeSources.VelocityRecordBytes)
                    or (>= FileSelectMapNativeSources.WindowTimers
                        and < FileSelectMapNativeSources.WindowTimers + FileSelectMapDefinitions.AreaCount * 2),
            _ => "Installed world labels/windows read cartridge coordinate or motion tables.");
        return guard;
    }
}
