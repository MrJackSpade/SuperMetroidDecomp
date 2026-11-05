using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Assets;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using static SuperMetroid.Verification.Frontend.Map.MapPresentationVerification;
using SuperMetroid.Core.Frontend.FileSelect;
using SuperMetroid.Core.Frontend.Pause;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Frontend.Map;
using SuperMetroid.AssetExtraction.CartridgeTables;

namespace SuperMetroid.Verification.Frontend.Map;

/// <summary>Checks installed map atlas bytes and the pause render path.</summary>
internal static class MapAtlasVerification
{
    /// <summary>Compares stock tile bytes with ROM and the installed atlas's pause pixels with native.</summary>
    internal static void VerifyMapAtlasIntegration(ISnesAddressSpace bus,
        AreaMapPresentation original, ImportedAreaMap[] rules)
    {
        byte[] source = RomDataReader.ReadFixedBank(CartridgeImportSource.Require(bus), MapSourceDefinitions.Artwork.MapCharacters, MapTileAtlasFormat.ByteCount);
        var vram = new SnesVram();
        MapCharacterArtwork.Of(original).Map.LoadTo(new VramRegion(vram, PpuMemoryLayout.Vram).Slice(0 / sizeof(ushort), MapTileAtlasFormat.ByteCount / sizeof(ushort)));
        for (int i = 0; i < source.Length; i++) AssertEqual(source[i], vram.ReadByte(i), "stock PNG recompiles exact map tile bytes");
        var guard = NewMapDataGuard(bus, rules);
        var system = new SharedGameState();
        system.Progress.ExploreEveryCell();
        var menu = new FileSelectRoomMapGraphics(system, AreaId.Crateria, mapPresentation: original, sheets: MenuSheetFixtures.For(original));
        var samus = new SamusState(InstalledFixtures.AnimationContinuation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence());
        var pause = new PauseMenuState(new PauseMenuEntry { Samus = samus, System = system, AreaIndex = AreaId.Crateria, RoomMapPosition = new(0, 0), MapPresentation = original, SpriteSheets = MenuSheetFixtures.For(original) });
        var nativePause = new PauseMenuState(new PauseMenuEntry { Samus = samus, System = system, AreaIndex = AreaId.Crateria, RoomMapPosition = new(0, 0), MapPresentation = original, SpriteSheets = MenuSheetFixtures.For(original) });
        RgbaPixel[] pauseBefore = pause.Render(), menuBefore = menu.RenderBackgrounds(new(0, 0)).ToArray();
        AssertTrue(pauseBefore.SequenceEqual(nativePause.Render()), "installed stock atlas gives exact pause pixels with artwork ROM blocked");
        Console.WriteLine("Map PNG atlas: exact stock bytes and pause pixels pass.");
    }
}
