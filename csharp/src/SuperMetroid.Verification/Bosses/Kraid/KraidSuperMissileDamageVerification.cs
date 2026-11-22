using SuperMetroid.Core.Samus.Drawing;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Runtime;
using static SuperMetroid.Verification.Bosses.Kraid.KraidReportedCollisionsVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Bosses.Kraid;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Enemies.EffectActors;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Bosses.Kraid;

using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Rooms;
/// <summary>Checks close-range Super Missile damage and native projectile slot traversal in Kraid's mouth.</summary>
internal static class KraidSuperMissileDamageVerification
{
    /// <summary>Runs two room shots through damage and deferred link cleanup, or emits the focused collision report.</summary>
    internal static int VerifyKraidSuperMissileDamage(bool collisionReport = false)
    {
        var game=SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.CreateGame(new SuperMetroid.Core.Saves.SaveSlotStore(SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.Maps));
        var projectiles=InstalledFixtures.Projectiles;
        InstalledFixtures.BindGame(game);
        game.Session.CreateGameplayRuntime(GameplayRuntimePurpose.SelectedGame);
        var runtime = game.Session.GameplayRuntime;
        runtime.HudSetup.InitializeHud(HudSnapshot.NewGame);
        runtime.RoomLoader.InitializeStartingCeresRoom();runtime.RoomLoader.InitializeCeresStartSamus(runtime.Room.LoadedScope);
        runtime.System.SetEvent(StoryEvent.ZebesAwake);
        runtime.LoadCartridgeRoomForDebug(RoomDefinitions.Kraid);
        var body=runtime.Enemies.Places.InUpdateOrder[0];
        var state= runtime.Enemies.Kraid ?? throw new InvalidOperationException("Verification requires runtime.Enemies.Kraid.");
        state.BodyState.FightPhase = KraidFightPhase.MainAttackWithMouthOpen;
        state.HeadPlayback=RoarPastOpenPose;state.HeadHold.LastFor(64);
        state.DisplayedHeadFrame = (global::SuperMetroid.Core.Bosses.Kraid.KraidHeadFrame?)(FullyOpenRoarFrame());
        var scope = runtime.Room.Scope ?? throw new InvalidOperationException("Verification requires a loaded room.");
        var level = KraidFloorLevel(scope.LevelData);
        runtime.Room.Adopt(scope with { LevelData = level });
        var samus=runtime.Player.Samus;samus.ChangePoseKeepingAnimation(SamusPoseId.FacingRightNormalPose);
        samus.Health=samus.MaxHealth=999;samus.SuperMissiles=samus.MaxSuperMissiles=10;samus.SelectedHudItem=HudItem.SuperMissile;
        SamusPoseMetadata.RefreshCollisionRadii(samus);SamusPresentation.InitializeAnimation(samus);samus.CommitPoseHistory();
        samus.Position = samus.Position.WithWhole(new(120, (ushort)(512-samus.Kinematics.Radius.Y)));samus.Control.Unlock();
        runtime.Room.LoadedCamera.SetPosition(new Position16<RoomPixels>(0, 384));
        game.Session.Enter(SuperMetroidGameState.MainGameplay);
        if (collisionReport) return VerifyKraidReportedCollisions(game, runtime, body, state);
        // Kraid's arm swipes reach this standing position from its sixth frame; contact knockback would move Samus off
        // the mouth's height before the second missile. Contact protection keeps the check on the missiles alone.
        samus.InvincibilityTimer = WrappingCountdown.Start(60);
        int oldHealth=body.Health;
        for(int frame=0;frame<60;frame++)
        {
            var result=game.Step(frame is 0 or 24 ? ControllerButton.X : 0);
            if(body.Health!=oldHealth || frame%8==0)
            {
                Console.WriteLine($"frame={frame} hp={body.Health} ammo={samus.SuperMissiles} samus={samus.Position.X.Whole},{samus.Position.Y.Whole} samusHp={samus.Health} pose={samus.Pose} mouth={state.DisplayedHeadFrame?.InvulnerableShape}");
                foreach(var shot in runtime.Projectiles.Shots.Where(p=>p.IsActive))Console.WriteLine($" shot={shot.Place} type={shot.HeldShot().Type} damage={shot.HeldShot().Damage} x={shot.Position.X.Whole} y={shot.Position.Y.Whole} r={shot.Radius.X},{shot.Radius.Y} pre={shot.Behavior}");
            }
            if (frame is 8 or 32)
            {
                AssertEqual(frame == 8 ? (ushort)700 : (ushort)400, body.Health,
                    "Each actual close-range Super Missile deals exactly 300 damage");
                AssertEqual(2, runtime.Projectiles.Pool.ShotCount.Count,
                    "Mouth collision defers owner/link removal to projectile pre-instructions");
                AssertTrue(runtime.Projectiles.Shots[0].KindStateOrNone() is SuperMissileShotState
                    && !runtime.Projectiles.Shots[0].IsSuperMissileLink(runtime.Projectiles.Shots), "Owner retains native pre-instruction");
                AssertTrue(runtime.Projectiles.Shots[1].IsSuperMissileLink(runtime.Projectiles.Shots), "Link retains native pre-instruction");
                AssertEqual(true, runtime.Projectiles.Shots[1].IsSpent,
                    "Descending mouth pass marks the overlapping link for native cleanup");
            }
            if (frame is 9 or 33)
            {
                AssertEqual(0, runtime.Projectiles.Pool.ShotCount.Count,
                    "Marked link clears itself and its owner on the next projectile update");
                AssertEqual(false, runtime.Projectiles.Shots.Any(shot => shot.IsActive),
                    "No stranded Super Missile or link survives the hit");
            }
            oldHealth=body.Health;
        }
        Console.WriteLine($"final hp={body.Health} ammo={samus.SuperMissiles}");
        AssertEqual(400, body.Health, "Two actual close-range Super Missiles must deal 600 damage, including their spawned links");
        AssertEqual(8, samus.SuperMissiles, "Exactly two Super Missile ammo units consumed");
        // Native first mouth hit leaves DP $12 = projectile type $8200.
        // $A7:B001 then CMPs the next shot's Y-radius-1 against that clobbered word.
        ushort nextTop=unchecked(486 - 8 - 1);
        ushort nativeCompare=unchecked((ushort)(nextTop-0x8200));
        Console.WriteLine($"native next top={nextTop:X4} clobberedBottom=8200 CMP={nativeCompare:X4} BPL-reject={(nativeCompare&0x8000)==0}");
        if((nativeCompare&0x8000)!=0)throw new InvalidDataException("Native scratch-word rejection is not satisfied by the reproduced coordinates.");
        Suite(nameof(VerifyKraidCollisionPlaceOrder), () => VerifyKraidCollisionPlaceOrder(runtime, body, state));
        return 0;
    }

    /// <summary>Blocks across the fixture's arena: Kraid's room is two screens wide.</summary>
    private const int ArenaWidth = 32;

    /// <summary>Rows in the fixture's arena: Kraid's room extended downward, so its floor and Kraid's body lie inside the grid.</summary>
    private const int ArenaHeight = 64;

    /// <summary>The arena's one solid row, which Samus stands on at height 512.</summary>
    private const int FloorRow = 32;

    /// <summary>
    /// Kraid's room as a flat arena: one solid row, open air everywhere else and no block behaviours, over a grid extended
    /// below the room. Each block shows what Kraid's room shows in its column, the rows past the room's last repeating that
    /// row, so every block the arena shows is one the Kraid tileset's sheet pictures (#1677); only collision is the fixture's.
    /// </summary>
    /// <exception cref="InvalidDataException">Kraid's room is not as wide as the arena.</exception>
    private static RoomLevelData KraidFloorLevel(RoomLevelData room)
    {
        if (room.Size.X != ArenaWidth)
            throw new InvalidDataException($"Kraid's room is {room.Size.X} blocks wide; the fixture's arena is {ArenaWidth}.");
        var background = room.BackgroundPlane.Cells.Cells;
        var terrain = new RoomTerrainBlock[ArenaWidth * ArenaHeight];
        var backdrop = new RoomBlockAppearance[terrain.Length];
        for (int index = 0; index < terrain.Length; index++)
        {
            int row = index / ArenaWidth;
            int shown = Math.Min(row, room.Size.Y - 1) * ArenaWidth + index % ArenaWidth;
            terrain[index] = new RoomTerrainBlock(room.ShownForegroundGrid[shown], row == FloorRow ? RoomCollisionType.SolidBlock : RoomCollisionType.Air);
            backdrop[index] = background[shown].Appearance;
        }
        return new RoomLevelData(new Extent16<RoomBlocks>(ArenaWidth, ArenaHeight),
            new RoomLevelPlanes(terrain, new RoomBlockBehavior[terrain.Length], backdrop), RoomLevelData.NothingBeyondGrid(), []);
    }

    /// <summary>The roar's fully open head pose, with its two open-mouth collision shapes.</summary>
    private static KraidHeadFrame FullyOpenRoarFrame() => KraidHeadFrame.Open;

    /// <summary>The roar cursor just past its fully open pose, where the native cursor stands during the open-mouth attack.</summary>
    private static SuperMetroid.Core.Animation.AnimationPlayback<KraidHeadSegment> RoarPastOpenPose => SuperMetroid.TestSupport.MovieReplay.KraidHeadNativeCursors.PlaybackAtStep(KraidHeadSegment.Roar, 5);

    /// <summary>Checks count-bounded traversal, inactive slot handling, scratch-word rejection, and adjacent bomb slots.</summary>
    private static void VerifyKraidCollisionPlaceOrder(
        SuperMetroidRuntime runtime, RoomEnemyPlace body, KraidEnemyState state)
    {
        var projectiles = runtime.Projectiles;
        void Prepare(ushort count)
        {
            projectiles.Reset();
            projectiles.Pool.ShotCount.Restore(count);
            body.Position = body.Position.WithWhole(new(176, 592));
            body.Health = 1000;
            state.BodyState.FightPhase = KraidFightPhase.MainAttackWithMouthOpen;
            state.HeadPlayback = RoarPastOpenPose;
            state.DisplayedHeadFrame = (global::SuperMetroid.Core.Bosses.Kraid.KraidHeadFrame?)(FullyOpenRoarFrame());
        }
        void Place(int index) => PlaceShot(index, SamusProjectileType.Of(SamusProjectileFamily.SuperMissile), 300);
        // Each case fires a new shot into the place, ending the one an earlier case left there.
        void PlaceShot(int index, SamusProjectileType type, int damage)
        {
            var shot = projectiles.Shots[index];
            shot.End();
            shot.Occupy(type, damage);
            shot.HeldShot().Position = shot.Position.WithWhole(new(176, 486));
            shot.HeldShot().Radius = new(8, 8);
            // Intentionally inactive: this native private pass has no sentinel check.
        }

        Prepare(0);
        Place(0);
        runtime.Enemies.ResolveKraidProjectileHits(projectiles, runtime.BombProjectiles);
        AssertEqual(1000, body.Health, "Zero projectile count skips even overlapping slot zero");

        Prepare(1);
        Place(4);
        runtime.Enemies.ResolveKraidProjectileHits(projectiles, runtime.BombProjectiles);
        AssertEqual(1000, body.Health, "Counter-derived native traversal excludes late slots");

        Prepare(1);
        Place(1);
        Place(0);
        runtime.Enemies.ResolveKraidProjectileHits(projectiles, runtime.BombProjectiles);
        AssertEqual(700, body.Health,
            "Inclusive count slot hits without an activity sentinel; clobbered scratch rejects slot zero");
        AssertEqual(true, projectiles.Shots[1].IsSpent,
            "The inclusive count slot, not only count minus one, receives the collision");

        Prepare(1);
        PlaceShot(1, SamusProjectileType.Beam(SamusBeamCombination.Power, charged: true), 60);
        PlaceShot(0, SamusProjectileType.Beam(SamusBeamCombination.Power, charged: true), 60);
        runtime.Enemies.ResolveKraidProjectileHits(projectiles, runtime.BombProjectiles);
        // A charged beam's hit lets later shots reach the mouth only from a row far below the room (#1470, #2507).
        AssertEqual(940, body.Health,
            "The first charged shot's reach rejects a second charged shot at the same height");

        // A missile's hit lets later shots reach the mouth from row 257 down, so a second missile at the same height follows
        // the first in and takes the same damage (#2507).
        int HealthAfterMissiles(params int[] places)
        {
            Prepare(1);
            foreach (int place in places)
                PlaceShot(place, SamusProjectileType.Of(SamusProjectileFamily.Missile), 100);
            runtime.Enemies.ResolveKraidProjectileHits(projectiles, runtime.BombProjectiles);
            return body.Health;
        }
        int afterOne = HealthAfterMissiles(1);
        AssertTrue(afterOne < 1000, "a close-range missile damages Kraid's mouth");
        AssertEqual(2 * (1000 - afterOne), 1000 - HealthAfterMissiles(1, 0),
            "The first missile's reach lets a second missile at the same height hit");

        Prepare(5);
        var bomb = runtime.BombProjectiles.Bombs[0];
        bomb.Occupy(SamusBombKind.Bomb, 30);
        bomb.HeldBomb().Position = bomb.Position.WithWhole(new(176, 486));
        bomb.HeldBomb().Radius = new(8, 8);
        runtime.Enemies.ResolveKraidProjectileHits(projectiles, runtime.BombProjectiles);
        AssertEqual(1000, body.Health, "First bomb slot uses Kraid's zero bomb vulnerability");
        AssertEqual(true, bomb.IsSpent,
            "Count five reaches and marks the adjacent physical bomb slot");
        AssertEqual(true, runtime.Enemies.RoomEffectActors.Any(sprite =>
            sprite.Shows(RoomEffectActorKind.DudShot) && sprite.Position.X.Whole == 176 && sprite.Position.Y.Whole == 486),
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

