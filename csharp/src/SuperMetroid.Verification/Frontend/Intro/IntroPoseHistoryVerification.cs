using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Drawing;
using static SuperMetroid.Verification.Runtime.Attract.AttractCaptureTestsVerification;
using static SuperMetroid.Verification.Frontend.FrontendRenderCaptureTestsVerification;
using static SuperMetroid.Verification.Runtime.GameplayCaptureIntegrationTestsVerification;
using SuperMetroid.Core.Input;
using static SuperMetroid.Verification.Harness.RetailGameFixtureVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Poses;
namespace SuperMetroid.Verification.Frontend.Intro;

using SuperMetroid.Verification.Harness;

/// <summary>Checks Samus pose history publication during intro flashback and discovery.</summary>
internal static class IntroPoseHistoryVerification
{
    /// <summary>Checks idle, run, and same-pose history shifts after exercising the full intro scene.</summary>
    internal static void VerifyIntroPoseHistory()
    {
        Suite(nameof(VerifyIntroScenePoseHistory), () => VerifyIntroScenePoseHistory());
        var samus = new SamusState(GameplayAudioRequests.Standalone(), InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork()) { Position = FixedPosition<RoomPixels>.FromWhole(new(128, 235)) }.InPose(SamusPoseId.FacingLeftNormalPose);
        // The intro's rooms are plain terrains: air, with a solid floor row.
        var blocks = new PlainTerrainBlock[16 * 32];
        for (int x = 0; x < 16; x++) blocks[16 * 16 + x] = PlainTerrainBlock.Solid;
        var room = new PlainTerrain(new Extent16<RoomBlocks>(16, 32), blocks);
        SamusPoseMetadata.RefreshCollisionRadii(samus);
        SamusPresentation.InitializeAnimation(samus);
        var history = samus.PoseHistory;
        var random = new SharedRandom();
        history.Restore(samus.Pose, SamusMovementType.Standing, SamusMovementType.SpinJumping);
        IntroSamusDemoMovement.StepGroundedLeft(room, samus, 0, 0, 0, random);
        AssertEqual(SamusMovementType.SpinJumping, history.MovementBeforePrevious, "idle intro retains older metadata");
        AssertEqual(samus.Pose, history.PreviousPose, "idle intro retains previous pose");
        AssertEqual(SamusMovementType.Standing, history.PreviousMovement, "idle intro retains previous metadata");
        IntroSamusDemoMovement.StepGroundedLeft(room, samus, GameplayAction.Left, GameplayAction.Left, 1, random);
        AssertEqual(SamusPoseId.MovingLeftNormalPose, samus.Pose, "intro fixture starts running left");
        AssertEqual(SamusMovementType.Standing, history.MovementBeforePrevious, "intro run transition shifts prior metadata");
        AssertEqual(samus.Pose, history.PreviousPose, "intro run transition commits current pose");
        AssertEqual(SamusMovementType.Running, history.PreviousMovement, "intro run transition commits running metadata");
        samus.HorizontalSpeed.BaseSpeed = samus.HorizontalSpeed.BaseSpeed.WithWhole(2);
        history.Restore(history.PreviousPose, history.PreviousMovement, SamusMovementType.SpinJumping);
        IntroSamusDemoMovement.StepGroundedLeft(room, samus, 0, 0, 2, random);
        AssertEqual(SamusPoseId.MovingLeftNormalPose, samus.Pose, "intro release fixture retains running during deceleration");
        AssertEqual(SamusMovementType.Running, history.MovementBeforePrevious, "intro same-pose fallback shifts running metadata");
        AssertEqual(samus.Pose, history.PreviousPose, "intro same-pose fallback commits current pose");
        AssertEqual(SamusMovementType.Running, history.PreviousMovement, "intro same-pose fallback commits current metadata");
    }

    /// <summary>
    /// Runs the real intro and reads the scenes it composes (#2554): the flashback draws Samus from its setup update at her
    /// starting place, her drawn body changes as its demo runs, knocks back and lands her, the page-two text that follows
    /// Mother Brain's deletion draws no Samus, and the discovery carries her on, drawn right of the screen she enters from.
    /// </summary>
    private static void VerifyIntroScenePoseHistory()
    {
        // The intro waits on its music queue, which the game runs after every dispatch.
        var audio = AudioQueues.Standalone();
        var intro = CreateRetailIntroFixture(audio);
        _ = intro.StepToMotherBrainFlashback(audio);
        LayeredScene scene = intro.ComposeScene();
        AssertEqual<int?>(IntroCinematicDefinitions.Flashback.SamusPosition.X, scene.DrawnSamusOrigin?.X,
            "the flashback draws Samus at her starting place from its setup update");
        SpriteSheetFrame? shown = scene.DrawnSamusBody?.Image;
        int bodyChanges = 0;
        bool pageTwoText = false;
        for (int tick = 0; intro.Phase != IntroCinematicPhase.BabyDiscoveryCrossfade; tick++)
        {
            AssertTrue(tick < 20_000, $"the intro reaches the discovery; it stands in {intro.Phase}");
            intro.Step(tick % 47 == 0 ? ControllerButton.A : 0);
            audio.AdvanceFrame(default);
            // Publishing the display runs the draw passes whose side effects the scene keeps (projectile trails), as the
            // frontend publishes every frame.
            scene = intro.ComposeScene();
            if (intro.Phase is IntroCinematicPhase.PageText or IntroCinematicPhase.PageAwaitingInput)
            {
                pageTwoText = true;
                AssertTrue(scene.DrawnSamusBody is null, $"tick {tick}: the page-two text after Mother Brain's deletion draws no Samus");
                continue;
            }
            if (scene.DrawnSamusBody is not { } body)
                continue;
            if (shown is not null && body.Image != shown)
                bodyChanges++;
            shown = body.Image;
        }
        AssertTrue(pageTwoText && bodyChanges > 0, $"the flashback runs Samus and the page-two text follows it; body changes {bodyChanges}");
        AssertTrue(intro.ComposeScene().DrawnSamusOrigin is { X: >= 256 }, "the discovery carries Samus on, drawn right of the screen she enters from");
    }

    /// <summary>Runs intro pose history and frontend rendering capture fixtures.</summary>
    [VerificationCommand("--frontend-fixtures")]
    internal static int FrontendFixturesCommand()
    {
        Suite(nameof(VerifyIntroPoseHistory), VerifyIntroPoseHistory);
        Suite(nameof(VerifyFrontendRenderCapture), VerifyFrontendRenderCapture);
        Suite(nameof(VerifyGameplayCaptureIntegration), VerifyGameplayCaptureIntegration);
        Suite(nameof(VerifyAttractCapture), VerifyAttractCapture);
        return 0;
    }
}
