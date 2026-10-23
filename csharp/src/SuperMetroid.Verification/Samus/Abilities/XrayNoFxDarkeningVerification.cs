using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Samus.Drawing;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
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
/// <summary>Checks X-ray color math in a room without visible layer-three FX.</summary>
internal static class XrayNoFxDarkeningVerification
{
    /// <summary>Confirms opaque BG3 halving, transparent fixed-color fallback, and Fireflea subtraction.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native CGRAM slots, native VRAM words")]
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
        var tile = new byte[32];
        for (int row = 0; row < 8; row++) tile[row * 2] = 255;
        runtime.Vram.LoadBytes(0x3fe * 32, tile);
        runtime.Vram.ExecuteWordTransfer(Enumerable.Repeat((ushort)0x3fe, 2048).ToArray(),
            GameplayVramLayout.Bg1TilemapWord, 1);
        runtime.Cgram.AsPaletteColors().SetColor(1, new Bgr555(24, 20, 16));
        // Native Clear_FX_Tilemap's $184E selects BG3 palette six, color three.
        runtime.Cgram.AsPaletteColors().SetColor(6 * 4 + 3, new Bgr555(0, 0, 0));
        var snapshot = GameplayDisplayCapture.Capture(runtime.PublishDisplayFrame());
        var pixels = SoftwareLayeredSnapshotRenderer.Render(snapshot);
        var expected = new Rgba32(99, 82, 66, 255); // RGB5(12,10,8)
        foreach (int index in new[] { 80 * 256 + 64, 120 * 256 + 128, 180 * 256 + 192 })
            AssertEqual(expected, pixels[index], "opaque black BG3 halves outside-beam foreground instead of brightening");
        var math = (GameplayColorMathRenderLayer)snapshot.Layers[0];
        AssertTrue(math.Subscreen is not null, "no-FX X-ray retains the actual BG3 plane");
        AssertEqual((ushort)0x184e, runtime.Vram.ReadWord(0x5880), "capture retains native cleared tilemap");
        // A transparent replacement must still take the hardware fixed-color fallback.
        runtime.Vram.ExecuteWordTransfer(Enumerable.Repeat((ushort)0x006f, 0x780).ToArray(), 0x5880, 1);
        runtime.Vram.LoadBytes(runtime.Display.GameplayHudCharacterBaseWord * 2 + 0x6f * 16, new byte[16]);
        var transparent = SoftwareLayeredSnapshotRenderer.Render(GameplayDisplayCapture.Capture(runtime.PublishDisplayFrame()));
        AssertEqual(new Rgba32(255, 222, 189, 255), transparent[80 * 256 + 64],
            "transparent BG3 preserves native fixed-color fallback without halving");
        runtime.RoomLayer3Fx.Type = RoomFxType.Fireflea;
        // The X-ray blending branch the FX selects reaches the PPU at the accepted NMI (#1307).
        runtime.FrameBoundary.BeginAcceptedFrame(0);
        var fireflea = GameplayDisplayCapture.Capture(runtime.PublishDisplayFrame());
        var firefleaMath = (GameplayColorMathRenderLayer)fireflea.Layers[0];
        AssertTrue(!firefleaMath.AddSubscreen && firefleaMath.Subscreen is null,
            "Fireflea retains fixed-color subtraction without a BG3 operand");
        var darkPixels = SoftwareLayeredSnapshotRenderer.Render(fireflea);
        AssertEqual(new Rgba32(140, 107, 74, 255), darkPixels[80 * 256 + 64],
            "Fireflea subtracts seven without halving");
        Console.WriteLine("X-ray no-FX capture: opaque BG3 darkening, transparent fallback and Fireflea exception verified.");
    }

    /// <summary>Runs the focused no-FX X-ray display check.</summary>
    [VerificationCommand("--xray-no-fx-darkening")]
    internal static int XrayNoFxDarkeningCommand()
    {
        Suite(nameof(VerifyXrayNoFxDarkening), () => VerifyXrayNoFxDarkening());
        return 0;
    }
}
