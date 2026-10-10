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
internal static partial class Program
{
    /// <summary>Verifies the Chozo hand carries morph-ball Samus with animation frozen until release, while movement and optional Power Bomb state continue.</summary>
    /// <param name="powerBomb">Starts a Power Bomb before capture and verifies its independent progression through the statue ride.</param>
    private static void VerifyChozoGrabAnimation(bool powerBomb = false)
    {
        const BindingFlags flags=BindingFlags.Instance|BindingFlags.NonPublic;
        var installation=RepositoryInstallation.Installation;
        var bus=installation.OpenRuntimeAddressSpace();
        var game=new SuperMetroidGame(bus);
        game.BindMapPresentation(installation.LoadMaps());
        game.BindGameplayBasePalettes(installation.LoadGameplayBasePalettes());
        game.BindStandardObjectArt(installation.LoadStandardObjects());
        game.BindIntroCinematicArt(installation.LoadIntroCinematicArt());
        game.BindSamusBodyArt(installation.LoadSamusBodyArt());
        game.BindEndingMode7Art(installation.LoadEndingMode7Art());
        game.BindEndingObjectArt(installation.LoadEndingObjectArt());
        game.BindEndingPaletteArt(installation.LoadEndingPalettes());
        game.BindRoomCharacterArt(installation.LoadRoomCharacters());
        game.BindRoomPaletteArt(installation.LoadRoomPalettes());
        game.BindRoomMetatileArt(installation.LoadRoomMetatiles());
        game.BindRoomVisualLayouts(installation.LoadRoomVisualLayouts());
        game.BindRoomPlmShotBlockVisuals(installation.LoadRoomPlmShotBlockVisuals());
        game.BindRoomPlmGrappleBlockVisuals(installation.LoadRoomPlmGrappleBlockVisuals());
        game.BindRoomPlmStationVisuals(installation.LoadRoomPlmStationVisuals());
        game.BindRoomPlmBlueDoorVisuals(installation.LoadRoomPlmBlueDoorVisuals());
        game.BindRoomPlmColoredDoorVisuals(installation.LoadRoomPlmColoredDoorVisuals());
        game.BindRoomPlmGreyDoorVisuals(installation.LoadRoomPlmGreyDoorVisuals());
        game.BindRoomPlmEyeDoorVisuals(installation.LoadRoomPlmEyeDoorVisuals());
        game.BindRoomPlmMotherBrainGlassVisuals(installation.LoadRoomPlmMotherBrainGlassVisuals());
        game.BindRoomPlmNoobTubeVisuals(installation.LoadRoomPlmNoobTubeVisuals());
        game.BindRoomPlmDownwardGateVisuals(installation.LoadRoomPlmDownwardGateVisuals());
        game.BindRoomPlmElevatorPlatformVisuals(installation.LoadRoomPlmElevatorPlatformVisuals());
        game.BindRoomPlmEscapeGateVisuals(installation.LoadRoomPlmEscapeGateVisuals());
        game.BindRoomPlmBombTorizoHandVisuals(installation.LoadRoomPlmBombTorizoHandVisuals());
        game.BindRoomPlmDraygonCannonVisuals(installation.LoadRoomPlmDraygonCannonVisuals());
        game.BindRoomPlmChozoStatueVisuals(installation.LoadRoomPlmChozoStatueVisuals());
        game.BindRoomPlmLinkedRestoreVisuals(installation.LoadRoomPlmLinkedRestoreVisuals());
        game.BindRoomPlmTourianAccessVisuals(installation.LoadRoomPlmTourianAccessVisuals());
        game.BindRoomPlmSpeedBoosterVisuals(installation.LoadRoomPlmSpeedBoosterVisuals());
        game.BindRoomPlmMaridiaElevatubeVisuals(installation.LoadRoomPlmMaridiaElevatubeVisuals());
        game.BindRoomPlmSporeSpawnCeilingVisuals(installation.LoadRoomPlmSporeSpawnCeilingVisuals());
        game.BindRoomPlmSamusEaterVisuals(installation.LoadRoomPlmSamusEaterVisuals());
        game.BindRoomPlmBotwoonWallVisuals(installation.LoadRoomPlmBotwoonWallVisuals());
        game.BindRoomPlmKraidVisuals(installation.LoadRoomPlmKraidVisuals());
        game.BindRoomPlmCrocomireVisuals(installation.LoadRoomPlmCrocomireVisuals());
        game.BindRoomPlmMotherBrainFakeDeathVisuals(installation.LoadRoomPlmMotherBrainFakeDeathVisuals());
        game.BindRoomPlmCollectibleVisuals(installation.LoadRoomPlmCollectibleVisuals());
        game.BindRoomPlmDynamicCollectibleArt(installation.LoadRoomPlmDynamicCollectibleArt());
        game.BindXrayRevealVisuals(installation.LoadXrayRevealVisuals());
        game.BindRoomBackgroundTilemapArt(installation.LoadRoomBackgroundTilemaps());
        game.BindRoomSkyTilemapArt(installation.LoadRoomSkyTilemaps());
        var projectiles=installation.LoadProjectiles();
        game.BindProjectileCompositions(projectiles.Catalog);
        game.BindProjectileFrameBindings(projectiles.FrameBindings);
        game.BindBeamArtwork(projectiles.BeamTiles);
        game.BindEnemyTileArtwork(installation.LoadEnemyTiles());
        game.BindTrailArtwork(projectiles.Trails);
        game.BindChargeFlarePlacement(projectiles.FlarePlacement);
        game.BindChargeFlareCompositions(projectiles.FlareCompositions);
        game.BindGrappleArtwork(projectiles.GrappleTiles);
        typeof(SuperMetroidGame).GetMethod("CreateGameplayRuntime",flags)!.Invoke(game,new object[]{false});
        var runtime=(SuperMetroidRuntime)typeof(SuperMetroidGame).GetField("runtime",flags)!.GetValue(game)!;
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.InitializeStartingCeresRoom();runtime.InitializeCeresStartSamus();
        runtime.System.SetEvent(EventNumber.ZebesAwake);
        runtime.System.SetBossBits(AreaId.WreckedShip, BossBits.AreaBoss);
        runtime.LoadCartridgeRoomForDebug(0xc98e);
        var samus = runtime.Samus!;
        samus.Pose = SamusPoseIds.MorphBallGroundRightPose;
        samus.CollectedItems = samus.EquippedItems = (ushort)SamusEquipmentFlags.MorphBall;
        samus.RefreshCollisionRadii(bus);
        samus.InitializeAnimation(bus);
        samus.CommitPoseHistory(bus);
        samus.AnimationFrame = 2;
        typeof(SamusState).GetProperty(nameof(SamusState.AnimationFrameTimer))!.SetValue(samus, (ushort)3);
        samus.XPosition = 0x4a * 16 + 8;
        samus.YPosition = (ushort)(0x17 * 16 - samus.Kinematics.YRadius);
        var level = runtime.LevelData!;
        var block = level.GetCollisionBlockByIndex(level.GetBlockIndex(0x4a, 0x17));
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
            ushort bombX = (ushort)(samus.XPosition - 24);
            runtime.BombProjectiles.PowerBombExplosion.Arm();
            runtime.BombProjectiles.PowerBombExplosion.Spawn(bombX, samus.YPosition);
            expectedExplosion.Arm();
            expectedExplosion.Spawn(bombX, samus.YPosition);
        }
        runtime.Plms.NotifyChozoStatueHandCollision(level, block, samus, samus.Pose, movingDown: true);
        AssertTrue(samus.StationaryScriptControlLocked && samus.InputLocked,
            "hand trigger installs native stationary handlers");
        AssertEqual((ushort)2, samus.AnimationFrame, "grab preserves captured ball frame");
        AssertEqual((ushort)3, samus.AnimationFrameTimer, "grab preserves captured ball timer");
        ushort startX = samus.XPosition, startY = samus.YPosition;
        bool carried = false;
        int frames = 0;
        while (samus.StationaryScriptControlLocked && frames < 2000)
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
            if (samus.StationaryScriptControlLocked)
            {
                AssertEqual((ushort)2, samus.AnimationFrame, "ball frame remains frozen while carried");
                AssertEqual((ushort)3, samus.AnimationFrameTimer, "ball animation timer remains frozen while carried");
            }
            carried |= samus.XPosition != startX || samus.YPosition != startY;
            frames++;
        }
        if (powerBomb)
            AssertTrue(!runtime.BombProjectiles.PowerBombExplosion.IsActive && runtime.BombProjectiles.PowerBombExplosion.Flag == 0,
                "Power Bomb completes and releases its armed flag during the statue ride");
        AssertTrue(carried, "statue still moves Samus while animation is locked");
        AssertTrue(!samus.StationaryScriptControlLocked && !samus.InputLocked && runtime.GroundedSamusMovementEnabled,
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
}
