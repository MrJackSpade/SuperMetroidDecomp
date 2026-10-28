using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Samus.Drawing;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Samus.Abilities;

using SuperMetroid.Verification.Harness;
using SuperMetroid.TestSupport.FrameRendering;
/// <summary>Checks X-ray color math in a room without visible layer-three FX.</summary>
internal static class XrayNoFxDarkeningVerification
{
    /// <summary>Confirms opaque BG3 halving, transparent fixed-color fallback, and Fireflea subtraction.</summary>
    internal static void VerifyXrayNoFxDarkening()
    {
        var game=new SuperMetroidGame(new SuperMetroid.Core.Saves.SaveSlotStore());
        InstalledFixtures.BindGame(game);
        game.Session.CreateGameplayRuntime(GameplayRuntimePurpose.SelectedGame);
        var runtime = game.Session.GameplayRuntime;
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom();runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.System.SetEvent(EventNumber.ZebesAwake);
        runtime.LoadCartridgeRoomForDebug(RoomHeaderDefinitions.Parlor);
        AssertTrue(runtime.RoomLayer3Fx.CaptureForDisplay() is null, "fixture has no visible room FX");
        var samus = runtime.Player.IntroducedSamus;
        samus.Pose = SamusPoseId.FacingRightNormalPose;
        samus.Kinematics.YSpeed = default;
        SamusPoseMetadata.RefreshCollisionRadii(samus);
        SamusPresentation.InitializeAnimation(samus);
        AssertTrue(samus.Xray.TryBegin(samus, SamusMovementType.Standing, new XrayBeginConditions { GameState = SuperMetroidGameState.MainGameplay }), "fixture enters X-ray");
        // The closed-beam setup state makes the selected pixels unambiguously outside.
        samus.Xray.SetupStage = XraySetupStage.ReadBg1FirstScreen;
        runtime.FrameBoundary.BeginAcceptedFrame(0);
        // Native Clear_FX_Tilemap's $184E selects BG3 palette six, color three.
        runtime.Colours.AsPaletteColors().SetColor(6 * 4 + 3, new RgbColor(0, 0, 0));
        // The room's own foreground is the main screen colour math works on (#1433): BG1 draws from the room's block
        // pictures, so each expectation is derived from the colour the frame shows before blending.
        LayeredScene snapshot = GameplayDisplayCapture.Capture(runtime.PublishDisplayFrame());
        RgbaPixel[] unblended = Unblended(snapshot);
        int[] samples = [.. new[] { 80, 120, 180 }.Select(row => FirstLitPixel(unblended, row))];
        RgbaPixel[] pixels = DisplayRendering.Render(snapshot);
        foreach (int index in samples)
            AssertEqual(Channels(unblended[index], channel => channel / 2), pixels[index],
                "opaque black BG3 halves outside-beam foreground instead of brightening");
        var math = (GameplayColorMathRenderLayer)snapshot.Layers[0];
        AssertTrue(math.Subscreen is not null, "no-FX X-ray retains the actual BG3 plane");
        // Clear_FX_Tilemap leaves the effect area of a room without FX in the cleared cell, which the subscreen shows.
        Layer3Plane layer3 = runtime.AcceptedLayer3Plane;
        AssertTrue(layer3.Tiles[(Layer3Plane.HudRows * Layer3Plane.Columns)..].ToArray().All(cell => cell == Layer3Plane.ClearedCell),
            "capture retains the cleared BG3 effect area");
        // Where the subscreen shows nothing, colour math takes the fixed colour without halving: the blend with its BG3
        // operand removed is the transparent case.
        LayeredScene transparentScene = Replace(snapshot, new GameplayColorMathRenderLayer(math.Gameplay, math.Lines, math.RevealBlocks, math.Blend));
        RgbaPixel[] transparent = DisplayRendering.Render(transparentScene);
        foreach (int index in samples)
            AssertEqual(Channels(unblended[index], channel => Math.Min(MaximumChannel, channel + FixedColorChannel)), transparent[index],
                "transparent BG3 preserves native fixed-color fallback without halving");
        runtime.RoomLayer3Fx.Type = RoomFxType.Fireflea;
        // The X-ray blending branch the FX selects reaches the PPU at the accepted NMI (#1307).
        runtime.FrameBoundary.BeginAcceptedFrame(0);
        var fireflea = GameplayDisplayCapture.Capture(runtime.PublishDisplayFrame());
        var firefleaMath = (GameplayColorMathRenderLayer)fireflea.Layers[0];
        AssertTrue(!firefleaMath.Blend.BlendsWithSubscreen && firefleaMath.Subscreen is null,
            "Fireflea retains fixed-color subtraction without a BG3 operand");
        RgbaPixel[] darkPixels = DisplayRendering.Render(fireflea);
        RgbaPixel[] firefleaMain = Unblended(fireflea);
        foreach (int index in samples)
            AssertEqual(Channels(firefleaMain[index], channel => Math.Max(0, channel - FixedColorChannel)), darkPixels[index],
                "Fireflea subtracts seven without halving");
        Console.WriteLine("X-ray no-FX capture: opaque BG3 darkening, transparent fallback and Fireflea exception verified.");
    }

    /// <summary>The five-bit fixed colour each channel of the X-ray's fixed-colour operand holds.</summary>
    private const int FixedColorChannel = 7;

    /// <summary>The largest five-bit channel value.</summary>
    private const int MaximumChannel = 31;

    /// <summary>The frame as its main screen shows before colour math: the scene with its blend layer's gameplay planes.</summary>
    private static RgbaPixel[] Unblended(LayeredScene scene) =>
        DisplayRendering.Render(Replace(scene, ((GameplayColorMathRenderLayer)scene.Layers[0]).Gameplay));

    /// <summary><paramref name="scene"/> with its first layer, the gameplay blend, replaced by <paramref name="first"/>.</summary>
    private static LayeredScene Replace(LayeredScene scene, RenderLayer first)
    {
        RenderLayer[] layers = scene.Layers.ToArray();
        layers[0] = first;
        return new LayeredScene(scene.Paint, layers, scene.Brightness);
    }

    /// <summary>The first pixel of <paramref name="row"/> the main screen shows with every channel lit.</summary>
    private static int FirstLitPixel(RgbaPixel[] pixels, int row)
    {
        for (int column = 0; column < ScreenGeometry.ScreenWidthPixels; column++)
        {
            RgbaPixel pixel = pixels[row * ScreenGeometry.ScreenWidthPixels + column];
            if (pixel.R != 0 && pixel.G != 0 && pixel.B != 0)
                return row * ScreenGeometry.ScreenWidthPixels + column;
        }
        throw new InvalidOperationException($"Fixture row {row} shows no lit foreground pixel.");
    }

    /// <summary><paramref name="pixel"/> with each five-bit channel replaced by <paramref name="map"/> of it.</summary>
    private static RgbaPixel Channels(RgbaPixel pixel, Func<int, int> map) =>
        new RgbColor(map(pixel.R >> 3), map(pixel.G >> 3), map(pixel.B >> 3)).ToRgbaPixel();

    /// <summary>Runs the focused no-FX X-ray display check.</summary>
    [VerificationCommand("--xray-no-fx-darkening")]
    internal static int XrayNoFxDarkeningCommand()
    {
        Suite(nameof(VerifyXrayNoFxDarkening), () => VerifyXrayNoFxDarkening());
        return 0;
    }
}
