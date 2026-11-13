using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.AssetExtraction.Cartridge;
using static SuperMetroid.Verification.Frontend.Map.MapWindowMotionDefinitionsVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Frontend.FileSelect;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.TestSupport.FrameRendering;

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
        ushort[] used = Enumerable.Repeat(ushort.MaxValue, SuperMetroid.Core.Progression.AreaIds.Zebes.Count).ToArray();
        var native = new FileSelectAreaMapGraphics(SuperMetroid.Core.Progression.AreaId.Crateria, original.Backdrops, MenuSheetFixtures.For(original));
        var installed = new FileSelectAreaMapGraphics(SuperMetroid.Core.Progression.AreaId.Crateria, original.Backdrops, MenuSheetFixtures.For(original));
        native.BindLabels(original.Labels);
        installed.BindLabels(original.Labels);
        foreach (SuperMetroid.Core.Progression.AreaId areaId in SuperMetroid.Core.Progression.AreaIds.Zebes)
        {
            int area = SuperMetroid.AssetExtraction.Progression.AreaNumberCodec.Encode(areaId);
            native.SelectArea(areaId); installed.SelectArea(areaId);
            // The two scenes' pixels are compared through their descriptions, drawn only when those differ (#1532).
            AssertTrue(ScenePixels.Same(native.Scene(used), installed.Scene(used)), "stock label pixels with position ROM blocked");
            var nativeWindow = new FileSelectMapWindow(areaId,
                ReadCartridgeMapWindowMotion(bus, area), original.Labels);
            var installedWindow = new FileSelectMapWindow(areaId, original.Labels);
            while (!nativeWindow.IsComplete)
            {
                AssertEqual(new VerifyWorldMapLabelsCase(nativeWindow.Left, nativeWindow.Top, nativeWindow.Right, nativeWindow.Bottom),
                    new VerifyWorldMapLabelsCase(installedWindow.Left, installedWindow.Top, installedWindow.Right, installedWindow.Bottom),
                    "stock window edges");
                AssertEqual(nativeWindow.Step(), installedWindow.Step(), "stock window completion timing");
            }
        }
        Console.WriteLine("World-map labels: native pixels/window timing and blocked ROM reads pass.");
    }

    /// <summary>Blocks migrated label positions and window motion tables while allowing unrelated cartridge reads.</summary>
    private static MutableCartridgeReadGuard NewWorldLabelReadGuard(ISnesAddressSpace source)
    {
        // The native label and window tables hold one record per Zebes area.
        const int ZebesAreaRecords = 6;
        var guard = new MutableCartridgeReadGuard(source);
        guard.Forbid(address =>
                address is (>= MapSourceDefinitions.FileSelectMap.LabelPositions
                        and < MapSourceDefinitions.FileSelectMap.LabelPositions + ZebesAreaRecords * 4)
                    or (>= FileSelectMapNativeSources.WindowVelocities
                        and < FileSelectMapNativeSources.WindowVelocities + ZebesAreaRecords * FileSelectMapNativeSources.VelocityRecordBytes)
                    or (>= FileSelectMapNativeSources.WindowTimers
                        and < FileSelectMapNativeSources.WindowTimers + ZebesAreaRecords * 2),
            _ => "Installed world labels/windows read cartridge coordinate or motion tables.");
        return guard;
    }
}
