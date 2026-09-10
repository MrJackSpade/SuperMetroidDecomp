using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Runtime;

public sealed partial class SuperMetroidRuntime
{
    // Retain the existing beam/boss hit publication after EnemyMain. Ordinary
    // bombs now have their own per-enemy pre-AI dispatch; they must not run here.
    private void ResolveUpdatedBeamHits()
    {
        Enemies.ResolveCeresRidleyProjectileHits(
            _addressSpace,
            Projectiles,
            BombProjectiles);
        Enemies.ResolveKraidProjectileHits(
            _addressSpace,
            Projectiles,
            BombProjectiles);
        Enemies.ResolvePhantoonProjectileHits(
            _addressSpace,
            Projectiles,
            BombProjectiles);
        Enemies.ResolveOrdinaryProjectileHits(
            _addressSpace,
            Projectiles,
            BombProjectiles,
            Samus);
    }

    // Terrain preparation owns the external movement words before alpha. Enemy
    // actors subsequently add platform displacement; beta consumes it unchanged.
    private void PrepareEnemyFrame()
    {
        if (Camera is not null && Enemies.IsLoaded)
        {
            // Samus's bank-$94 collision phase follows EnemyMain. A pseudo-door contact
            // published last frame therefore becomes `$0E16=1` immediately before this
            // frame's elevator actor dispatcher, preserving the native producer order.
            if (LevelData?.ConsumeElevatorDoorContact() == true)
                Enemies.PublishElevatorDoorContact();
            if (Samus is not null)
            {
                // `$94:9B60-$9B72` clears all four external-displacement words before
                // EnemyMain. Rideable enemies then accumulate accepted platform deltas and
                // bank `$90` consumes those live values during Samus movement below. They
                // are producer-owned words, so movement deliberately does not clear them.
                Samus.Kinematics.ExtraXSubdisplacement = 0;
                Samus.Kinematics.ExtraXDisplacement = 0;
                Samus.Kinematics.ExtraYSubdisplacement = 0;
                Samus.Kinematics.ExtraYDisplacement = 0;

                if (LevelData is null || ActiveRoom is null)
                {
                    throw new InvalidOperationException(
                        "Live Samus terrain reactions require an active cartridge room.");
                }
                SamusTerrainHazardCollision.PrepareFrame(
                    _addressSpace,
                    LevelData,
                    Samus,
                    ActiveRoom.AreaIndex,
                    System.HasAnyBossBits(ActiveRoom.AreaIndex, BossBits.AreaBoss));
            }
        }
    }

    // GameState_8 runs this after Samus alpha/projectile update and before beta
    // movement. Keep actor side effects together at that shared frame boundary.
    private void RunEnemyMainPhase()
    {
        if (Camera is not null && Enemies.IsLoaded)
        {
            Enemies.StepFrame(
                Camera.XPosition,
                Camera.YPosition,
                TimeIsFrozen,
                Samus,
                Controller1.NewlyPressed,
                LevelData,
                Controller1.Current,
                Projectiles,
                NmiFrameCounter8,
                ActiveSamusMode7Transform,
                BombProjectiles,
                VramWrites,
                resolveSamusContactBeforeAi: true);
            if (Enemies.LastElevatorEvent == ElevatorFrameEvent.DepartureStarted)
            {
                // MakeSamusFaceForward clears all pending pose requests after alpha
                // has sampled this frame's controls. Do not apply that stale Down
                // crouch request to the new forward-facing, elevator-owned body.
                ProspectiveSamusPose = null;
                ProspectiveSamusFallbackPose = null;
            }
            if (!TimeIsFrozen && Enemies.MotherBrain is { Head: { } rainbowHead } rainbowBrain)
                rainbowBrain.RainbowBeamHdma.Step(_addressSpace, rainbowBrain.RainbowBeamHdmaActive,
                    rainbowHead.XPosition, rainbowHead.YPosition,
                    rainbowBrain.RainbowBeamAngle, rainbowBrain.RainbowBeamAngularWidth);
            if (Enemies.Phantoon is { } phantoon)
            {
                // Phantoon's body is BG2 artwork anchored by the bank-$A7 scroll writes,
                // while eye/tentacle collision follows enemy positions. Publish those
                // writes before the next accepted NMI latches the matching OAM frame.
                // The room's fixed layer-two axes preserve them during camera scrolling.
                BackgroundScroll.SetBg2ScrollRegisters(
                    phantoon.Bg2HorizontalScroll, phantoon.Bg2VerticalScroll);
            }
            if (Enemies.CeresEscapeStartedThisFrame)
            {
                // $A6:C117 publishes these global side effects on the same EnemyMain call
                // that changes ceres_status from one to two. Keep the actor as producer,
                // but apply timer and boss state in their existing runtime-owned systems.
                EscapeTimer.RequestCeresStart();
                if (ActiveRoom is null)
                    throw new InvalidOperationException("Ceres escape started without an active room.");
                System.SetBossBits(ActiveRoom.AreaIndex, BossBits.AreaBoss);
            }
            if (Enemies.RequestedShitroidCameraX is ushort shitroidCameraX)
            {
                // `$A9:EFE6` writes layer1_x_pos during EnemyMain, before the ordinary
                // scrolling routine later in this frame. Do not route it through entry
                // placement, which would clear subposition and ideal-camera state.
                Camera.SetLayerOneXFromEnemyAi(shitroidCameraX);
                BackgroundScroll.Layer1XPosition = shitroidCameraX;
            }
            if (Enemies.ElevatorClearedProjectileData)
            {
                // `$90:ADB7` clears all ten projectile slots and their counters. Ordinary
                // beam/missile slots were reset inside the actor call; bombs and the shared
                // cooldown live in this companion owner and complete that same operation.
                BombProjectiles.Reset();
            }
            // `$A6:A2DF` does not install the post-enemy hook until Ridley's animation word
            // becomes nonzero. Before the reveal it branches directly into `$A6:A2E3`
            // during EnemyMain, so emit the Baby/door OBJ now—before queued enemy layers.
            Enemies.DrawCeresRidleyImmediateBabyAndDoor(
                Oam,
                Camera.XPosition,
                Camera.YPosition);
            if (Samus is not null && !TimeIsFrozen)
            {
                Enemies.ResolveRidleySamusContact(
                    Samus,
                    Controller1.Current);
            }
            if (Samus is not null)
            {
                Samus.Kinematics.InteractiveEnemies = Enemies.InteractiveCollisionBodies;
                if (Enemies.LastGunshipEvent == GunshipFrameEvent.EntryStarted)
                {
                    // MakeSamusFaceForward performs a direct suit-palette reload and the
                    // ship explicitly clears elevator status after installing locked demo
                    // handlers. The enemy system owns pose/motion; these two global words
                    // remain runtime-owned and are applied on its typed event boundary.
                    Samus.LoadSuitPalette(_addressSpace, Cgram);
                    ElevatorStatus = 0;
                }
                else if (Enemies.LastGunshipEvent == GunshipFrameEvent.LandingCompleted)
                {
                    // `$A2:A987` restores the ordinary Samus handler pair only after the
                    // closing-pad hold. Enemy AI has already unlocked Samus.InputLocked;
                    // publish the runtime's matching movement gate at the same boundary.
                    GroundedSamusMovementEnabled = true;
                }
            }
        }
    }
}
