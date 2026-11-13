using SuperMetroid.TestSupport.MovieReplay;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Map.Artwork;
using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.Core.Frontend.Menus;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Frontend.InstalledFileSelectMapVerification;
using static SuperMetroid.Verification.Frontend.Map.MapSpriteCasesVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Frontend.FileSelect;
using SuperMetroid.Core.Frontend.Pause;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
using SuperMetroid.AssetExtraction.Frontend.Map;
using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.Core.Rendering;
using SuperMetroid.TestSupport.FrameRendering;

namespace SuperMetroid.Verification.Frontend.Map;

/// <summary>Checks imported map sprites in file-select and pause presentation.</summary>
internal static class MapSpritesVerification
{
    /// <summary>
    /// The sprite set the cartridge draws a map sprite's palette-inheriting parts in, by the shading a file-select draw in
    /// that colouring publishes (#1512): every colouring the map draws in, the highlighted sprites before the cycle's first
    /// step, and the highlight cycle's first frame.
    /// </summary>
    internal static IReadOnlyDictionary<PictureShading, int> MapSpriteSets { get; } = new Dictionary<PictureShading, int>
    {
        [PictureShading.Shaded(MapSpriteShades.Drawn(MapSpriteScreen.FileSelect, MapSpriteColouring.Labels))] = 0,
        [PictureShading.Shaded(MapSpriteShades.Drawn(MapSpriteScreen.FileSelect, MapSpriteColouring.DimLabels))] = 1,
        [PictureShading.Shaded(MapSpriteShades.Drawn(MapSpriteScreen.FileSelect, MapSpriteColouring.Highlighted))] = 3,
        [PictureShading.Shaded(MapSpriteShades.Highlight(0))] = 3,
        [PictureShading.Shaded(MapSpriteShades.Drawn(MapSpriteScreen.FileSelect, MapSpriteColouring.DefeatedBossMarker))] = 6,
        [PictureShading.Shaded(MapSpriteShades.Drawn(MapSpriteScreen.FileSelect, MapSpriteColouring.Markers))] = 7,
    };

    /// <summary>The shading of every colouring the map draws its sprites in.</summary>
    internal static IReadOnlyList<PictureShading> MapSpriteShadings { get; } = [.. MapSpriteSets.Keys];

    /// <summary>Compares native sprite bytes and clipping, and the installed sprites through rendered maps.</summary>
    internal static void VerifyMapSprites(ISnesAddressSpace bus, AreaMapPresentation original)
    {
        var native = new OamBuffer();
        var installed = new OamBuffer();
        Suite(nameof(VerifyMapSpriteNativeCompositions), () => VerifyMapSpriteNativeCompositions(bus, InstalledFixtures.ImportOf(original).Sprites));
        // Independent coordinate oracle: the native carry/sign test parks wrapped
        // Y at $180/$E0, rather than letting it reappear on the opposite screen edge.
        for (int origin = 0; origin < 256; origin++)
        for (int offset = 0; offset < 256; offset++)
        {
            installed.BeginFrame();
            installed.AddOnScreenSpritePart(SnesSpritemapXWord.Create(-1, true), new SnesSpritemapYByte((byte)offset),
                SnesObjAttributeWord.Create(3, 2, 1, PictureFlip.Horizontal), new Position16<SpritePixels>(0, (ushort)origin));
            int sum = origin + offset;
            int signedY = origin + unchecked((sbyte)offset);
            bool hidden = signedY is < -32 or >= 224;
            AssertEqual(hidden ? (byte)128 : (byte)255, installed.LowTable[0], "typed sprite X clipping");
            AssertEqual(hidden ? (byte)224 : (byte)sum, installed.LowTable[1], "typed sprite Y clipping");
            AssertEqual((byte)3, (byte)(installed.HighTable[0] & 3), "typed sprite preserves high X and large size");
        }
        var vram = new SnesVram(); InstalledFixtures.ImportOf(original).Sprites.LoadArtworkTo(new VramRegion(vram, PpuMemoryLayout.Vram).Slice(MenuVramLayout.MapSpriteByteAddress / sizeof(ushort), MapSpriteFormat.ByteCount / sizeof(ushort)));
        AssertTrue(vram.Bytes.Slice(MenuVramLayout.MapSpriteByteAddress, MapSpriteFormat.ByteCount).SequenceEqual(
            RomDataReader.ReadFixedBank(CartridgeImportSource.Require(bus), MapSourceDefinitions.Menu.ObjectCharacters, MapSpriteFormat.ByteCount)), "map OBJ PNG round-trips native characters exactly");
        Suite(nameof(VerifyRenderedSprites), () => VerifyRenderedSprites());
        Console.WriteLine("Map sprites: 26 native compositions, clipping/capacity/palette parity and guarded file-select/pause sprite pixels pass.");

        void VerifyRenderedSprites()
        {
            var guard = NewMapSpriteGuard(bus);
            var system = new SharedGameState(new SharedRandom()); system.SetAreaMapAcquired(AreaId.WreckedShip);
            system.Progress.ExploreEveryCell();
            var marker = new FileSelectStationMarker(LoadStationDefinitions.WreckedShipSave,
                original.SaveMarkers);
            var room = new FileSelectRoomMapGraphics(system, AreaId.WreckedShip, mapPresentation: original, sheets: MenuSheetFixtures.For(original));
            var nativeRoom = new FileSelectRoomMapGraphics(system,
                AreaId.WreckedShip, mapPresentation: original, sheets: MenuSheetFixtures.For(original));
            // Each comparison draws only scenes whose descriptions differ (#1532).
            LayeredScene before = nativeRoom.CaptureRenderSnapshot(new(0, 0), marker);
            AssertTrue(ScenePixels.Same(before, room.CaptureRenderSnapshot(new(0, 0), marker)), "stock map sprite pixels match with source reads blocked");
            // The capture names the installed content by path, as a host's store does, instead of copying it (#1532).
            room = SuperMetroid.Verification.Harness.GraphDigestVerification.RoundTrip(room);
            room.BindFixtureArtwork(original);
            AssertTrue(ScenePixels.Same(before, room.CaptureRenderSnapshot(new(0, 0), marker)), "restored sprite resources keep the installed pixels");
            var pauseNative = new PauseMenuState(new PauseMenuEntry { Samus = new SamusState(InstalledFixtures.AnimationContinuation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState()), System = system, AreaIndex = AreaId.WreckedShip, RoomMapPosition = new(19, 20), MapPresentation = original, SpriteSheets = MenuSheetFixtures.For(original) });
            var pause = new PauseMenuState(new PauseMenuEntry { Samus = new SamusState(InstalledFixtures.AnimationContinuation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState()), System = system, AreaIndex = AreaId.WreckedShip, RoomMapPosition = new(19, 20), MapPresentation = original, SpriteSheets = MenuSheetFixtures.For(original) });
            AssertTrue(ScenePixels.Same(pauseNative.CaptureRenderSnapshot(), pause.CaptureRenderSnapshot()), "pause map sprite pixels match with source reads blocked");
        }
    }

    /// <summary>
    /// Import-only spritemap oracle shared by map and pause-UI parity checks. Gameplay
    /// OAM cannot read immutable cartridge sprites after installation.
    /// </summary>
    internal static void DrawImportedSpritemap(ISnesAddressSpace bus, OamBuffer oam,
        int address, Position16<SpritePixels> origin, ushort paletteBits,
        bool originIsOnScreen = true) => ImportedSpritemapOracle.DrawGeneric(
            CartridgeImportSource.Require(bus), oam, address, origin,
            paletteBits, originIsOnScreen);

    /// <summary>Forbids runtime reads of installed map characters and every native map sprite pointer and body.</summary>
    private static MutableCartridgeReadGuard NewMapSpriteGuard(ISnesAddressSpace source)
    {
        IImportCartridgeSource cartridge = CartridgeImportSource.Require(source);
        var forbidden = new HashSet<int>();
        Add(MapSourceDefinitions.Menu.ObjectCharacters, MapSpriteFormat.ByteCount);
        Add(MapSourceDefinitions.FileSelectMap.LabelSpritemaps, 2);
        foreach (var frame in MapSpriteArtwork.FrameNames)
        {
            int entry = MapSourceDefinitions.Menu.SpritemapPointers + (int)frame.Id * 2;
            int pointer = MapSourceDefinitions.Menu.ObjectBank | RomDataReader.ReadWordFixedBank(cartridge, entry);
            Add(entry, 2); Add(pointer, 2 + 5 * RomDataReader.ReadWordFixedBank(cartridge, pointer));
        }
        void Add(int start, int count) { for (int index = 0; index < count; index++) forbidden.Add(start + index); }
        var guard = new MutableCartridgeReadGuard(source);
        guard.Forbid(forbidden.Contains, address => $"Installed map read migrated sprite data at {address:X6}.");
        return guard;
    }
}
