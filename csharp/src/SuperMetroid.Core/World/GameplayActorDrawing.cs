using SuperMetroid.Core.Samus.Death;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Enemies.Elevator;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Samus.Grapple;
using SuperMetroid.Core.Samus.Colours;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Samus.Drawing;
using SuperMetroid.Core.Gameplay;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.World;

/// <summary>
/// Draws the gameplay actors each frame (Samus, arm cannon, projectiles, grapple, enemies) and publishes what
/// that drawing did.
/// </summary>
internal sealed class GameplayActorDrawing
{
    /// <summary>Supplies installed frames, projectile compositions, and suit colours artwork.</summary>
    private readonly RuntimeArtwork _artwork;
    /// <summary>Provides the attract-demo state that can alter ordinary Samus presentation.</summary>
    private readonly IAttractDemoState _attractSession;
    /// <summary>Supplies the current input state used by Samus's presentation.</summary>
    private readonly GameplayActionInput _playerActions;
    /// <summary>Controls gameplay display modes that can suppress ordinary Samus drawing.</summary>
    private readonly GameplayDisplayState _display;
    /// <summary>Determines whether time-dependent enemy projectile drawing is suspended.</summary>
    private readonly GameplayTimeFreeze _freeze;
    /// <summary>Provides frame-counter cadence for blinking and drawing branches.</summary>
    private readonly FrameClock _clock;

    /// <summary>Receives Samus's whole-frame draws in the same order as the sprites around them.</summary>
    private readonly SpriteFrameBuffer _sprites;
    /// <summary>Supplies Samus pose, colours, grapple state, and movement effects.</summary>
    private readonly SamusFrameState _player;

    /// <summary>Binds the shared runtime owners whose state determines each actor drawing pass.</summary>
    internal GameplayActorDrawing(GameplayActorDrawingDependencies dependencies)
    {
        _artwork = dependencies.Artwork;
        _attractSession = dependencies.AttractSession;
        _playerActions = dependencies.PlayerActions;
        _display = dependencies.Display;
        _freeze = dependencies.Freeze;
        _clock = dependencies.Clock;
        _sprites = dependencies.Sprites;
        _player = dependencies.Player;
    }

    /// <summary>What Samus's projectile, bomb, flare and trail draws show: each the sheet of its own beams (#1586).</summary>
    private ProjectileDrawArtwork ProjectileArtwork => new(_artwork.ProjectileSheets);
    /// <summary>
    /// Shared enemy-layer/Samus/projectile drawing, independently of movement. <paramref name="deathStep"/> is the frame's
    /// death-sequence step, which a frame the death sequence owns draws.
    /// </summary>
    internal void DrawGameplayActors(RoomScope room, SamusState samus, SamusDrawOwner owner, SamusColourEffects colourEffects,
        SamusDeathSequenceStepResult? deathStep)
    {
        bool deathOwnsSamus = owner == SamusDrawOwner.DeathSequence;
        // The frame draws bomb/projectile explosions before reaching the enemy-layer
        // phase that draws Samus and her projectiles, so the explosions lie beneath them.
        if (!deathOwnsSamus)
        {
            room.BombProjectiles.Draw(_sprites, room.Camera.PixelPosition, ProjectileArtwork);
            room.Projectiles.DrawExplosions(_sprites, room.Camera.PixelPosition, ProjectileArtwork);
        }

        // Enemy projectiles in front of Samus, and the room's own actors that stand in front of her, follow the bomb and
        // projectile explosions and precede Samus's draw.
        if (!deathOwnsSamus && !samus.Xray.AreEnemyProjectilesSuspended)
        {
            // The Ceres elevator is drawn in front of Samus, as both its definitions are. Projectiles in front of
            // Samus are drawn here, before Samus.
            room.CeresElevatorArrival?.Draw(_sprites, room.Enemies.Presentation, room.Camera.PixelPosition);
            // A hit burst among the effect actors is one of Samus's weapon effects: it shows the sheet of the beams she
            // has equipped as it is drawn, as her charge flare does (#1713).
            room.Enemies.Projectiles.DrawEnemyProjectilesInFrontOfSamus(
                _sprites,
                room.Camera.PixelPosition,
                new SamusHitBurstArtwork(ProjectileArtwork, SamusBeamCombinations.Of(samus.EquippedBeams)),
                _freeze.TimeIsFrozen);
        }

        if (colourEffects == SamusColourEffects.Advance)
        {
            // Samus's colours update during gameplay, just before she is drawn, so the picture drawn after this step
            // shows every colour it sets: the full-boost glow out of liquid and the normal suit a cancelled move asks for.
            // The drained or rainbow colouring comes first in the colour update, and nothing else in it runs after it.
            // Its one-shot suit restore also stands for the suit shade
            // shown at once when the drained Samus falls and when her rainbow stops.
            // A cancelled move or a new pose asks for the normal suit during movement, before this colour step. That
            // shade shows first, so the charge and drained colourings of the same update show over it.
            if (!deathOwnsSamus)
                samus.HorizontalSpeed.ShowDueSuitShade(
                    _player.Colours, samus.EquippedItems);
            bool drainedOwnsSamusColours = !deathOwnsSamus &&
                samus.Drained.UpdateColouring(_player.Colours, samus.EquippedItems);
            bool chargeGlowRestoredSuitShade = false;
            if (!deathOwnsSamus && !drainedOwnsSamusColours)
            {
                // The colour update always runs charge/post-shot handling before running the
                // selected special Samus colouring. Ordinary charged shots paint all of Samus's visible colours
                // white for three updates and restore the suit on the fourth; Hyper shots
                // step ten complete shades on alternating updates before restore.
                // A restored suit from the beam-charge colour step bypasses the entire special
                // colour step on glow expiry. In particular, stored shine must not tick
                // or overwrite the restored suit on this frame.
                chargeGlowRestoredSuitShade = room.Projectiles.UpdateBeamChargeGlow(
                    _player.Colours,
                    samus,
                    room.FxOverlay.VisorAnimation);

                if (!chargeGlowRestoredSuitShade)
                    samus.HorizontalSpeed.UpdateSpeedBoosterGlow(_player.Colours, new SpeedBoosterGlowFrame { MovementType = SamusPoseMetadata.ReadMovementType(samus), AnimationOnSpinStart = samus.AnimationIsOn(SamusAnimationFrames.ScrewAttackSpinStart), AnimationPastScrewAttackSpin = samus.AnimationHasReached(SamusAnimationFrames.ScrewAttackWallJumpReady), AnimationBeforeWallJumpSpin = !samus.AnimationHasReached(SamusAnimationFrames.WallJumpOrdinarySpinStart), EquippedItems = samus.EquippedItems, SuppressActiveSpeedBoosterGlow = samus.SpecialColouring.Holder != SamusSpecialColouringKind.None, BottomBoundarySubmerged = samus.LiquidPhysics.IsBottomBoundarySubmerged(samus) });
            }
            // The shinespark colouring runs at the same point of the colour step. It
            // executes only when charge handling did not restore the suit.
            if (!deathOwnsSamus && !drainedOwnsSamusColours && !chargeGlowRestoredSuitShade)
            {
                samus.Shinespark.UpdateColouring(
                    _player.Colours,
                    samus.EquippedItems);
            }
            // The Crystal Flash colouring owns the colours of Samus's shots and bombs during Crystal
            // Flash. It is mutually exclusive with the shinespark and X-ray colouring but
            // intentionally runs at the same point of the colour step.
            if (!deathOwnsSamus && !drainedOwnsSamusColours && !chargeGlowRestoredSuitShade)
            {
                samus.CrystalFlash.UpdateColouring(
                    _player.Colours,
                    samus);
            }
            // The X-ray colouring changes only one of the visor's colours while active; its teardown
            // restores the complete Power/Varia/Gravity suit colours once.
            if (!deathOwnsSamus && !drainedOwnsSamusColours && !chargeGlowRestoredSuitShade)
            {
                samus.Xray.UpdateColouring(
                    _player.Colours,
                    samus.EquippedItems);
            }
            // The Metroid-drain colour step runs after every charge and special-colouring family. A Metroid
            // holding Samus makes her alternate between the boost glow and the normal suit, and the ordinary hurt
            // flash does not run, just as it does not under the drained or rainbow colouring.
            bool metroidOwnsSamusColours = !deathOwnsSamus &&
                !drainedOwnsSamusColours &&
                SamusMetroidAttachmentFlash.Update(_player.Colours, samus);
            if (!deathOwnsSamus && !drainedOwnsSamusColours && !metroidOwnsSamusColours)
            {
                SamusHurtFlash.Update(
                    _player.Colours,
                    samus,
                    _playerActions.Current);
            }

        }

        if (deathOwnsSamus)
        {
            if (deathStep is { } step)
                samus.DeathSequence.Draw(step.Draws, samus, _sprites, room.Camera.PixelPosition, _display.ActiveTurningFloor);
        }
        else
        {
            // Drawing Samus begins with the arm-cannon cover update before the default
            // drawing. HUD-selection stability, cover transition, and the
            // pose-authored before/after-body mode must therefore settle before charge
            // flare, atmospheric, or body sprites are appended.
            samus.ArmCannon.Update(samus);
            // The departing speed echoes close on Samus every update she is shown, whichever way she is drawn (#2573).
            samus.HorizontalSpeed.AdvanceDepartingSpeedEchoes(samus.Position.Whole);

            // The elevator's drawing replaces Samus's ordinary drawing while an
            // elevator owns a front-facing body. It still arrives after the cover update, so
            // the arm-cannon cover state above advances every frame. Odd frames draw nothing
            // more. Even frames draw her body alone, as her death does: no invincibility flicker and none of her
            // other pictures (atmosphere, charge flare, arm cannon, speed and shinespark echoes, grapple). The projectiles
            // are drawn below, after this drawing, as for every other way she is drawn.
            bool elevatorOwnsSamusDrawing =
                room.Enemies.Elevator.ElevatorStatus != ElevatorActorStatus.Inactive && SamusPoseMetadata.IsForwardFacingPose(samus.Pose);
            if (elevatorOwnsSamusDrawing)
            {
                if (RoomMainRoutines.ShouldDrawSamusOnElevator(_clock.Counter))
                {
                    // The elevator ride draws the body without the ordinary invincibility
                    // flicker, so this path cannot flicker twice.
                    samus.Presentation.DrawWithoutFlicker(samus,
                        _sprites,
                        room.Camera.PixelPosition,
                        _display.ActiveTurningFloor);
                }
            }
            else if (samus.Shinespark.Phase is
                     ShinesparkPhase.Crash or ShinesparkPhase.CrashHold)
            {
                // The shinespark crash's drawing takes over at the instant an active
                // shinespark crashes. Unlike the default drawing, it intentionally
                // omits charging flare/audio, atmosphere, both arm-cannon priority
                // paths, ordinary departing speed echoes, and grapple graphics. It
                // draws the live body first, then the crash's second echo before its
                // first on odd frames. The projectiles and trails below are still drawn after it.
                samus.Presentation.Draw(samus, 
                    _sprites,
                    room.Camera.PixelPosition,
                    _clock.Counter,
                    _display.ActiveTurningFloor);
                samus.Presentation.DrawShinesparkCrashEchoes(samus, 
                    _sprites,
                    room.Camera.PixelPosition,
                    _clock.Counter);
            }
            else
            {
                SamusGrappleDrawing grappleDrawing = SamusGrappleBeamDrawing.DrawingFor(samus.Grapple.Phase);

                if (grappleDrawing == SamusGrappleDrawing.Beam)
                {
                    // A firing beam's rope start and flare follow Samus's final position, after every move this update
                    // made, while its flare shows (#2573).
                    SamusGrappleMovement.SettleFiringOrigins(samus);
                    // The active grapple's drawing begins with the grapple flare. This flare is a different animation
                    // owner from the charge flare below.
                    SamusGrappleBeamDrawing.DrawFlareBeforeSamus(
                        samus,
                        _sprites,
                        room.Camera.PixelPosition,
                        ProjectileArtwork);
                }
                else if (grappleDrawing == SamusGrappleDrawing.Ordinary)
                {
                    // Samus's ordinary drawing advances/emits the ordinary charge flare before the rest of her
                    // drawing. The grapple's drawing never does this,
                    // including its cancel/release fallback frames.
                    // The flare animates as a simulation phase, then shows what it advanced to.
                    ChargeFlareComponents flare = room.Projectiles.AdvanceChargeFlare(samus);
                    // Drawing any flare component sets the value a Puyo's ceiling strike reads.
                    if (flare.ShowsAny)
                        room.Enemies.PuyoCeilingTurn.ChargeFlareDrawn();
                    room.Projectiles.DrawChargeFlare(
                        _sprites,
                        samus,
                        room.Camera.PixelPosition,
                        flare,
                        _display.ActiveTurningFloor,
                        new ChargeFlareArtwork
                        {
                            Placement = _artwork.ChargeFlarePlacement,
                            Sheets = ProjectileArtwork,
                        });
                }

                // The drawing all three routes share begins here: ordinary, active grapple,
                // and grapple teardown. Samus's effects animate here, so this is where the waits her breath bubbles
                // and lava spray keep count one update, and where her particles update and draw: earlier in the sprite
                // list than cannon/body, but later than either applicable flare.
                samus.LiquidPhysics.EffectWaits.Advance();
                samus.Presentation.Particles.UpdateAndDraw(
                    _sprites,
                    room.Camera.PixelPosition,
                    samus.LiquidPhysics.UntestedWaterSurfaceLine,
                    samus.Artwork.Sheets);

                // BeforeBody draws the arm cannon behind her body and AfterBody in front of it. Hidden draws no arm
                // cannon, even while the HUD's selection holds its cover open.
                if (samus.ArmCannon.DrawOrder == SamusArmCannonDrawOrder.BeforeBody)
                {
                    samus.ArmCannon.Draw(
                        _sprites,
                        samus,
                        room.Camera.PixelPosition,
                        _clock.Counter);
                }
                samus.Presentation.Draw(samus, 
                    _sprites,
                    room.Camera.PixelPosition,
                    _clock.Counter,
                    _display.ActiveTurningFloor);
                if (samus.ArmCannon.DrawOrder == SamusArmCannonDrawOrder.AfterBody)
                {
                    samus.ArmCannon.Draw(
                        _sprites,
                        samus,
                        room.Camera.PixelPosition,
                        _clock.Counter);
                }

                if (grappleDrawing == SamusGrappleDrawing.Beam)
                {
                    // The active grapple's drawing deliberately omits drawing Samus's speed echoes. It advances
                    // the endpoint's animation after Samus, moves the flare on, and only
                    // then draws the rope and endpoint when length is nonzero.
                    SamusGrappleBeamDrawing.DrawConnectedBeam(
                        samus.Grapple,
                        _sprites,
                        room.Camera.PixelPosition,
                        samus.Artwork.Sheets.Get(SpriteSheetId.Grapple));
                }
                else
                {
                    // The ordinary drawing and the grapple's teardown frame both end with Samus's echoes. The teardown
                    // frame shows no charge flare and no rope: it draws neither the ordinary flare nor the grapple's beam.
                    samus.Presentation.DrawSpeedBoosterEchoes(samus, 
                        _sprites,
                        room.Camera.PixelPosition);
                    samus.Presentation.DrawReleasedShinesparkCrashEchoes(samus, 
                        _sprites,
                        room.Camera.PixelPosition,
                        _clock.Counter);
                }
            }

            // Samus's projectiles draw straight after her, after the grapple beam too, so every live beam picture
            // lies in front of her body.
            room.Projectiles.DrawLiveProjectiles(
                _sprites,
                room.Camera.PixelPosition,
                ProjectileArtwork);
            // The ordinary projectile draw is immediately followed by the projectile
            // trails. Trails are detached world-space objects, so they must keep
            // animating after their source beam has collided or left the viewport.
            // Trails animate as a simulation phase, under time freeze too, then show where they are.
            room.Projectiles.AdvanceTrails(_freeze.TimeIsFrozen);
            room.Projectiles.DrawTrails(_sprites, room.Camera.PixelPosition, ProjectileArtwork);

            // The post-draw input snapshot survives the next input phase. In
            // particular, Fire can first cancel a spin and then start Grapple without
            // requiring another physical press after the prospective pose is applied.
            samus.InputSnapshot.SnapshotDrawInput(_playerActions.Current, _playerActions.NewlyPressed);
            SamusInputSnapshot.UpdateHurtFlashFromHealthLoss(samus);
            // The demo's input is never replaced with auto-jump.
            if (_attractSession.IsAttractDemo) samus.InputSnapshot.AutoJump.Reinstate();

            // The charge-sound latch follows the drawing of Samus and her projectiles. A hurt flash's sound-recovery update
            // may have armed this latch above; consuming it here preserves both the
            // same-frame charging sound and ordering after projectile drawing.
            SamusPostDrawAudio.Step(
                samus,
                _player.PreviousMovementTypeForXray,
                _playerActions.Current);
        }

        if (!deathOwnsSamus)
        {
            // The enemies draw after Samus and her projectiles, each at its frame's depth.
            room.Enemies.Drawing.Advance(room.Camera.PixelPosition);
            room.Enemies.Drawing.Draw(_sprites, room.Camera.PixelPosition, samus);
        }

        if (!deathOwnsSamus && !samus.Xray.AreEnemyProjectilesSuspended)
        {
            room.Enemies.Projectiles.DrawEnemyProjectilesBehindSamus(
                _sprites,
                room.Camera.PixelPosition, _freeze.TimeIsFrozen);
        }

        if (!deathOwnsSamus)
        {
            // Once Ceres Ridley's animation starts, his Baby's animation runs after the actors' drawing steps.
            room.Enemies.Ceres.StepCeresBabyAfterActors();
            // The Baby and the door of Ridley's room, while it shows its overlay, each show once, at their own place.
            room.Enemies.Ceres.PublishCeresBaby(_sprites, room.Camera.PixelPosition);
            room.Enemies.Ceres.PublishRidleyRoomDoorOverlay(_sprites, room.Camera.PixelPosition);
        }
    }
}
