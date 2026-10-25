using SuperMetroid.Core.Samus.Drawing;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Enemies.Chozo;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Enemies.Chozo;

using SuperMetroid.Verification.Harness;
/// <summary>Checks the Wrecked Ship Chozo hand's Samus carry and optional Power Bomb continuation.</summary>
internal static class ChozoGrabAnimationVerification
{
    /// <summary>Checks frozen ball animation during capture, movement and release, and blast state when armed.</summary>
    internal static void VerifyChozoGrabAnimation(bool powerBomb = false)
    {
        var game=new SuperMetroidGame(new SuperMetroid.Core.Saves.SaveSlotStore());
        InstalledFixtures.BindGame(game);
        game.Session.CreateGameplayRuntime(GameplayRuntimePurpose.SelectedGame);
        var runtime = game.Session.GameplayRuntime;
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom();runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.System.SetEvent(EventNumber.ZebesAwake);
        runtime.System.SetBossBits(AreaId.WreckedShip, BossBits.AreaBoss);
        runtime.LoadCartridgeRoomForDebug(RoomHeaderDefinitions.BowlingAlley);
        var samus = runtime.Player.IntroducedSamus;
        samus.Pose = SamusPoseId.MorphBallGroundRightPose;
        samus.CollectedItems = samus.EquippedItems = SamusEquipmentFlags.MorphBall;
        SamusPoseMetadata.RefreshCollisionRadii(samus);
        SamusPresentation.InitializeAnimation(samus);
        samus.CommitPoseHistory();
        samus.AnimationFrame = 2;
        samus.AnimationFrameTimer = WrappingCountdown.Start(3);
        samus.Position = samus.Position.WithWhole(new(0x4a * 16 + 8, (ushort)(0x17 * 16 - samus.Kinematics.Radius.Y)));
        var level = runtime.Room.LoadedLevelData;
        var block = level.GetCollisionBlockByIndex(level.GetBlockIndex(new(0x4a, 0x17)));
        AssertEqual(ChozoStatueRoomDefinitions.WreckedShipHandBts.Value, block.Bts.Value,
            "fixture uses the retail statue hand block");
        var expectedExplosion = new SamusPowerBombExplosionState
        {
            PresentationColors = runtime.BombProjectiles.PowerBombExplosion.PresentationColors,
        };
        if (powerBomb)
        {
            // Detonation immediately before hand capture; origin differs from Samus so
            // cleanup cannot accidentally enter Crystal Flash in this carry fixture.
            ushort bombX = (ushort)(samus.Position.X.Whole - 24);
            runtime.BombProjectiles.PowerBombExplosion.Arm();
            runtime.BombProjectiles.PowerBombExplosion.Spawn(new(bombX, samus.Position.Y.Whole));
            expectedExplosion.Arm();
            expectedExplosion.Spawn(new(bombX, samus.Position.Y.Whole));
        }
        runtime.Plms.ChozoStatue.NotifyChozoStatueHandCollision(level, block, samus, samus.Pose, movingDown: true);
        AssertTrue(samus.Control.Mode.Beta == SamusBetaHandler.ScriptLocked && samus.Control.AlphaLocked,
            "hand trigger installs native stationary handlers");
        AssertEqual(2, samus.AnimationFrame, "grab preserves captured ball frame");
        AssertEqual((ushort)3, samus.AnimationFrameTimer.RawValue, "grab preserves captured ball timer");
        ushort startX = samus.Position.X.Whole, startY = samus.Position.Y.Whole;
        bool carried = false;
        int frames = 0;
        while (samus.Control.Mode.Beta == SamusBetaHandler.ScriptLocked && frames < 2000)
        {
            runtime.StepFrame(SnesButton.Right);
            if (powerBomb)
            {
                if (expectedExplosion.StepFrame()) expectedExplosion.ReleaseFlag();
                var actual = runtime.BombProjectiles.PowerBombExplosion;
                AssertEqual(expectedExplosion.Phase, actual.Phase, $"carried Power Bomb phase at frame {frames}");
                AssertEqual(expectedExplosion.PreExplosionRadius, actual.PreExplosionRadius, $"carried Power Bomb pre-radius at frame {frames}");
                AssertEqual(expectedExplosion.ExplosionRadius, actual.ExplosionRadius, $"carried Power Bomb radius at frame {frames}");
                AssertEqual(expectedExplosion.RenderedPhase, actual.RenderedPhase, $"carried Power Bomb rendered phase at frame {frames}");
                AssertEqual(expectedExplosion.RenderedPreExplosionRadius, actual.RenderedPreExplosionRadius, "carried blast rendered pre-radius");
                AssertEqual(expectedExplosion.RenderedExplosionRadius, actual.RenderedExplosionRadius, "carried blast rendered radius");
                AssertEqual(expectedExplosion.FixedColor.Red, actual.FixedColor.Red, "carried blast red");
                AssertEqual(expectedExplosion.FixedColor.Green, actual.FixedColor.Green, "carried blast green");
                AssertEqual(expectedExplosion.FixedColor.Blue, actual.FixedColor.Blue, "carried blast blue");
                AssertEqual(expectedExplosion.Status, actual.Status, $"carried Power Bomb cleanup at frame {frames}");
            }
            if (samus.Control.Mode.Beta == SamusBetaHandler.ScriptLocked)
            {
                AssertEqual(2, samus.AnimationFrame, "ball frame remains frozen while carried");
                AssertEqual((ushort)3, samus.AnimationFrameTimer.RawValue, "ball animation timer remains frozen while carried");
            }
            carried |= samus.Position.X.Whole != startX || samus.Position.Y.Whole != startY;
            frames++;
        }
        if (powerBomb)
            AssertTrue(!runtime.BombProjectiles.PowerBombExplosion.IsActive && !runtime.BombProjectiles.PowerBombExplosion.IsArmed,
                "Power Bomb completes and releases its armed flag during the statue ride");
        AssertTrue(carried, "statue still moves Samus while animation is locked");
        AssertTrue(samus.Control.Mode.Beta != SamusBetaHandler.ScriptLocked && !samus.Control.AlphaLocked && runtime.Player.GroundedSamusMovementEnabled,
            "native release restores handlers and movement");
        bool animationResumed = false;
        for (int frame = 0; frame < 12; frame++)
        {
            runtime.StepFrame(SnesButton.Right);
            if (powerBomb)
            {
                if (expectedExplosion.StepFrame()) expectedExplosion.ReleaseFlag();
                var actual = runtime.BombProjectiles.PowerBombExplosion;
                AssertEqual(expectedExplosion.Phase, actual.Phase, $"carried Power Bomb phase at frame {frames}");
                AssertEqual(expectedExplosion.PreExplosionRadius, actual.PreExplosionRadius, $"carried Power Bomb pre-radius at frame {frames}");
                AssertEqual(expectedExplosion.ExplosionRadius, actual.ExplosionRadius, $"carried Power Bomb radius at frame {frames}");
                AssertEqual(expectedExplosion.RenderedPhase, actual.RenderedPhase, $"carried Power Bomb rendered phase at frame {frames}");
                AssertEqual(expectedExplosion.RenderedPreExplosionRadius, actual.RenderedPreExplosionRadius, "carried blast rendered pre-radius");
                AssertEqual(expectedExplosion.RenderedExplosionRadius, actual.RenderedExplosionRadius, "carried blast rendered radius");
                AssertEqual(expectedExplosion.FixedColor.Red, actual.FixedColor.Red, "carried blast red");
                AssertEqual(expectedExplosion.FixedColor.Green, actual.FixedColor.Green, "carried blast green");
                AssertEqual(expectedExplosion.FixedColor.Blue, actual.FixedColor.Blue, "carried blast blue");
                AssertEqual(expectedExplosion.Status, actual.Status, $"carried Power Bomb cleanup at frame {frames}");
            }
            animationResumed |= samus.AnimationFrame != 2 || samus.AnimationFrameTimer.RawValue != 3;
        }
        AssertTrue(animationResumed, "ball animation resumes after release");
        if (powerBomb) Console.WriteLine("Power Bomb continues through capture: exact phases, radii, rendered state, colors and flag cleanup pass.");
        Console.WriteLine($"Walking Chozo: native hand, frozen frame/timer through {frames} carry frames, movement and release verified.");
    }

    /// <summary>Runs the Chozo carry fixture with a Power Bomb detonating before capture.</summary>
    [VerificationCommand("--chozo-power-bomb")]
    internal static int ChozoPowerBombCommand()
    {
     VerifyChozoGrabAnimation(powerBomb: true); return 0; 
    }

    /// <summary>Runs the focused Chozo hand grab and animation fixture.</summary>
    [VerificationCommand("--chozo-grab-animation")]
    internal static int ChozoGrabAnimationCommand()
    {
        Suite(nameof(VerifyChozoGrabAnimation), () => VerifyChozoGrabAnimation());
        return 0;
    }
}
