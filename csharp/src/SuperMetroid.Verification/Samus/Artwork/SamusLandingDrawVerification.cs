using SuperMetroid.Core.Samus.Drawing;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Poses;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;

namespace SuperMetroid.Verification.Samus.Artwork;

/// <summary>#1397: Samus is drawn on screen on every gameplay update across a landing.</summary>
internal static class SamusLandingDrawVerification
{
    /// <summary>Lands from a standing jump, a running jump and a fall through the production runtime; every update draws Samus's body on screen.</summary>
    internal static void VerifySamusLandingDraws()
    {
        int landings = CheckLanding("standing jump", setup: _ => { }, update => update < 8 ? ControllerButton.A : 0);
        landings += CheckLanding("running jump", setup: _ => { }, update => update < 8 ? ControllerButton.Right | ControllerButton.A : ControllerButton.Right);
        landings += CheckLanding("fall", runtime =>
        {
            SamusState samus = runtime.Player.IntroducedSamus;
            samus.Position = samus.Position.WithWholeY(120);
        }, _ => 0);
        Console.WriteLine($"#1397: {landings} landing-pose updates across a standing jump, a running jump and a fall draw Samus on screen.");
    }

    /// <summary>
    /// Starts Samus grounded in a closed box, applies <paramref name="setup"/>, then steps with <paramref name="input"/>
    /// and checks that every update draws her body on screen. Returns how many updates showed a landing pose.
    /// </summary>
    private static int CheckLanding(string name, Action<SuperMetroidRuntime> setup, Func<int, ControllerButton> input)
    {
        SuperMetroidRuntime runtime = CreateRetailRuntimeFixture();
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.FrameBoundary.BeginAcceptedFrame(0);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(runtime.Room.LoadedRoom.Definition, new Position16<RoomPixels>(0, 0));
        var level = runtime.Room.LoadedLevelData;
        for (int y = 0; y <= 16; y++)
        for (int x = 0; x < level.Size.X; x++)
            level.SetForegroundEntry(y * level.Size.X + x,
                y is 0 or 16 || x is 0 or 15 ? (ushort)0x8000 : (ushort)0);
        runtime.InitializeDebugGroundedSamus(64, 232, 16);
        SamusState samus = runtime.Player.IntroducedSamus;
        samus.Control.Unlock();
        samus.Pose = SamusPoseId.FacingRightNormalPose;
        SamusPoseMetadata.RefreshCollisionRadii(samus);
        SamusPresentation.InitializeAnimation(samus);
        setup(runtime);
        // Let the camera settle on the box before the checked updates.
        for (int settle = 0; settle < 20; settle++) runtime.StepFrame(0);
        int landingUpdates = 0;
        for (int update = 0; update < 60; update++)
        {
            runtime.StepFrame(input(update));
            SpriteDraw[] body = [.. runtime.Sprites.Draws.Where(draw => draw.Source.Kind == SpriteSourceKind.Samus)];
            AssertTrue(body.Length > 0, $"{name} update {update} ({samus.Pose}) draws Samus's body");
            AssertTrue(body.Any(OverlapsScreen),
                $"{name} update {update} ({samus.Pose}) draws Samus's body on screen, not at {string.Join(", ", body.Select(draw => draw.ToString()))}");
            if (samus.Pose is >= SamusPoseId.NormalLandingRightPose and <= SamusPoseId.SpinLandingLeftPose)
                landingUpdates++;
        }
        AssertTrue(landingUpdates > 0, $"{name} reaches a landing pose");
        return landingUpdates;
    }

    /// <summary>Whether any of the draw's placed parts covers a visible screen pixel.</summary>
    private static bool OverlapsScreen(SpriteDraw draw) => Enumerable.Range(0, draw.Frame.Parts.Length).Select(draw.Place).Any(part =>
        part.Position.X + part.Size.X > 0 && part.Position.X < FrontendFrame.Width &&
        part.Position.Y + part.Size.Y > 0 && part.Position.Y < FrontendFrame.Height);
}
