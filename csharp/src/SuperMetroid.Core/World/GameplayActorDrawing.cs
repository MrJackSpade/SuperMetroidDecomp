using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Bosses.Draygon;
using SuperMetroid.Core.Enemies.Elevator;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Samus.Grapple;
using SuperMetroid.Core.Samus.Palettes;
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
    /// <summary>Supplies installed frames, projectile compositions, and suit palette artwork.</summary>
    private readonly RuntimeArtwork _artwork;
    /// <summary>Provides the attract-demo state that can alter ordinary Samus presentation.</summary>
    private readonly IAttractDemoState _attractSession;
    /// <summary>Supplies the current input state used by presentation handlers.</summary>
    private readonly ControllerInputState _controller1;
    /// <summary>Controls gameplay display modes that can suppress ordinary Samus drawing.</summary>
    private readonly GameplayDisplayState _display;
    /// <summary>Determines whether time-dependent enemy projectile drawing is suspended.</summary>
    private readonly GameplayTimeFreeze _freeze;
    /// <summary>Provides frame-counter cadence for blinking and drawing branches.</summary>
    private readonly FrameClock _clock;

    /// <summary>Receives Samus's whole-frame draws in the same order as the sprites around them.</summary>
    private readonly SpriteFrameBuffer _sprites;
    /// <summary>Supplies Samus pose, palettes, grapple state, and movement effects.</summary>
    private readonly SamusFrameState _player;
    /// <summary>Provides the camera and Ceres arrival actors for this draw pass.</summary>
    private readonly ActiveRoomState _room;

    /// <summary>Binds the shared runtime owners whose state determines each actor drawing pass.</summary>
    internal GameplayActorDrawing(GameplayActorDrawingDependencies dependencies)
    {
        _artwork = dependencies.Artwork;
        _attractSession = dependencies.AttractSession;
        _controller1 = dependencies.Controller1;
        _display = dependencies.Display;
        _freeze = dependencies.Freeze;
        _clock = dependencies.Clock;
        _sprites = dependencies.Sprites;
        _player = dependencies.Player;
        _room = dependencies.Room;
    }

    /// <summary>What Samus's projectile, bomb, flare and trail draws show: each the sheet of its own beams (#1586).</summary>
    private ProjectileDrawArtwork ProjectileArtwork => new(_artwork.ProjectileSheets);
    /// <summary>Shared enemy-layer/Samus/projectile drawing, independently of movement.</summary>
    internal void DrawGameplayActors(RoomScope room, SamusDrawOwner owner, SamusPaletteEffects paletteEffects = SamusPaletteEffects.Advance)
    {
        bool deathOwnsSamus = owner == SamusDrawOwner.DeathSequence;
        if (_player.Samus is null || _room.Camera is null) return;
        _artwork.BindGrapplePresentation();
        // The frame draws bomb/projectile explosions before reaching the enemy-layer
        // phase that draws Samus and her projectiles. Preserve that sprite ordering.
        if (!deathOwnsSamus)
        {
            room.BombProjectiles.Draw(_sprites, _room.Camera.PixelPosition, ProjectileArtwork);
            room.Projectiles.DrawExplosions(_sprites, _room.Camera.PixelPosition, ProjectileArtwork);
        }

        // Enemy projectiles in front of Samus follow bomb/projectile explosions and precede Samus's
        // draw. Room-specific actors may draw in front of Samus
        // here without teaching the reusable Landing Site runtime how to own enemies.
        if (!deathOwnsSamus && !_player.Samus.Xray.AreEnemyProjectilesSuspended)
        {
            // The Ceres elevator is drawn in front of Samus, as both its definitions are. Projectiles in front of
            // Samus are drawn here, before Samus.
            _room.CeresElevatorArrival?.Draw(_sprites, room.Enemies.Presentation, _room.Camera.PixelPosition);
            if (room.Enemies.IsLoaded)
            {
                // A hit burst among the sprite objects is one of Samus's weapon effects: it shows the sheet of the beams she
                // has equipped as it is drawn, as her charge flare does (#1713).
                room.Enemies.Projectiles.DrawEnemyProjectilesInFrontOfSamus(
                    _sprites,
                    _room.Camera.PixelPosition,
                    new SamusHitBurstArtwork(ProjectileArtwork, SamusBeamCombinations.Of(_player.Samus.EquippedBeams)),
                    _freeze.TimeIsFrozen);
            }
        }

        if (paletteEffects == SamusPaletteEffects.Advance)
        {
            // Samus's palette update runs during gameplay. The renderer
            // reads the colours directly, so perform the suit palette copy immediately
            // before the matching draw phase. This covers dry-room Speed Booster stage four
            // and the normal-suit restoration requested by cancellation.
            // A negative super-special flag is the first branch of the palette update and returns
            // immediately after the drained/rainbow handler. Its one-shot normal restore
            // also models the direct palette loads in letting the drained Samus fall and stopping her rainbow.
            // Cancellation/pose initialization copies the normal suit during
            // movement, before this dispatch. Flush that deferred write first so charge
            // and drained palettes retain their same-frame priority.
            if (!deathOwnsSamus)
                _player.Samus.HorizontalSpeed.ApplyPendingNormalSuitPaletteRestore(
                    _player.Colours, _player.Samus.EquippedItems);
            bool drainedOwnsSamusPalette = !deathOwnsSamus &&
                _player.Samus.Drained.UpdatePalette(_player.Colours, _player.Samus.EquippedItems);
            bool chargeGlowRestoredNormalPalette = false;
            if (!deathOwnsSamus && !drainedOwnsSamusPalette)
            {
                // The palette update always runs charge/post-shot handling before dispatching the
                // selected special Samus palette. Ordinary charged shots paint all of Samus's visible colours
                // white for three calls and restore the suit on call four; Hyper shots
                // step ten complete palettes on alternating calls before restore.
                LastBeamChargePaletteStep = room.Projectiles.UpdateBeamChargePalette(
                    _player.Colours,
                    _player.Samus,
                    room.FxOverlay.VisorAnimation);
                // A restored suit from the beam-charge palette step bypasses the entire special
                // colour step on glow expiry. In particular, stored shine must not tick
                // or overwrite the restored suit on this frame.
                chargeGlowRestoredNormalPalette =
                    LastBeamChargePaletteStep.Action == SamusBeamChargePaletteAction.RestoredNormalSuit;

                if (!chargeGlowRestoredNormalPalette)
                    _player.Samus.HorizontalSpeed.UpdateSpeedBoosterPalette(_player.Colours, new SpeedBoosterPaletteFrame { MovementType = SamusPoseMetadata.ReadMovementType(_player.Samus), AnimationFrame = _player.Samus.AnimationFrame, EquippedItems = _player.Samus.EquippedItems, SuppressActiveSpeedBoosterPalette = _player.Samus.Shinespark.PaletteType != SamusSpecialPaletteType.None ||
                        _player.Samus.CrystalFlash.SpecialPaletteKind ==
                            SamusSpecialPaletteType.CrystalFlash ||
                        _player.Samus.Xray.SpecialPaletteKind == SamusSpecialPaletteType.Xray, BottomBoundarySubmerged = _player.Samus.LiquidPhysics.IsBottomBoundarySubmerged(_player.Samus) });
            }
            // The shinespark colouring runs at the same palette-update dispatch point. It
            // executes only when charge handling did not restore the suit.
            if (!deathOwnsSamus && !drainedOwnsSamusPalette && !chargeGlowRestoredNormalPalette)
            {
                _player.Samus.Shinespark.UpdatePalette(
                    _player.Colours,
                    _player.Samus.EquippedItems);
            }
            // The Crystal Flash colouring owns all sixteen of Samus's projectile colours during Crystal
            // Flash. It is mutually exclusive with the shinespark and X-ray colouring but
            // intentionally runs at the same palette-update dispatch point.
            if (!deathOwnsSamus && !drainedOwnsSamusPalette && !chargeGlowRestoredNormalPalette)
            {
                _player.Samus.CrystalFlash.UpdatePalette(
                    _player.Colours,
                    _player.Samus);
            }
            // The X-ray colouring changes only one of the visor's colours while active; its teardown
            // restores the complete Power/Varia/Gravity suit palette once.
            if (!deathOwnsSamus && !drainedOwnsSamusPalette && !chargeGlowRestoredNormalPalette)
            {
                _player.Samus.Xray.UpdatePalette(
                    _player.Colours,
                    _player.Samus.EquippedItems);
            }
            // The Metroid-drain palette step runs after every charge and special-palette family. A Metroid's
            // nonnegative super-special flag takes the alternating boost/normal branch and
            // returns before ordinary hurt flash, just as drained rainbow's negative flag
            // took the earlier branch.
            bool metroidOwnsSamusPalette = !deathOwnsSamus &&
                !drainedOwnsSamusPalette &&
                SamusSpecialSuperPalette.Update(_player.Colours, _player.Samus);
            if (!deathOwnsSamus && !drainedOwnsSamusPalette && !metroidOwnsSamusPalette)
            {
                SamusHurtFlashPalette.Update(
                    _player.Colours,
                    _player.Samus,
                    _controller1.Current);
            }

        }

        if (deathOwnsSamus)
        {
            if (_player.LastDeathSequenceStep is { DrawPose: true })
            {
                _player.Samus.Presentation.Draw(_player.Samus, 
                    _sprites,
                    _room.Camera.PixelPosition,
                    turningFloor: _display.ActiveTurningFloor);
            }
            else if (_player.LastDeathSequenceStep is { DrawExplosion: true })
                _player.Samus.DeathSequence.DrawExplosion(_sprites, _player.Samus.Presentation.Sheets);
        }
        else
        {
            // Drawing Samus begins with the arm-cannon cover update before dispatching the default
            // drawing handler. HUD-selection stability, cover transition, and the
            // pose-authored before/after-body mode must therefore settle before charge
            // flare, atmospheric, or body sprites are appended.
            _player.Samus.ArmCannon.Update(_player.Samus);

            // The elevator display handler replaces the ordinary display handler while an
            // elevator owns a front-facing body. It still arrives after the cover update, so
            // the arm-cannon cover state above advances every frame. Odd frames return
            // immediately. Even frames call the fatal/no-animation body renderer, which
            // deliberately bypasses ordinary invincibility flicker and every auxiliary
            // Samus layer: atmosphere, charge flare, cannon sprite, speed/shinespark
            // echoes, and grapple graphics. Projectile drawing remains in the shared
            // tail below, exactly as the game draws them after this handler.
            bool elevatorOwnsSamusDrawing =
                room.Enemies.Elevator.ElevatorStatus != ElevatorActorStatus.Inactive && SamusPoseMetadata.IsForwardFacingPose(_player.Samus.Pose);
            if (elevatorOwnsSamusDrawing)
            {
                if (RoomMainRoutines.ShouldDrawSamusOnElevator(_clock.Counter))
                {
                    // Omitting the live counter intentionally selects `Draw`'s default
                    // even value. The elevator handler calls the body renderer below the ordinary
                    // invincibility test, so this path cannot flicker twice.
                    _player.Samus.Presentation.Draw(_player.Samus, 
                        _sprites,
                        _room.Camera.PixelPosition,
                        turningFloor: _display.ActiveTurningFloor);
                }
            }
            else if (_player.Samus.Shinespark.Phase is
                     ShinesparkPhase.Crash or ShinesparkPhase.CrashEchoCircle)
            {
                // The shinespark-crash display handler is installed at the instant an active
                // shinespark crashes. Unlike the default handler, it intentionally
                // omits charging flare/audio, atmosphere, both arm-cannon priority
                // paths, ordinary departing speed echoes, and grapple graphics. It
                // draws the live body first, then crash slot one before slot zero on
                // odd frames. The shared projectile/trail tail below still follows.
                _player.Samus.Presentation.Draw(_player.Samus, 
                    _sprites,
                    _room.Camera.PixelPosition,
                    _clock.Counter,
                    _display.ActiveTurningFloor);
                _player.Samus.Presentation.DrawShinesparkCrashEchoes(_player.Samus, 
                    _sprites,
                    _room.Camera.PixelPosition,
                    _clock.Counter);
            }
            else
            {
                SamusGrappleDrawing grappleDrawing = SamusGrappleBeamDrawing.DrawingFor(_player.Samus.Grapple.Phase);

                if (grappleDrawing == SamusGrappleDrawing.Beam)
                {
                    // The active grapple display handler begins with the grapple flare. Firing also refreshes
                    // its hand origins here from Samus's post-movement position. This
                    // flare is a different animation owner from the charge flare below.
                    SamusGrappleBeamDrawing.DrawFlareBeforeSamus(
                        _player.Samus,
                        _sprites,
                        _room.Camera.PixelPosition,
                        ProjectileArtwork);
                }
                else if (grappleDrawing == SamusGrappleDrawing.Ordinary)
                {
                    // The ordinary display handler advances/emits the ordinary charge flare before it
                    // falls through to the shared drawing tail. Grapple's handler never calls this,
                    // including its cancel/release fallback frames.
                    // The flare animates as a simulation phase, then shows what it advanced to.
                    ChargeFlareComponents flare = room.Projectiles.AdvanceChargeFlare(_player.Samus);
                    // Drawing any flare component sets the value a Puyo's ceiling strike reads.
                    if (flare.Count > 0)
                        room.Enemies.PuyoCeilingTurn.ChargeFlareDrawn();
                    room.Projectiles.DrawChargeFlare(
                        _sprites,
                        _player.Samus,
                        _room.Camera.PixelPosition,
                        flare,
                        _display.ActiveTurningFloor,
                        new ChargeFlareArtwork
                        {
                            Placement = _artwork.ChargeFlarePlacement,
                            Objects = ProjectileArtwork,
                        });
                }

                // The shared drawing tail begins here for all three routes: ordinary, active grapple,
                // and grapple teardown. Samus's effects animate here, so this is where the waits her breath bubbles
                // and lava spray keep count one update, and where her particles update and draw: earlier in the sprite
                // list than cannon/body, but later than either applicable flare.
                _player.Samus.LiquidPhysics.EffectWaits.Advance();
                _player.Samus.Presentation.Particles.UpdateAndDraw(
                    _sprites,
                    _room.Camera.PixelPosition,
                    _player.Samus.LiquidPhysics.UntestedWaterSurfaceLine,
                    _player.Samus.Presentation.Sheets);

                // Drawing modes one and two differ only in sprite priority: one appends the
                // cannon before the body, while two appends it after. Mode zero suppresses
                // the independent object even if a HUD-driven cover frame remains nonzero.
                if (_player.Samus.ArmCannon.DrawOrder == SamusArmCannonDrawOrder.BeforeBody)
                {
                    _player.Samus.ArmCannon.Draw(
                        _sprites,
                        _player.Samus,
                        _room.Camera.PixelPosition,
                        _clock.Counter);
                }
                _player.Samus.Presentation.Draw(_player.Samus, 
                    _sprites,
                    _room.Camera.PixelPosition,
                    _clock.Counter,
                    _display.ActiveTurningFloor);
                if (_player.Samus.ArmCannon.DrawOrder == SamusArmCannonDrawOrder.AfterBody)
                {
                    _player.Samus.ArmCannon.Draw(
                        _sprites,
                        _player.Samus,
                        _room.Camera.PixelPosition,
                        _clock.Counter);
                }

                if (grappleDrawing == SamusGrappleDrawing.Beam)
                {
                    // The active grapple handler deliberately omits drawing Samus's speed echoes. It advances
                    // the endpoint's animation after Samus, moves the flare on, and only
                    // then draws the rope and endpoint when length is nonzero.
                    SamusGrappleBeamDrawing.DrawConnectedBeam(
                        _player.Samus.Grapple,
                        _sprites,
                        _room.Camera.PixelPosition,
                        _artwork.SpriteSheets?.Get(SpriteSheetId.Grapple));
                }
                else
                {
                    // The ordinary drawing tail and grapple's signed-range fallback share this
                    // exact echo tail. The latter still suppresses charge flare and rope
                    // because the ordinary flare and the active half of the grapple handler were bypassed.
                    _player.Samus.Presentation.DrawSpeedBoosterEchoes(_player.Samus, 
                        _sprites,
                        _room.Camera.PixelPosition);
                    _player.Samus.Presentation.DrawReleasedShinesparkCrashEchoes(_player.Samus, 
                        _sprites,
                        _room.Camera.PixelPosition,
                        _clock.Counter);
                }
            }

            // The frame draws Samus first and immediately draws her projectiles. This is
            // deliberately after the grapple beam too in the composite pass;
            // all live beam art still receives later sprite indices than Samus's body.
            room.Projectiles.DrawLiveProjectiles(
                _sprites,
                _room.Camera.PixelPosition,
                ProjectileArtwork);
            // The ordinary projectile draw is immediately followed by the projectile
            // trails. Trails are detached world-space objects, so they must keep
            // animating after their source beam has collided or left the viewport.
            // Trails animate as a simulation phase, under time freeze too, then show where they are.
            room.Projectiles.AdvanceTrails(_freeze.TimeIsFrozen);
            room.Projectiles.DrawTrails(_sprites, _room.Camera.PixelPosition, ProjectileArtwork);

            // The post-draw input snapshot survives the next input phase. In
            // particular, Fire can first cancel a spin and then start Grapple without
            // requiring another physical press after the prospective pose is applied.
            _player.Samus.InputSnapshot.SnapshotDrawInput(_controller1.Current, _controller1.NewlyPressed);
            SamusInputSnapshot.UpdateHurtFlashFromHealthLoss(_player.Samus);
            // The demo input handler is never replaced with auto-jump.
            if (_attractSession.IsAttractDemo) _player.Samus.InputSnapshot.AutoJumpInputPending = false;

            // The charge-sound latch follows the drawing of Samus and her projectiles. A counter-forty hurt update
            // may have armed this latch above; consuming it here preserves both the
            // same-frame charging sound and ordering after projectile drawing.
            SamusPostDrawAudio.Step(
                _player.Samus,
                _player.PreviousMovementTypeForXray,
                _controller1.Current);
        }

        if (!deathOwnsSamus && room.Enemies.IsLoaded)
        {
            // The enemies draw after Samus and her projectiles, each at its frame's depth.
            room.Enemies.Drawing.Advance(_room.Camera.PixelPosition);
            room.Enemies.Drawing.Draw(_sprites, _room.Camera.PixelPosition);
        }

        if (!deathOwnsSamus && room.Enemies.IsLoaded &&
            !_player.Samus.Xray.AreEnemyProjectilesSuspended)
        {
            room.Enemies.Projectiles.DrawEnemyProjectilesBehindSamus(
                _sprites,
                _room.Camera.PixelPosition, _freeze.TimeIsFrozen);
        }

        if (!deathOwnsSamus && room.Enemies.IsLoaded && room.Enemies.DisplaySources.Draygon is { } draygon)
        {
            // Draygon's body draw runs AFTER drawing enemies. Camera scrolling has already
            // run, so the body must use the same final camera as the appendage sprites above.
            // Placing it during enemy AI instead detaches the torso whenever scrolling
            // changes the camera between AI and drawing (especially during a grab). The
            // placement is Draygon's own published body position (#1354), not the background's scroll.
            draygon.BodyOrigin = new Position16<RoomPixels>(
                CoordinateAxis.Wrap(draygon.Body.Position.X.Whole + DraygonBackgroundData.HorizontalOrigin - draygon.BodyGraphicsDisplacement.X),
                CoordinateAxis.Wrap(draygon.Body.Position.Y.Whole + DraygonBackgroundData.VerticalOrigin - draygon.BodyGraphicsDisplacement.Y));
        }

        if (!deathOwnsSamus && room.Enemies.IsLoaded)
        {
            // Once his animation starts, Ceres Ridley's scene draws the Baby Metroid sprite (plus the arena-door
            // overlay) after every ordinary enemy layer; it is their only producer. Keeping this after layer seven
            // preserves the Baby's actual sprite position instead of inventing a Baby slot.
            room.Enemies.Ceres.DrawCeresRidleyBabyAndDoorOverEnemies(
                _sprites,
                _room.Camera.PixelPosition);
        }
    }

    /// <summary>
    /// Most recent beam-charge/post-shot body-palette branch. This remains
    /// distinct from the global Hyper Beam projectile palette object above: it owns Samus's
    /// body colours rather than her projectiles' colours.
    /// </summary>
    public SamusBeamChargePaletteStepResult LastBeamChargePaletteStep { get; internal set; }






}
