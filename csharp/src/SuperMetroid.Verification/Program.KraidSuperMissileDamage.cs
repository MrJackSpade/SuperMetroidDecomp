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
    private static int VerifyKraidSuperMissileDamage(bool collisionReport = false)
    {
        const BindingFlags flags=BindingFlags.Instance|BindingFlags.NonPublic;
        var installation = runtimeFixtureInstallation.Value;
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
        runtime.LoadCartridgeRoomForDebug(0xa59f);
        var body=runtime.Enemies.Slots[0];
        var state=(KraidEnemyState)typeof(RoomEnemySystem).GetField("_kraidState",flags)!.GetValue(runtime.Enemies)!;
        body.VariableA=(ushort)KraidAiFunction.MainAttackWithMouthOpen;
        body.VariableB=0x96f4;body.VariableC=64;
        state.CurrentHeadTilemap=0xa0c8;state.VulnerableMouthHitbox=0x97a0;state.InvulnerableMouthHitbox=0x97c0;
        var words=new ushort[32*64];for(int i=32*32;i<33*32;i++)words[i]=0x8000;
        var level=new RoomLevelData(32,64,words,new byte[2048],new ushort[2048],new byte[8]);
        typeof(SuperMetroidRuntime).GetProperty("LevelData")!.SetValue(runtime,level);
        var samus=runtime.Samus!;samus.Pose=SamusPoseIds.FacingRightNormalPose;
        samus.Health=samus.MaxHealth=999;samus.SuperMissiles=samus.MaxSuperMissiles=10;samus.SelectedHudItem=2;
        samus.RefreshCollisionRadii(bus);samus.InitializeAnimation(bus);samus.CommitPoseHistory(bus);
        samus.XPosition=120;samus.YPosition=(ushort)(512-samus.Kinematics.YRadius);samus.InputLocked=false;
        runtime.Camera!.SetPosition(0,384);
        typeof(SuperMetroidGame).GetProperty("GameState")!.SetValue(game,SuperMetroidGameState.MainGameplay);
        if (collisionReport) return VerifyKraidReportedCollisions(bus, game, runtime, body, state);
        var renderer=new CartridgeAudioRenderer(installation.LoadAudio());
        ushort oldHealth=body.Health;
        for(int frame=0;frame<60;frame++)
        {
            var result=game.Step(frame is 0 or 24 ? (ushort)SnesButton.X : (ushort)0);
            renderer.RenderFrame(result.AudioCommands);game.SetAudioAcknowledgements(renderer.ReadAcknowledgements());
            if(body.Health!=oldHealth || frame%8==0)
            {
                Console.WriteLine($"frame={frame} hp={body.Health} ammo={samus.SuperMissiles} samus={samus.XPosition},{samus.YPosition} mouth={state.InvulnerableMouthHitbox:X4}");
                foreach(var shot in runtime.Projectiles.Slots.Where(p=>p.IsActive))Console.WriteLine($" shot={shot.SlotIndex} type={shot.Type:X4} damage={shot.Damage} x={shot.XPosition} y={shot.YPosition} r={shot.XRadius},{shot.YRadius} pre={shot.PreInstruction}");
            }
            if (frame is 8 or 32)
            {
                AssertEqual(frame == 8 ? (ushort)700 : (ushort)400, body.Health,
                    "Each actual close-range Super Missile deals exactly 300 damage");
                AssertEqual((ushort)2, runtime.Projectiles.ProjectileCounter,
                    "Mouth collision defers owner/link removal to projectile pre-instructions");
                AssertEqual(SamusProjectilePreInstruction.SuperMissile,
                    runtime.Projectiles.Slots[0].PreInstruction, "Owner retains native pre-instruction");
                AssertEqual(SamusProjectilePreInstruction.SuperMissileLink,
                    runtime.Projectiles.Slots[1].PreInstruction, "Link retains native pre-instruction");
                AssertEqual(true, runtime.Projectiles.Slots[1].PackedDirection.HasLowByteLifecycleState,
                    "Descending mouth pass marks the overlapping link for native cleanup");
            }
            if (frame is 9 or 33)
            {
                AssertEqual((ushort)0, runtime.Projectiles.ProjectileCounter,
                    "Marked link clears itself and its owner on the next projectile update");
                AssertEqual(false, runtime.Projectiles.Slots.Any(shot => shot.IsActive),
                    "No stranded Super Missile or link survives the hit");
            }
            oldHealth=body.Health;
        }
        Console.WriteLine($"final hp={body.Health} ammo={samus.SuperMissiles}");
        AssertEqual((ushort)400, body.Health, "Two actual close-range Super Missiles must deal 600 damage, including their spawned links");
        AssertEqual((ushort)8, samus.SuperMissiles, "Exactly two Super Missile ammo units consumed");
        // Native first mouth hit leaves DP $12 = projectile type $8200.
        // $A7:B001 then CMPs the next shot's Y-radius-1 against that clobbered word.
        ushort nextTop=unchecked((ushort)(486-8-1));
        ushort nativeCompare=unchecked((ushort)(nextTop-0x8200));
        Console.WriteLine($"native next top={nextTop:X4} clobberedBottom=8200 CMP={nativeCompare:X4} BPL-reject={(nativeCompare&0x8000)==0}");
        if((nativeCompare&0x8000)!=0)throw new InvalidDataException("Native scratch-word rejection is not satisfied by the reproduced coordinates.");
        Suite(nameof(VerifyKraidCollisionSlotOrder), () => VerifyKraidCollisionSlotOrder(bus, runtime, body, state));
        return 0;
    }

    private static void VerifyKraidCollisionSlotOrder(ISnesAddressSpace bus,
        SuperMetroidRuntime runtime, RoomEnemySlot body, KraidEnemyState state)
    {
        var projectiles = runtime.Projectiles;
        void Prepare(ushort count)
        {
            projectiles.Reset();
            typeof(SamusProjectileSystem).GetProperty(nameof(SamusProjectileSystem.ProjectileCounter))!
                .SetValue(projectiles, count);
            body.XPosition = 176;
            body.YPosition = 592;
            body.Health = 1000;
            body.VariableA = (ushort)KraidAiFunction.MainAttackWithMouthOpen;
            body.VariableB = 0x96f4;
            state.InvulnerableMouthHitbox = 0x97c0;
            state.VulnerableMouthHitbox = 0x97a0;
        }
        void Place(int index)
        {
            var shot = projectiles.Slots[index];
            shot.Type = 0x8200;
            shot.Damage = 300;
            shot.XPosition = 176;
            shot.YPosition = 486;
            shot.XRadius = shot.YRadius = 8;
            // Intentionally inactive: this native private pass has no sentinel check.
        }

        Prepare(0);
        Place(0);
        runtime.Enemies.ResolveKraidProjectileHits(bus, projectiles, runtime.BombProjectiles);
        AssertEqual((ushort)1000, body.Health, "Zero projectile count skips even overlapping slot zero");

        Prepare(1);
        Place(4);
        runtime.Enemies.ResolveKraidProjectileHits(bus, projectiles, runtime.BombProjectiles);
        AssertEqual((ushort)1000, body.Health, "Counter-derived native traversal excludes late slots");

        Prepare(1);
        Place(1);
        Place(0);
        runtime.Enemies.ResolveKraidProjectileHits(bus, projectiles, runtime.BombProjectiles);
        AssertEqual((ushort)700, body.Health,
            "Inclusive count slot hits without an activity sentinel; clobbered scratch rejects slot zero");
        AssertEqual(true, projectiles.Slots[1].PackedDirection.HasLowByteLifecycleState,
            "The inclusive count slot, not only count minus one, receives the collision");

        Prepare(1);
        Place(1);
        Place(0);
        foreach (var shot in projectiles.Slots.Take(2))
        {
            shot.Type = 0x8010;
            shot.Damage = 60;
        }
        runtime.Enemies.ResolveKraidProjectileHits(bus, projectiles, runtime.BombProjectiles);
        AssertEqual((ushort)880, body.Health,
            "Charged shots whose clobbered comparisons still pass both damage Kraid; no one-hit cap");

        Prepare(5);
        var bomb = runtime.BombProjectiles.Slots[0];
        bomb.Type = SamusBombProjectileSystem.NormalBombType;
        bomb.Damage = 30;
        bomb.XPosition = 176;
        bomb.YPosition = 486;
        bomb.XRadius = bomb.YRadius = 8;
        runtime.Enemies.ResolveKraidProjectileHits(bus, projectiles, runtime.BombProjectiles);
        AssertEqual((ushort)1000, body.Health, "First bomb slot uses Kraid's zero bomb vulnerability");
        AssertEqual(true, new SamusProjectileDirectionWord(bomb.Direction).HasLowByteLifecycleState,
            "Count five reaches and marks the adjacent physical bomb slot");
        AssertEqual(true, runtime.Enemies.RoomSpriteObjects.Any(sprite =>
            sprite.Kind == RoomSpriteObjectKind.EnemyProjectileDud && sprite.XPosition == 176 && sprite.YPosition == 486),
            "Non-damaging mouth callback emits its native dud sprite");
        Console.WriteLine("Kraid close-range damage, deferred owner/link cleanup, count traversal and adjacent bomb slot verified.");
    }
}

