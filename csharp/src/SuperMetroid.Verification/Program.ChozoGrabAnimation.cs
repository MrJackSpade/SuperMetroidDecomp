using System.Buffers.Binary;
using System.Runtime.InteropServices;
using SuperMetroid.Core.Rendering;
using static SuiteTimingVerification;
using System.Reflection;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
/// <summary>Checks the Wrecked Ship Chozo hand's Samus carry and optional Power Bomb continuation.</summary>
internal static class ChozoGrabAnimationVerification
{
    /// <summary>Checks frozen ball animation during capture, movement and release, and blast state when armed.</summary>
    internal static void VerifyChozoGrabAnimation(bool powerBomb = false)
    {
        const BindingFlags flags=BindingFlags.Instance|BindingFlags.NonPublic;
        var installation=RepositoryInstallation.Installation;
        var bus=installation.OpenRuntimeAddressSpace();
        var game=new SuperMetroidGame(bus);
        InstalledFixtures.BindGame(game);
        typeof(SuperMetroidGame).GetMethod("CreateGameplayRuntime",flags)!.Invoke(game,new object[]{false});
        var runtime=(SuperMetroidRuntime)typeof(SuperMetroidGame).GetField("runtime",flags)!.GetValue(game)!;
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom();runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.System.SetEvent(EventNumber.ZebesAwake);
        runtime.System.SetBossBits(AreaId.WreckedShip, BossBits.AreaBoss);
        runtime.LoadCartridgeRoomForDebug(0xc98e);
        var samus = runtime.Player.Samus!;
        samus.Pose = SamusPoseId.MorphBallGroundRightPose;
        samus.CollectedItems = samus.EquippedItems = (ushort)SamusEquipmentFlags.MorphBall;
        samus.PoseMetadata.RefreshCollisionRadii(bus);
        samus.Presentation.InitializeAnimation(bus);
        samus.CommitPoseHistory(bus);
        samus.AnimationFrame = 2;
        typeof(SamusState).GetProperty(nameof(SamusState.AnimationFrameTimer))!.SetValue(samus, (ushort)3);
        samus.Position = samus.Position.WithWhole(new(0x4a * 16 + 8, (ushort)(0x17 * 16 - samus.Kinematics.Radius.Y)));
        var level = runtime.Room.LevelData!;
        var block = level.GetCollisionBlockByIndex(level.GetBlockIndex(new(0x4a, 0x17)));
        AssertEqual(ChozoStatuePlmRomData.WreckedShipHandBts.Value, block.Bts.Value,
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
        AssertTrue(samus.ScriptLocks.StationaryScriptControlLocked && samus.InputLocked,
            "hand trigger installs native stationary handlers");
        AssertEqual((ushort)2, samus.AnimationFrame, "grab preserves captured ball frame");
        AssertEqual((ushort)3, samus.AnimationFrameTimer, "grab preserves captured ball timer");
        ushort startX = samus.Position.X.Whole, startY = samus.Position.Y.Whole;
        bool carried = false;
        int frames = 0;
        while (samus.ScriptLocks.StationaryScriptControlLocked && frames < 2000)
        {
            runtime.StepFrame((ushort)SnesButton.Right);
            if (powerBomb)
            {
                if (expectedExplosion.StepFrame(bus)) expectedExplosion.ReleaseFlag();
                var actual = runtime.BombProjectiles.PowerBombExplosion;
                AssertEqual(expectedExplosion.Phase, actual.Phase, $"carried Power Bomb phase at frame {frames}");
                AssertEqual(expectedExplosion.PreExplosionRadius, actual.PreExplosionRadius, $"carried Power Bomb pre-radius at frame {frames}");
                AssertEqual(expectedExplosion.ExplosionRadius, actual.ExplosionRadius, $"carried Power Bomb radius at frame {frames}");
                AssertEqual(expectedExplosion.RenderedPhase, actual.RenderedPhase, $"carried Power Bomb rendered phase at frame {frames}");
                AssertEqual(expectedExplosion.RenderedPreExplosionRadius, actual.RenderedPreExplosionRadius, "carried blast rendered pre-radius");
                AssertEqual(expectedExplosion.RenderedExplosionRadius, actual.RenderedExplosionRadius, "carried blast rendered radius");
                AssertEqual(expectedExplosion.FixedColorRed, actual.FixedColorRed, "carried blast red");
                AssertEqual(expectedExplosion.FixedColorGreen, actual.FixedColorGreen, "carried blast green");
                AssertEqual(expectedExplosion.FixedColorBlue, actual.FixedColorBlue, "carried blast blue");
                AssertEqual(expectedExplosion.Status, actual.Status, $"carried Power Bomb cleanup at frame {frames}");
            }
            if (samus.ScriptLocks.StationaryScriptControlLocked)
            {
                AssertEqual((ushort)2, samus.AnimationFrame, "ball frame remains frozen while carried");
                AssertEqual((ushort)3, samus.AnimationFrameTimer, "ball animation timer remains frozen while carried");
            }
            carried |= samus.Position.X.Whole != startX || samus.Position.Y.Whole != startY;
            frames++;
        }
        if (powerBomb)
            AssertTrue(!runtime.BombProjectiles.PowerBombExplosion.IsActive && runtime.BombProjectiles.PowerBombExplosion.Flag == 0,
                "Power Bomb completes and releases its armed flag during the statue ride");
        AssertTrue(carried, "statue still moves Samus while animation is locked");
        AssertTrue(!samus.ScriptLocks.StationaryScriptControlLocked && !samus.InputLocked && runtime.Player.GroundedSamusMovementEnabled,
            "native release restores handlers and movement");
        bool animationResumed = false;
        for (int frame = 0; frame < 12; frame++)
        {
            runtime.StepFrame((ushort)SnesButton.Right);
            if (powerBomb)
            {
                if (expectedExplosion.StepFrame(bus)) expectedExplosion.ReleaseFlag();
                var actual = runtime.BombProjectiles.PowerBombExplosion;
                AssertEqual(expectedExplosion.Phase, actual.Phase, $"carried Power Bomb phase at frame {frames}");
                AssertEqual(expectedExplosion.PreExplosionRadius, actual.PreExplosionRadius, $"carried Power Bomb pre-radius at frame {frames}");
                AssertEqual(expectedExplosion.ExplosionRadius, actual.ExplosionRadius, $"carried Power Bomb radius at frame {frames}");
                AssertEqual(expectedExplosion.RenderedPhase, actual.RenderedPhase, $"carried Power Bomb rendered phase at frame {frames}");
                AssertEqual(expectedExplosion.RenderedPreExplosionRadius, actual.RenderedPreExplosionRadius, "carried blast rendered pre-radius");
                AssertEqual(expectedExplosion.RenderedExplosionRadius, actual.RenderedExplosionRadius, "carried blast rendered radius");
                AssertEqual(expectedExplosion.FixedColorRed, actual.FixedColorRed, "carried blast red");
                AssertEqual(expectedExplosion.FixedColorGreen, actual.FixedColorGreen, "carried blast green");
                AssertEqual(expectedExplosion.FixedColorBlue, actual.FixedColorBlue, "carried blast blue");
                AssertEqual(expectedExplosion.Status, actual.Status, $"carried Power Bomb cleanup at frame {frames}");
            }
            animationResumed |= samus.AnimationFrame != 2 || samus.AnimationFrameTimer != 3;
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
