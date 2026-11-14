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
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Gameplay;
using SuperMetroid.Core.Map;
using SuperMetroid.Core.Progression;
using SuperMetroid.TestSupport.FrameRendering;
using SuperMetroid.AssetExtraction.Hud;
namespace SuperMetroid.Verification.Hud;

//// <summary>Checks installed HUD artwork against the native characters and rendered minimap pixels.</summary>
internal static class HudAtlasVerification
{
    /// <summary>The cartridge's standard BG3 characters and their cleared half, at $9A:B200.</summary>
    private const int NativeHudCharacters = 0x9ab200;

    /// <summary>
    /// Checks that the selected stock HUD characters show at the next accepted NMI and render the native pixels.
    /// </summary>
    internal static void VerifyHudAtlasIntegration(ISnesAddressSpace bus,
        AreaMapPresentation original,
        MapPresentationInstalledRoomAssets fixtureAssets)
    {
        byte[] source = RomDataReader.ReadFixedBank(CartridgeImportSource.Require(bus), NativeHudCharacters, HudTileAtlasFormat.ByteCount);
        AssertTrue(source.SequenceEqual(InstalledFixtures.ImportOf(original).Characters.Hud.Bytes.Span), "HUD PNG and compiled padding match all native bytes");
        var native = SuperMetroid.Core.Runtime.SuperMetroidRuntime.Create(GameplayAssists.None, InstalledFixtures.BareRuntimeArtwork);
        native.Artwork.MapPresentation = original;
        var installed = SuperMetroid.Core.Runtime.SuperMetroidRuntime.Create(GameplayAssists.None, InstalledFixtures.BareRuntimeArtwork);
        installed.Artwork.MapPresentation = original;
        fixtureAssets.Bind(native);
        fixtureAssets.Bind(installed);
        Initialize(native); Initialize(installed);
        native.FrameBoundary.BeginAcceptedFrame(0);
        installed.FrameBoundary.BeginAcceptedFrame(0);
        AssertTrue(native.ComposedVram().AsSpan().SequenceEqual(installed.ComposedVram()), "accepted NMI shows the same characters with HUD artwork ROM blocked");
        RgbaPixel[] baseline = Render(native);
        AssertTrue(baseline.SequenceEqual(Render(installed)), "stock installed HUD/minimap renders identical pixels");

        Console.WriteLine("HUD PNG: exact native character and pixel parity passes.");

        void Initialize(SuperMetroidRuntime runtime)
        {
            runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
            runtime.Hud.UpdateMinimap(runtime.System, new MinimapFrame { Area = AreaId.Crateria, RoomMap = new(5, 5), RoomSize = new(16, 16), Samus = new(128, 128), FrameCounter = 8, MapRevealMode = MapRevealMode.Secret, PresentationMap = (runtime.Artwork.MapPresentation ?? throw new InvalidOperationException("Verification requires runtime.Artwork.MapPresentation.")).Get(AreaId.Crateria) });
        }
        // The top rows of the BG3 plane the frame latches, its characters as their bound artwork composes them, drawn added
        // over the black backdrop: the plane no longer holds the HUD, which is its own layer (#1466), so the band shows
        // the plane's own rows on the plane's layer.
        static RgbaPixel[] Render(SuperMetroidRuntime runtime)
        {
            // No room colours are installed in this focused runtime fixture, and gameplay holds no colour image (#1512):
            // the plane is drawn in colours that give every BG3 colour a distinct grayscale value, so a changed index is
            // observable rather than accepting parity between two black HUDs.
            RgbColor[] colours = new RgbColor[256];
            for (int i = 0; i < 32; i++)
                colours[i] = RgbColor.FromWord((ushort)(i * 0x421));
            SuperMetroid.Core.Assets.LayeredPicture plane = FxOverlayFixtures.Picture(runtime.FrameBoundary.CaptureFxOverlayPlane().WordsIn(InstalledFixtures.ImportMaps.RoomFxPages), runtime.ComposedVram(),
                colours);
            var layer = new OverlayBlendLayer(new AtmosphereOverlay(FxOverlayFixtures.Rows(plane, 0, FxOverlayPlaneConstants.Rows), ScrollBands.Uniform(0)), 0,
                BlendOperation.Add, ScrollBands.Uniform(0));
            RgbaPixel[] frame = DisplayRendering.Render(new LayeredScene(FramePaint.FromNativeGameplay(colours, []),
                new DisplayLayer[] { layer }, DisplayBrightness.Full));
            return frame[..(DisplayScreen.HudHeight * DisplayScreen.Width)];
        }
    }
}
