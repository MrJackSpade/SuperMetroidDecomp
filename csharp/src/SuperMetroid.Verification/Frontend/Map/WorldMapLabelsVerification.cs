using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.AssetExtraction.Cartridge;
using static SuperMetroid.Verification.Frontend.Map.MapWindowMotionDefinitionsVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Frontend.FileSelect;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
using SuperMetroid.AssetExtraction.CartridgeTables;

namespace SuperMetroid.Verification.Frontend.Map;

/// <summary>Checks installed world-map labels and opening windows against native pixels and motion.</summary>
internal static class WorldMapLabelsVerification
{
    /// <summary>Verifies stock output with ROM coordinates blocked.</summary>
    internal static void VerifyWorldMapLabels(ISnesAddressSpace bus, AreaMapPresentation original)
    {
        Suite(nameof(VerifyMapWindowOriginX), () => VerifyMapWindowOriginX(bus, original.Labels));
        Suite(nameof(VerifyMapWindowOriginY), () => VerifyMapWindowOriginY(bus, original.Labels));
        var guard = NewWorldLabelReadGuard(bus);
        ushort[] used = Enumerable.Repeat(ushort.MaxValue, FileSelectMapDefinitions.AreaCount).ToArray();
        var native = new FileSelectAreaMapGraphics(0, original.Backdrops, original.WorldArtwork, MenuSheetFixtures.For(original));
        var installed = new FileSelectAreaMapGraphics(0, original.Backdrops, original.WorldArtwork, MenuSheetFixtures.For(original));
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
        Console.WriteLine("World-map labels: native pixels/window timing and blocked ROM reads pass.");
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
