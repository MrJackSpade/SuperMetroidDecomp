using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Runtime;
using static SuperMetroid.Verification.Frontend.Map.MapPresentationVerification;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.Core.Hud.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;
using SuperMetroid.Core.Gameplay;
using SuperMetroid.Core.Map;
using SuperMetroid.Core.Map.Artwork;
using SuperMetroid.Diagnostics.SaveStates;
using SuperMetroid.Core.Progression;
using SuperMetroid.AssetExtraction.Frontend.Map;
using SuperMetroid.TestSupport.FrameRendering;
namespace SuperMetroid.Verification.Hud;

//// <summary>Checks installed HUD artwork against the native characters and rendered minimap pixels.</summary>
internal static class HudAtlasVerification
{
    /// <summary>The cartridge's standard BG3 characters and their cleared half, at $9A:B200.</summary>
    private const int NativeHudCharacters = 0x9ab200;

    /// <summary>
    /// Checks that the selected HUD characters show at the next accepted NMI, that visible PNG edits reach the HUD, and
    /// that rebinding artwork shows the new characters from the next accepted NMI whether or not one has passed since the
    /// HUD was set up.
    /// </summary>
    internal static void VerifyHudAtlasIntegration(ISnesAddressSpace bus, string stock, string overrides,
        AreaMapPresentationCatalog original, ImportedAreaMap[] rules,
        GameplayBasePaletteCatalog initialPalettes,
        MapPresentationInstalledRoomAssets fixtureAssets)
    {
        byte[] source = RomDataReader.ReadFixedBank(CartridgeImportSource.Require(bus), NativeHudCharacters, HudTileAtlasFormat.ByteCount);
        AssertTrue(source.SequenceEqual(original.Resolve(InstalledArtworkId.StandardHudTiles).Span), "HUD PNG and compiled padding match all native bytes");
        var guard = NewMapDataGuard(bus, rules);
        var native = SuperMetroid.Core.Runtime.SuperMetroidRuntime.Create(HostGameplayOverrides.None, initialPaletteArt: initialPalettes);
        native.Artwork.MapPresentation = original;
        var installed = SuperMetroid.Core.Runtime.SuperMetroidRuntime.Create(HostGameplayOverrides.None, initialPaletteArt: initialPalettes);
        installed.Artwork.MapPresentation = original;
        fixtureAssets.Bind(native);
        fixtureAssets.Bind(installed);
        Initialize(native); Initialize(installed);
        native.FrameBoundary.BeginAcceptedFrame(0);
        installed.FrameBoundary.BeginAcceptedFrame(0);
        AssertTrue(native.ComposedVram().AsSpan().SequenceEqual(installed.ComposedVram()), "accepted NMI shows the same characters with HUD artwork ROM blocked");
        RgbaPixel[] baseline = Render(native);
        AssertTrue(baseline.SequenceEqual(Render(installed)), "stock installed HUD/minimap renders identical pixels");

        Directory.CreateDirectory(overrides);
        string replacement = Path.Combine(overrides, HudTileAtlasFormat.FileName);
        IndexedPngImage image;
        using (var input = File.OpenRead(Path.Combine(stock, HudTileAtlasFormat.FileName)))
            image = IndexedPng.Read(input, new(MapTileAtlasFormat.Width, MapTileAtlasFormat.Height));
        for (int i = 0; i < image.Pixels.Length; i++) image.Pixels[i] = (byte)((image.Pixels[i] + 1) % 4);
        using (var output = File.Create(replacement)) IndexedPng.Write(output, new(image.Size.X, image.Size.Y), image.Pixels, image.Colours);
        var edited = MapPresentationFiles.Load(stock, overrides);
        // This snapshot already shows the old HUD. Setup's cleared half now lies under room data.
        using var displayedSnapshot = new MemoryStream();
        DebuggerObjectGraphSerializer.Serialize(displayedSnapshot, installed);
        displayedSnapshot.Position = 0;
        var displayed = DebuggerObjectGraphSerializer.Deserialize<SuperMetroidRuntime>(displayedSnapshot);
        fixtureAssets.Bind(displayed);
        int characterStart = GameplayVramLayout.HudCharacterWord * 2;
        int characterEnd = characterStart + HudTileAtlasFormat.CharacterByteCount;
        displayed.Vram.LoadBytes(characterEnd, Enumerable.Repeat((byte)0x5a, HudTileAtlasFormat.CharacterByteCount).ToArray());
        byte[] previousDisplay = displayed.ComposedVram();
        displayed.Artwork.MapPresentation = edited;
        AssertTrue(previousDisplay.AsSpan().SequenceEqual(displayed.ComposedVram()), "catalog binding shows nothing new before the accepted NMI");
        displayed.FrameBoundary.BeginAcceptedFrame(0);
        byte[] shownAfterRebind = displayed.ComposedVram();
        AssertTrue(edited.Resolve(InstalledArtworkId.StandardHudTiles).Span[..HudTileAtlasFormat.CharacterByteCount]
            .SequenceEqual(shownAfterRebind.AsSpan(characterStart, HudTileAtlasFormat.CharacterByteCount)),
            "accepted NMI shows the newly bound PNG over an already displayed HUD");
        AssertTrue(previousDisplay.AsSpan(0, characterStart).SequenceEqual(shownAfterRebind.AsSpan(0, characterStart)) &&
            previousDisplay.AsSpan(characterEnd).SequenceEqual(shownAfterRebind.AsSpan(characterEnd)),
            "HUD rebind changes nothing shown outside its characters, including live room tilemaps");
        var setUp = SuperMetroid.Core.Runtime.SuperMetroidRuntime.Create(HostGameplayOverrides.None, initialPaletteArt: initialPalettes);
        setUp.Artwork.MapPresentation = original;
        fixtureAssets.Bind(setUp);
        Initialize(setUp);
        using var snapshot = new MemoryStream();
        DebuggerObjectGraphSerializer.Serialize(snapshot, setUp);
        snapshot.Position = 0;
        var restored = DebuggerObjectGraphSerializer.Deserialize<SuperMetroidRuntime>(snapshot);
        fixtureAssets.Bind(restored);
        restored.Artwork.MapPresentation = edited;
        restored.FrameBoundary.BeginAcceptedFrame(0);
        AssertTrue(edited.Resolve(InstalledArtworkId.StandardHudTiles).Span.SequenceEqual(
            restored.ComposedVram().AsSpan(GameplayVramLayout.HudCharacterWord * 2, HudTileAtlasFormat.ByteCount)),
            "a HUD set up before its first accepted NMI shows the newly bound PNG, not captured old pixels");
        RgbaPixel[] modified = Render(restored);
        RgbaPixel[] rebound = Render(displayed);
        int[] differing = [.. Enumerable.Range(0, modified.Length).Where(pixel => rebound[pixel] != modified[pixel])];
        AssertTrue(differing.Length == 0,
            "already displayed HUD rebind renders the same edited pixels as a HUD set up before its first NMI" + (differing.Length == 0 ? ""
                : $" ({differing.Length} pixels differ, first at {differing[0] % 256},{differing[0] / 256}: rebound {rebound[differing[0]]}, fresh {modified[differing[0]]})"));
        AssertTrue(Enumerable.Range(8, 24).Any(y => Enumerable.Range(216, 40).Any(x => baseline[y * 256 + x] != modified[y * 256 + x])),
            "edited HUD PNG changes rendered minimap region after accepted NMI");
        var reboundBeforeNmi = SuperMetroid.Core.Runtime.SuperMetroidRuntime.Create(HostGameplayOverrides.None, initialPaletteArt: initialPalettes);
        reboundBeforeNmi.Artwork.MapPresentation = original;
        fixtureAssets.Bind(reboundBeforeNmi);
        Initialize(reboundBeforeNmi);
        reboundBeforeNmi.Artwork.MapPresentation = edited;
        reboundBeforeNmi.FrameBoundary.BeginAcceptedFrame(0);
        AssertTrue(Render(reboundBeforeNmi).SequenceEqual(modified), "artwork rebound before the first accepted NMI shows the current installed artwork");
        File.WriteAllText(replacement, "broken HUD PNG");
        AssertThrows<InvalidDataException>(() => MapPresentationFiles.Load(stock, overrides), "broken HUD override has no silent fallback");
        Console.WriteLine("HUD PNG: exact native character and pixel parity, visible minimap edit and current-content rebind pass.");

        void Initialize(SuperMetroidRuntime runtime)
        {
            // No room palette has been installed in this focused runtime fixture.
            // Give every BG3 color a distinct grayscale value so a changed index is
            // observable, rather than accepting parity between two black HUDs.
            for (int i = 0; i < 32; i++) runtime.Colours.AsPaletteColors().SetColor(i, RgbColor.FromWord((ushort)(i * 0x421)));
            runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
            runtime.Hud.UpdateMinimap(runtime.System, new MinimapFrame { AreaIndex = AreaId.Crateria, RoomMap = new(5, 5), RoomSize = new(16, 16), Samus = new(128, 128), FrameCounter = 8, MapRevealMode = MapRevealMode.Secret, PresentationMap = (runtime.Artwork.MapPresentation ?? throw new InvalidOperationException("Verification requires runtime.Artwork.MapPresentation.")).Get(AreaId.Crateria) });
        }
        // The HUD band the display renderer draws: the BG3 plane the frame latches, its characters as their bound artwork
        // composes them.
        static RgbaPixel[] Render(SuperMetroidRuntime runtime)
        {
            Layer3Cells hud = Layer3CellsFixtures.FromWords(runtime.FrameBoundary.CaptureLayer3Plane().Words, runtime.ComposedVram(),
                runtime.Colours.Colors);
            var layer = new OrdinaryGameplayRenderLayer(new OrdinaryGameplayView(new Position16<RoomPixels>(0, 0), new Position16<Bg2Pixels>(0, 0),
                new Extent16<TilemapCells>(32, 32), ScreenLayers.Overlay), RoomBackgroundsFixtures.Blank() with { Bg3 = hud });
            RgbaPixel[] frame = DisplayRendering.Render(new LayeredScene(new FramePaint(runtime.Colours.Colors, []),
                new RenderLayer[] { layer }, FadeLevel.Full));
            return frame[..(DisplayScreen.HudHeight * DisplayScreen.Width)];
        }
    }
}
