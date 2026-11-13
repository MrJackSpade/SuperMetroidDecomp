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
/// <summary>Checks X-ray color math in a room without a visible room effect.</summary>
internal static class XrayNoFxDarkeningVerification
{
    /// <summary>Confirms the fixed colour adds without halving where no effect lies behind, and Fireflea subtraction.</summary>
    internal static void VerifyXrayNoFxDarkening()
    {
        var game=new SuperMetroidGame(new SuperMetroid.Core.Saves.SaveSlotStore());
        InstalledFixtures.BindGame(game);
        game.Session.CreateGameplayRuntime(GameplayRuntimePurpose.SelectedGame);
        var runtime = game.Session.GameplayRuntime;
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom();runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.System.SetEvent(StoryEvent.ZebesAwake);
        runtime.LoadCartridgeRoomForDebug(RoomDefinitions.Parlor);
        AssertTrue(runtime.RoomFxOverlay.CaptureForDisplay() is null, "fixture has no visible room FX");
        var samus = runtime.Player.IntroducedSamus;
        samus.Pose = SamusPoseId.FacingRightNormalPose;
        samus.Kinematics.YSpeed = default;
        SamusPoseMetadata.RefreshCollisionRadii(samus);
        SamusPresentation.InitializeAnimation(samus);
        AssertTrue(samus.Xray.TryBegin(samus, SamusMovementType.Standing, new XrayBeginConditions { GameState = SuperMetroidGameState.MainGameplay }), "fixture enters X-ray");
        // The warm-up's third update shows the blended room with the beam's window still closed, so the selected pixels are
        // unambiguously outside.
        samus.Xray.WarmUp = XrayWarmUp.Started.Spend().Spend();
        runtime.FrameBoundary.BeginAcceptedFrame(0);
        // The room's own foreground is the main screen colour math works on (#1433): it draws from the room's block
        // pictures, so each expectation is derived from the colour the frame shows before blending.
        LayeredScene snapshot = GameplayDisplayCapture.Capture(runtime.PublishDisplayFrame());
        RgbaPixel[] unblended = Unblended(snapshot);
        int[] samples = [.. new[] { 80, 120, 180 }.Select(row => FirstLitPixel(unblended, row))];
        var math = (GameplayBlendLayer)snapshot.Layers[0];
        // A room with no visible effect blends with nothing behind (#1514): the cleared effect area is not drawn.
        AssertTrue(math.OverlayBehind is null && !math.BackgroundBehind && !math.Blend.Halved,
            "no-FX X-ray blends with nothing lying behind, so it adds its colour whole");
        // Where nothing lies behind, colour math takes the fixed colour without halving.
        RgbaPixel[] pixels = DisplayRendering.Render(snapshot);
        foreach (int index in samples)
            AssertEqual(Channels(unblended[index], channel => Math.Min(MaximumChannel, channel + FixedColorChannel)), pixels[index],
                "the fixed colour adds to the outside-beam foreground without halving");
        runtime.RoomFxOverlay.Type = RoomFxType.Fireflea;
        // The X-ray blending branch the FX selects reaches the PPU at the accepted NMI (#1307).
        runtime.FrameBoundary.BeginAcceptedFrame(0);
        var fireflea = GameplayDisplayCapture.Capture(runtime.PublishDisplayFrame());
        var firefleaMath = (GameplayBlendLayer)fireflea.Layers[0];
        AssertTrue(firefleaMath.OverlayBehind is null && !firefleaMath.BackgroundBehind && firefleaMath.Blend.Operation == BlendOperation.Subtract,
            "Fireflea retains its shade's subtraction with nothing lying behind");
        RgbaPixel[] darkPixels = DisplayRendering.Render(fireflea);
        RgbaPixel[] firefleaMain = Unblended(fireflea);
        foreach (int index in samples)
            AssertEqual(Channels(firefleaMain[index], channel => Math.Max(0, channel - FixedColorChannel)), darkPixels[index],
                "Fireflea subtracts seven without halving");
        Console.WriteLine("X-ray no-FX capture: fixed-colour addition without an effect behind and Fireflea exception verified.");
    }

    /// <summary>The five-bit fixed colour each channel of the X-ray's fixed-colour operand holds.</summary>
    private const int FixedColorChannel = 7;

    /// <summary>The largest five-bit channel value.</summary>
    private const int MaximumChannel = 31;

    /// <summary>The frame as its main screen shows before colour math: the scene with its blend layer's gameplay planes.</summary>
    private static RgbaPixel[] Unblended(LayeredScene scene) =>
        DisplayRendering.Render(Replace(scene, ((GameplayBlendLayer)scene.Layers[0]).Gameplay));

    /// <summary><paramref name="scene"/> with its first layer, the gameplay blend, replaced by <paramref name="first"/>.</summary>
    private static LayeredScene Replace(LayeredScene scene, DisplayLayer first)
    {
        DisplayLayer[] layers = scene.Layers.ToArray();
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
        new FadeLevelColour(map(pixel.R >> 3), map(pixel.G >> 3), map(pixel.B >> 3)).ToColour().ToRgbaPixel();

    /// <summary>Runs the focused no-FX X-ray display check.</summary>
    [VerificationCommand("--xray-no-fx-darkening")]
    internal static int XrayNoFxDarkeningCommand()
    {
        Suite(nameof(VerifyXrayNoFxDarkening), () => VerifyXrayNoFxDarkening());
        return 0;
    }
}
