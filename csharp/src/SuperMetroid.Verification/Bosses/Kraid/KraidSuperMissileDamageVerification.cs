using System.Reflection;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Runtime;
using static SuperMetroid.Verification.Bosses.Kraid.KraidReportedCollisionsVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Bosses.Kraid;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.SpriteObjects;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Samus.Projectiles;
namespace SuperMetroid.Verification.Bosses.Kraid;
using SuperMetroid.Verification.Harness;
/// <summary>Checks close-range Super Missile damage and native projectile slot traversal in Kraid's mouth.</summary>
internal static class KraidSuperMissileDamageVerification
{
    /// <summary>Runs two room shots through damage and deferred link cleanup, or emits the focused collision report.</summary>
    internal static int VerifyKraidSuperMissileDamage(bool collisionReport = false)
    {
        const BindingFlags flags=BindingFlags.Instance|BindingFlags.NonPublic;
        var installation = RepositoryInstallation.Installation;
        var bus=installation.OpenRuntimeAddressSpace();
        var game=new SuperMetroidGame(bus);
        var projectiles=InstalledFixtures.Projectiles;
        InstalledFixtures.BindGame(game);
        game.CreateGameplayRuntime(false);
        var runtime=game.runtime;
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom();runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.System.SetEvent(EventNumber.ZebesAwake);
        runtime.LoadCartridgeRoomForDebug(0xa59f);
        var body=runtime.Enemies.SlotTable.Slots[0];
        var state=(KraidEnemyState)(runtime.Enemies.KraidBehavior.Kraid ?? throw new InvalidOperationException("Verification requires runtime.Enemies.KraidBehavior.Kraid."));
        body.VariableA=(ushort)KraidAiFunction.MainAttackWithMouthOpen;
        body.VariableB=0x96f4;body.VariableC=64;
        (state ?? throw new InvalidOperationException("Verification requires state.")).CurrentHeadTilemap=0xa0c8;state.VulnerableMouthHitbox=0x97a0;state.InvulnerableMouthHitbox=0x97c0;
        var words=new ushort[32*64];for(int i=32*32;i<33*32;i++)words[i]=0x8000;
        var level=new RoomLevelData(new Extent16<RoomBlocks>(32, 64), new RoomLevelPlanes(words, new byte[2048], new ushort[2048]), new byte[8]);
        runtime.Room.LevelData = level;
        var samus=runtime.Player.IntroducedSamus;samus.Pose=SamusPoseId.FacingRightNormalPose;
        samus.Health=samus.MaxHealth=999;samus.SuperMissiles=samus.MaxSuperMissiles=10;samus.SelectedHudItem=2;
        samus.PoseMetadata.RefreshCollisionRadii(bus);samus.Presentation.InitializeAnimation(bus);samus.CommitPoseHistory(bus);
        samus.Position = samus.Position.WithWhole(new(120, (ushort)(512-samus.Kinematics.Radius.Y)));samus.InputLocked=false;
        runtime.Room.LoadedCamera.SetPosition(new Position16<RoomPixels>(0, 384));
        game.GameState = SuperMetroidGameState.MainGameplay;
        if (collisionReport) return VerifyKraidReportedCollisions(bus, game, runtime, body, state);
        ushort oldHealth=body.Health;
        for(int frame=0;frame<60;frame++)
        {
            var result=game.Step(frame is 0 or 24 ? (ushort)SnesButton.X : (ushort)0);
            if(body.Health!=oldHealth || frame%8==0)
            {
                Console.WriteLine($"frame={frame} hp={body.Health} ammo={samus.SuperMissiles} samus={samus.Position.X.Whole},{samus.Position.Y.Whole} mouth={state.InvulnerableMouthHitbox:X4}");
                foreach(var shot in runtime.Projectiles.Slots.Where(p=>p.IsActive))Console.WriteLine($" shot={shot.SlotIndex} type={shot.Type:X4} damage={shot.Damage} x={shot.Position.X.Whole} y={shot.Position.Y.Whole} r={shot.Radius.X},{shot.Radius.Y} pre={shot.PreInstruction}");
            }
            if (frame is 8 or 32)
            {
                AssertEqual(frame == 8 ? (ushort)700 : (ushort)400, body.Health,
                    "Each actual close-range Super Missile deals exactly 300 damage");
                AssertEqual((ushort)2, runtime.Projectiles.SlotTable.ProjectileCounter,
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
                AssertEqual((ushort)0, runtime.Projectiles.SlotTable.ProjectileCounter,
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

    /// <summary>Checks count-bounded traversal, inactive slot handling, scratch-word rejection, and adjacent bomb slots.</summary>
    private static void VerifyKraidCollisionSlotOrder(ISnesAddressSpace bus,
        SuperMetroidRuntime runtime, RoomEnemySlot body, KraidEnemyState state)
    {
        var projectiles = runtime.Projectiles;
        void Prepare(ushort count)
        {
            projectiles.Reset();
            projectiles.SlotTable.ProjectileCounter = count;
            body.Position = body.Position.WithWhole(new(176, 592));
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
            shot.Position = shot.Position.WithWhole(new(176, 486));
            shot.Radius = new(8, 8);
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
        bomb.Position = bomb.Position.WithWhole(new(176, 486));
        bomb.Radius = new(8, 8);
        runtime.Enemies.ResolveKraidProjectileHits(bus, projectiles, runtime.BombProjectiles);
        AssertEqual((ushort)1000, body.Health, "First bomb slot uses Kraid's zero bomb vulnerability");
        AssertEqual(true, new SamusProjectileDirectionWord(bomb.Direction).HasLowByteLifecycleState,
            "Count five reaches and marks the adjacent physical bomb slot");
        AssertEqual(true, runtime.Enemies.RoomSpriteObjects.Any(sprite =>
            sprite.Kind == RoomSpriteObjectKind.EnemyProjectileDud && sprite.Position.X.Whole == 176 && sprite.Position.Y.Whole == 486),
            "Non-damaging mouth callback emits its native dud sprite");
        Console.WriteLine("Kraid close-range damage, deferred owner/link cleanup, count traversal and adjacent bomb slot verified.");
    }

    /// <summary>Runs the focused reported collision path from the command dispatcher.</summary>
    [VerificationCommand("--kraid-reported-collisions")]
    internal static int KraidReportedCollisionsCommand()
    {
        return VerifyKraidSuperMissileDamage(collisionReport: true);
    }

    /// <summary>Runs the two-shot Kraid mouth damage check from the command dispatcher.</summary>
    [VerificationCommand("--kraid-super-missile-damage")]
    internal static int KraidSuperMissileDamageCommand()
    {
        return VerifyKraidSuperMissileDamage();
    }
}

