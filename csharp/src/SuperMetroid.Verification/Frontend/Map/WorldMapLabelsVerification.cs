using SuperMetroid.Core.Frontend.Artwork;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.AssetExtraction.Cartridge;
using static SuperMetroid.Verification.Frontend.Map.MapWindowMotionDefinitionsVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Frontend.FileSelect;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.TestSupport.FrameRendering;

namespace SuperMetroid.Verification.Frontend.Map;

/// <summary>Checks installed world-map labels and opening windows against native pixels and motion.</summary>
internal static class WorldMapLabelsVerification
{
    /// <summary>Verifies stock output with ROM coordinates blocked.</summary>
    internal static void VerifyWorldMapLabels(ISnesAddressSpace bus, InstalledScreenContent original)
    {
        Suite(nameof(VerifyMapWindowOriginX), () => VerifyMapWindowOriginX(bus, original.Maps.Labels));
        Suite(nameof(VerifyMapWindowOriginY), () => VerifyMapWindowOriginY(bus, original.Maps.Labels));
        var guard = NewWorldLabelReadGuard(bus);
        ushort[] used = Enumerable.Repeat(ushort.MaxValue, SuperMetroid.Core.Progression.AreaIds.Zebes.Count).ToArray();
        var native = new FileSelectAreaMapGraphics(SuperMetroid.Core.Progression.AreaId.Crateria, original, MenuSheetFixtures.For(original));
        var installed = new FileSelectAreaMapGraphics(SuperMetroid.Core.Progression.AreaId.Crateria, original, MenuSheetFixtures.For(original));
        foreach (SuperMetroid.Core.Progression.AreaId areaId in SuperMetroid.Core.Progression.AreaIds.Zebes)
        {
            int area = SuperMetroid.AssetExtraction.Progression.AreaNumberCodec.Encode(areaId);
            native.SelectArea(areaId); installed.SelectArea(areaId);
            // The two scenes' pixels are compared through their descriptions, drawn only when those differ (#1532).
            AssertTrue(ScenePixels.Same(native.Scene(used), installed.Scene(used)), "stock label pixels with position ROM blocked");
            // The area's reveal moves its window by the motion the cartridge's tables give it: the window starts at the
            // area's label either way, so the same motion makes the same edges and completion timing.
            AssertEqual(ReadCartridgeMapWindowMotion(bus, area),
                FileSelectMapWindowMotions.Get(SuperMetroid.Core.Progression.ZebesAreaFixtures.Of(areaId)),
                $"area {areaId}'s reveal motion");
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
