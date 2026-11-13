using SuperMetroid.AssetExtraction.Frontend.Map.Artwork;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Assets;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using static SuperMetroid.Verification.Frontend.InstalledFileSelectMapVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Frontend.FileSelect;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Rendering;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
using SuperMetroid.Core.Frontend.Menus;
using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.TestSupport.FrameRendering;

namespace SuperMetroid.Verification.Frontend.Map;

/// <summary>Checks installed file-select world and room screens against native presentation.</summary>
internal static class MapScreensVerification
{
    /// <summary>Compares six areas' VRAM and pixels with native output.</summary>
    internal static void VerifyMapScreens(ISnesAddressSpace bus, AreaMapPresentation original)
    {
        var guard = NewMapScreenGuard(bus);
        var system = new SharedGameState(new SharedRandom());
        system.Progress.ExploreEveryCell();
        for (int index = 0; index < MapScreenDefinitions.ZebesAreas; index++)
        {
            var native = World(bus, original, index);
            var installed = World(guard, original, index);
            var shownScreen = MenuScreenFixtures.Vram(native, original);
            byte[] usedNativeVram = shownScreen.Bytes.ToArray();
            var shownScreen2 = MenuScreenFixtures.Vram(installed, original);
            AssertTrue(usedNativeVram.SequenceEqual(shownScreen2.Bytes),
                "stock world resources reproduce native VRAM");
            foreach (bool backdropMath in new[] { false, true })
                AssertTrue(ScenePixels.Same(native.BackgroundsScene(backdropMath), installed.BackgroundsScene(backdropMath)), "all six world layers preserve native additive pixels");
            installed.SelectArea(AreaIds.Zebes[(index + 1) % AreaIds.Zebes.Count]);
            installed.SelectArea(AreaIds.Zebes[index]);
            AssertTrue(ScenePixels.Same(native.BackgroundsScene(), installed.BackgroundsScene()), "reselection reloads authored world background without ROM reads");
            AreaId area = AreaIds.Zebes[index];
            var nativeRoom = new FileSelectRoomMapGraphics(system, area,
                mapPresentation: original, sheets: MenuSheetFixtures.For(original));
            var installedRoom = new FileSelectRoomMapGraphics(system, area, mapPresentation: original, sheets: MenuSheetFixtures.For(original));
            var shownScreen3 = MenuScreenFixtures.Vram(nativeRoom, original);
            var shownScreen4 = MenuScreenFixtures.Vram(installedRoom, original);
            AssertTrue(shownScreen3.ImageEquals(shownScreen4), "stock resolved frame/footer/area label matches full native VRAM");
            AssertTrue(ScenePixels.Same(nativeRoom.FrameOnlyScene(), installedRoom.FrameOnlyScene()), "native frame-only transition pixels match");
            foreach (ushort scroll in new ushort[] { 0, 8, 127, 255 })
                AssertTrue(ScenePixels.Same(nativeRoom.BackgroundsScene(new(scroll, scroll)), installedRoom.BackgroundsScene(new(scroll, scroll))), "fixed room frame remains exact while map scrolls");
        }
        Suite(nameof(VerifyInstalledFileSelectMenu), () => VerifyInstalledFileSelectMenu(original, verifyCapturedRendering: true));
        Console.WriteLine("Map screen resources: six native world/room VRAM and pixel comparisons and the guarded full menu pass.");

        static FileSelectAreaMapGraphics World(ISnesAddressSpace addressSpace, AreaMapPresentation catalog, int area) =>
            new(AreaIds.Zebes[area], catalog.Backdrops, MenuSheetFixtures.ForFileSelectMap(InstalledFixtures.ImportOf(catalog)));
    }

    /// <summary>Forbids runtime cartridge reads across all migrated world pages, room frames, labels, and map character sheets.</summary>
    private static MutableCartridgeReadGuard NewMapScreenGuard(ISnesAddressSpace source)
    {
        IImportCartridgeSource cartridge = CartridgeImportSource.Require(source);
        var forbidden = new HashSet<int>();
        Add(MapSourceDefinitions.Menu.Background, MapScreenDefinitions.PageBytes);
        Add(MapSourceDefinitions.FileSelectMap.AreaForeground, MapScreenDefinitions.PageBytes);
        Add(MapSourceDefinitions.FileSelectMap.AreaBackgrounds, MapScreenDefinitions.PageBytes * MapScreenDefinitions.ZebesAreas);
        Add(MapSourceDefinitions.Menu.ForegroundCharacters, WorldMapArtworkFormat.ForegroundBytes);
        Add(MapSourceDefinitions.Menu.BackgroundCharacters, WorldMapArtworkFormat.BackgroundBytes);
        Add(MapSourceDefinitions.FileSelectMap.RoomFrame, 1600);
        Add(MapSourceDefinitions.FileSelectMap.RoomFrameFooter, 322);
        Add(MapSourceDefinitions.FileSelectMap.AreaLabelPointers, MapScreenDefinitions.ZebesAreas * 2);
        for (int area = 0; area < MapScreenDefinitions.ZebesAreas; area++)
            Add(MapSourceDefinitions.Menu.ObjectBank | RomDataReader.ReadWordFixedBank(cartridge, MapSourceDefinitions.FileSelectMap.AreaLabelPointers + area * 2), 24);
        void Add(int start, int length) { for (int offset = 0; offset < length; offset++) forbidden.Add(start + offset); }
        var guard = new MutableCartridgeReadGuard(source);
        guard.Forbid(forbidden.Contains, address => $"Installed map screens read cartridge presentation at {address:X6}.");
        return guard;
    }
}
