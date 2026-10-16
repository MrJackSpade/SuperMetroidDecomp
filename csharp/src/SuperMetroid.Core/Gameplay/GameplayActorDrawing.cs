using SuperMetroid.Core.Enemies.Drawing;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Bosses.Draygon;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Samus.Grapple;
using SuperMetroid.Core.Samus.Palettes;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Samus.Drawing;

namespace SuperMetroid.Core.Gameplay;

/// <summary>
/// Draws the gameplay actors each frame (Samus, arm cannon, projectiles, grapple, enemies) and publishes what
/// that drawing did.
/// </summary>
internal sealed class GameplayActorDrawing
{
    /// <summary>Supplies installed spritemaps, projectile compositions, and suit palette artwork.</summary>
    private readonly RuntimeArtwork _artwork;
    /// <summary>Provides the attract-demo state that can alter ordinary Samus presentation.</summary>
    private readonly IAttractDemoState _attractSession;
    /// <summary>Supplies active background offsets for the drawing phase.</summary>
    private readonly BackgroundScrollState _backgroundScroll;
    /// <summary>Draws bomb effects before the enemy layer pass, following native OAM order.</summary>
    private readonly SamusBombProjectileSystem _bombProjectiles;
    /// <summary>Receives the palette writes that the software PPU will read for this frame.</summary>
    private readonly SnesCgram _cgram;
    /// <summary>Supplies the current input state used by presentation handlers.</summary>
    private readonly ControllerInputState _controller1;
    /// <summary>Controls gameplay display modes that can suppress ordinary Samus drawing.</summary>
    private readonly GameplayDisplayState _display;
    /// <summary>Supplies enemy layers, projectiles, and room-specific post-draw hooks.</summary>
    private readonly RoomEnemySystem _enemies;
    /// <summary>Determines whether time-dependent enemy projectile drawing is suspended.</summary>
    private readonly GameplayTimeFreeze _freeze;
    /// <summary>Provides accepted-NMI cadence for blinking and drawing branches.</summary>
    private readonly NmiClock _nmi;
    /// <summary>Receives actor sprites in cartridge drawing order.</summary>
    private readonly OamBuffer _oam;

    /// <summary>The gameplay OBJ character page enemy-format sprites draw from, as the table's bindings show it.</summary>
    private SpriteArtwork ObjectPage => _artwork.GameplayObjectPage(_enemies.ObjCharacters);
    /// <summary>Receives Samus's whole-frame draws in the same order as the OAM sprites around them.</summary>
    private readonly SpriteFrameBuffer _sprites;
    /// <summary>Supplies Samus pose, palettes, grapple state, and movement effects.</summary>
    private readonly SamusFrameState _player;
    /// <summary>Supplies beam palette updates and player-projectile effects.</summary>
    private readonly SamusProjectileSystem _projectiles;
    /// <summary>Provides the camera and Ceres arrival actors for this draw pass.</summary>
    private readonly ActiveRoomState _room;
    /// <summary>Receives actor graphics transfers requested during presentation.</summary>
    private readonly VramWriteQueue _vramWrites;
    /// <summary>Reads cartridge drawing tables and palette data used by native routines.</summary>
    private readonly ISnesAddressSpace _addressSpace;

    /// <summary>Binds the shared runtime owners whose state determines each actor drawing pass.</summary>
    internal GameplayActorDrawing(GameplayActorDrawingDependencies dependencies)
    {
        _artwork = dependencies.Artwork;
        _attractSession = dependencies.AttractSession;
        _backgroundScroll = dependencies.BackgroundScroll;
        _bombProjectiles = dependencies.BombProjectiles;
        _cgram = dependencies.Cgram;
        _controller1 = dependencies.Controller1;
        _display = dependencies.Display;
        _enemies = dependencies.Enemies;
        _freeze = dependencies.Freeze;
        _nmi = dependencies.Nmi;
        _oam = dependencies.Sprites.Oam;
        _sprites = dependencies.Sprites;
        _player = dependencies.Player;
        _projectiles = dependencies.Projectiles;
        _room = dependencies.Room;
        _vramWrites = dependencies.VramWrites;
        _addressSpace = dependencies.AddressSpace;
    }

    /// <summary>What Samus's projectile, bomb, flare and trail draws bind: compositions and the page her equipped beams select.</summary>
    private ProjectileDrawArtwork ProjectileArtwork => new(_artwork.ProjectileCompositions, _artwork.ProjectileObjects,
        _player.Samus?.EquippedBeams ?? 0);
    /// <summary>Shared enemy-layer/Samus/projectile drawing, independently of movement.</summary>
    internal void DrawGameplayActors(bool deathOwnsSamus, bool advanceSamusPalette = true)
    {
        if (_player.Samus is null || _room.Camera is null) return;
        _artwork.BindGrapplePresentation();
        // $A0:884D draws bomb/projectile explosions before reaching the enemy-layer
        // phase that calls DrawSamusAndProjectiles. Preserve that OAM ordering.
        if (!deathOwnsSamus)
        {
            _bombProjectiles.Draw(_addressSpace, _sprites, _room.Camera.PixelPosition, ProjectileArtwork);
            _projectiles.DrawExplosions(_addressSpace, _sprites, _room.Camera.PixelPosition, ProjectileArtwork);
        }

        // `$A0:885D` calls `$86:8390` after bomb/projectile explosions and before the
        // layer loop reaches Samus at layer three. Room-specific actors may supply this
        // pass without teaching the reusable Landing Site runtime how to own enemies.
        if (!deathOwnsSamus && !_player.Samus.Xray.AreEnemyProjectilesSuspended)
        {
            // Both Ceres elevator definitions carry properties $3000, including bit
            // $1000 selected by Draw_HighPriority_EnemyProjectile at `$86:8390`.
            // That named pass occurs here, before enemy layers zero through two and
            // Samus; lower OAM indices retain their native same-priority overlap win.
            _room.CeresElevatorArrival?.Draw(_sprites, ObjectPage, _room.Camera.PixelPosition);
            if (_enemies.IsLoaded)
            {
                _enemies.Projectiles.DrawHighPriorityEnemyProjectiles(
                    _sprites, ObjectPage,
                    _room.Camera.PixelPosition, _freeze.TimeIsFrozen);
            }
        }

        // The global layer loop emits layers zero through two before its phase-three
        // call to DrawSamusAndProjectiles. Landing Site's gunship definition selects
        // layer two, so its hull correctly precedes (and can sit behind) Samus OAM.
        if (!deathOwnsSamus && _enemies.IsLoaded)
        {
            _enemies.Drawing.AdvanceLayers(_room.Camera.PixelPosition, 0, 2);
            _enemies.Drawing.DrawLayers(_sprites, ObjectPage, _room.Camera.PixelPosition, 0, 2);
        }

        if (advanceSamusPalette)
        {
            // `$91:D6F7` updates Samus's palette buffer during gameplay. The software PPU
            // reads CGRAM directly, so perform the literal ROM pointer/table copy immediately
            // before the matching draw phase. This covers dry-room Speed Booster stage four
            // and the normal-suit restoration requested by `$91:DE53` cancellation.
            // A negative super-special flag is the first branch of `$91:D6F7` and returns
            // immediately after the drained/rainbow handler. Its one-shot normal restore
            // also models the direct palette loads in controller zero and command `$17`.
            // Native cancellation/pose initialization copies the normal suit during
            // movement, before this dispatch. Flush that deferred write first so charge
            // and drained palettes retain their native same-frame priority.
            if (!deathOwnsSamus)
                _player.Samus.HorizontalSpeed.ApplyPendingNormalSuitPaletteRestore(
                    _addressSpace, _cgram, _player.Samus.EquippedItems, _artwork.MapPresentation?.SamusSuitColors);
            bool drainedOwnsSamusPalette = !deathOwnsSamus &&
                _player.Samus.Drained.UpdatePalette(_addressSpace, _cgram, _player.Samus.EquippedItems,
                    _artwork.MapPresentation?.SamusSuitColors);
            LastHurtFlashPaletteStep = default;
            LastVisorPaletteStep = default;
            bool chargeGlowRestoredNormalPalette = false;
            if (!deathOwnsSamus && !drainedOwnsSamusPalette)
            {
                // `$91:D708` always runs charge/post-shot handling before dispatching the
                // selected special Samus palette. Ordinary charged shots paint colors 1-15
                // white for three calls and restore the ROM suit on call four; Hyper shots
                // step ten complete bank-$9B palettes on alternating calls before restore.
                LastBeamChargePaletteStep = _projectiles.UpdateBeamChargePalette(
                    _addressSpace,
                    _cgram,
                    _player.Samus,
                    _display.LayerBlendingDefaultConfig);
                LastVisorPaletteStep = _projectiles.LastVisorPaletteStep;
                // Native carry from HandleBeamChargePalettes bypasses the entire special
                // dispatcher on glow expiry. In particular, stored shine must not tick
                // or overwrite the restored suit on this frame.
                chargeGlowRestoredNormalPalette =
                    LastBeamChargePaletteStep.Action == SamusBeamChargePaletteAction.RestoredNormalSuit;

                if (!chargeGlowRestoredNormalPalette)
                    _player.Samus.HorizontalSpeed.UpdateSpeedBoosterPalette(_addressSpace, _cgram, new SpeedBoosterPaletteFrame { MovementType = _player.Samus.PoseMetadata.ReadMovementType(_addressSpace), AnimationFrame = _player.Samus.AnimationFrame, EquippedItems = _player.Samus.EquippedItems, SuppressActiveSpeedBoosterPalette = _player.Samus.Shinespark.PaletteType != SamusSpecialPaletteType.None ||
                        _player.Samus.CrystalFlash.SpecialPaletteKind ==
                            SamusSpecialPaletteType.CrystalFlash ||
                        _player.Samus.Xray.SpecialPaletteKind == SamusSpecialPaletteType.Xray, BottomBoundarySubmerged = _player.Samus.LiquidPhysics.IsBottomBoundarySubmerged(_player.Samus) }, suitColors: _artwork.MapPresentation?.SamusSuitColors, cycleColors: _artwork.MapPresentation?.SamusFullBodyCycleColors);
            }
            // Palette handlers one and six run at the same `$91:D6F7` dispatch point. They
            // execute only when charge handling returned carry clear.
            if (!deathOwnsSamus && !drainedOwnsSamusPalette && !chargeGlowRestoredNormalPalette)
            {
                _player.Samus.Shinespark.UpdatePalette(
                    _addressSpace,
                    _cgram,
                    _player.Samus.EquippedItems,
                    _artwork.MapPresentation?.SamusSuitColors,
                    _artwork.MapPresentation?.SamusFullBodyCycleColors);
            }
            // Handler seven owns all sixteen colors of sprite palette six during Crystal
            // Flash. It is mutually exclusive with shinespark/X-ray special handlers but
            // intentionally runs at the same `$91:D6F7` dispatch point.
            if (!deathOwnsSamus && !drainedOwnsSamusPalette && !chargeGlowRestoredNormalPalette)
            {
                _player.Samus.CrystalFlash.UpdatePalette(
                    _addressSpace,
                    _cgram,
                    _player.Samus,
                    _artwork.BeamArtwork?.Palettes,
                    _artwork.MapPresentation?.CrystalFlashColors);
            }
            // Handler eight changes only visor color four while active; `$FFFF` teardown
            // restores the complete ROM-selected Power/Varia/Gravity suit palette once.
            if (!deathOwnsSamus && !drainedOwnsSamusPalette && !chargeGlowRestoredNormalPalette)
            {
                _player.Samus.Xray.UpdatePalette(
                    _addressSpace,
                    _cgram,
                    _player.Samus.EquippedItems,
                    _artwork.MapPresentation?.SamusSuitColors);
            }
            // `$91:D8A5` runs after every charge and special-palette family. A Metroid's
            // nonnegative super-special flag takes the alternating boost/normal branch and
            // returns before ordinary hurt flash, just as drained rainbow's negative flag
            // took the routine's earlier branch.
            bool metroidOwnsSamusPalette = !deathOwnsSamus &&
                !drainedOwnsSamusPalette &&
                SamusSpecialSuperPalette.Update(_cgram, _player.Samus);
            if (!deathOwnsSamus && !drainedOwnsSamusPalette && !metroidOwnsSamusPalette)
            {
                LastHurtFlashPaletteStep = SamusHurtFlashPalette.Update(
                    _addressSpace,
                    _cgram,
                    _player.Samus,
                    _controller1.Current,
                    _artwork.MapPresentation?.SamusHurtColors);
            }

        }

        if (deathOwnsSamus)
        {
            if (_player.LastDeathSequenceStep is { DrawPose: true })
            {
                LastSamusBodyDrawn = _player.Samus.Presentation.Draw(
                    _addressSpace,
                    _sprites,
                    _room.Camera.PixelPosition,
                    mode7Transform: _display.ActiveSamusMode7Transform);
            }
            else if (_player.LastDeathSequenceStep is { DrawExplosion: true })
                _player.Samus.DeathSequence.DrawExplosion(_addressSpace, _sprites, _artwork.SamusBodyArt?.Spritemaps, _artwork.SamusBodyArt?.DeathTiles);
        }
        else
        {
            // `DrawSamusSprites` begins with `$90:C5C4` before dispatching the default
            // drawing handler. HUD-selection stability, cover transition, and the
            // pose-authored before/after-body mode must therefore settle before charge
            // flare, atmospheric, or body OAM is appended.
            LastArmCannonUpdate = _player.Samus.ArmCannon.Update(_addressSpace, _player.Samus);
            LastArmCannonDraw = default;

            // `$90:EB86` replaces the ordinary `$90:EB52` display handler while an
            // elevator owns a front-facing body. It still arrives after `$90:C5C4`, so
            // the arm-cannon cover state above advances every frame. Odd NMIs return
            // immediately. Even NMIs call the fatal/no-animation body renderer, which
            // deliberately bypasses ordinary invincibility flicker and every auxiliary
            // Samus layer: atmosphere, charge flare, cannon OBJ, speed/shinespark
            // echoes, and grapple graphics. Projectile drawing remains in the shared
            // tail below, exactly as `DrawSamusAndProjectiles` does after this handler.
            bool elevatorOwnsSamusDrawing =
                ((ushort)_enemies.ElevatorBehavior.ElevatorStatus) != 0 && SamusPoseMetadata.IsForwardFacingPose(_player.Samus.Pose);
            if (elevatorOwnsSamusDrawing)
            {
                if (RoomMainRoutines.ShouldDrawSamusOnElevator(_nmi.NmiFrameCounter))
                {
                    // Omitting the live counter intentionally selects `Draw`'s default
                    // even value. Native `$90:EB86` calls `$90:85D2`, below `$85E2`'s
                    // ordinary invincibility test, so this path cannot flicker twice.
                    LastSamusBodyDrawn = _player.Samus.Presentation.Draw(
                        _addressSpace,
                        _sprites,
                        _room.Camera.PixelPosition,
                        mode7Transform: _display.ActiveSamusMode7Transform);
                }
            }
            else if (_player.Samus.Shinespark.Phase is
                     ShinesparkPhase.Crash or ShinesparkPhase.CrashEchoCircle)
            {
                // `$90:EBF3` is installed by `$90:D2BA` at the instant an active
                // shinespark crashes. Unlike the default handler, it intentionally
                // omits charging flare/audio, atmosphere, both arm-cannon priority
                // paths, ordinary departing speed echoes, and grapple graphics. It
                // draws the live body first, then crash slot one before slot zero on
                // odd NMIs. The shared projectile/trail tail below still follows.
                LastShinesparkCrashDrawingHandlerActive = true;
                LastSamusBodyDrawn = _player.Samus.Presentation.Draw(
                    _addressSpace,
                    _sprites,
                    _room.Camera.PixelPosition,
                    _nmi.NmiFrameCounter,
                    _display.ActiveSamusMode7Transform);
                _player.Samus.Presentation.DrawShinesparkCrashEchoes(
                    _addressSpace,
                    _sprites,
                    _room.Camera.PixelPosition,
                    _nmi.NmiFrameCounter);
            }
            else
            {
                bool grappleHandlerInstalled =
                    SamusGrappleMovement.UsesGrappleDrawingHandler(_player.Samus.Grapple.Phase);
                bool grappleBeamSpecificPath = grappleHandlerInstalled &&
                    SamusGrappleMovement.UsesBeamSpecificDrawingPath(_player.Samus.Grapple.Phase);
                LastGrappleDrawingHandlerActive = grappleHandlerInstalled;
                LastGrappleBeamSpecificDrawingPath = grappleBeamSpecificPath;

                if (grappleBeamSpecificPath)
                {
                    // Active `$90:EB86` begins with `$9B:C036`. Firing also refreshes
                    // its hand origins here from Samus's post-movement position. This
                    // flare is a different animation owner from the charge flare below.
                    LastGrappleFlareDrawn = SamusGrappleMovement.DrawFlareBeforeSamus(
                        _addressSpace,
                        _player.Samus,
                        _sprites,
                        _room.Camera.PixelPosition,
                        _artwork.ChargeFlareCompositions,
                        _artwork.StandardObjects);
                }
                else if (!grappleHandlerInstalled)
                {
                    // `$90:EB52` advances/emits the ordinary charge flare before it
                    // falls through to `$90:EB55`. Grapple's handler never calls this,
                    // including its cancel/release fallback frames.
                    // The flare animates as a simulation phase, then shows what it advanced to.
                    ChargeFlareComponents flare = _projectiles.AdvanceChargeFlare(_player.Samus);
                    _projectiles.DrawChargeFlare(
                        _addressSpace,
                        _sprites,
                        _player.Samus,
                        _room.Camera.PixelPosition,
                        flare,
                        _display.ActiveSamusMode7Transform,
                        new ChargeFlareArtwork
                        {
                            Placement = _artwork.ChargeFlarePlacement,
                            Compositions = _artwork.ChargeFlareCompositions,
                            Objects = ProjectileArtwork,
                        });
                }

                // `$90:EB55` begins here for all three routes: ordinary, active grapple,
                // and grapple teardown. Reverse atmospheric-slot order is therefore
                // earlier in OAM than cannon/body, but later than either applicable
                // flare. This exact overlap order is visible through transparent pixels.
                _player.Samus.LiquidPhysics.AtmosphericEffects.UpdateAndDraw(
                    _addressSpace,
                    _sprites,
                    _room.Camera.PixelPosition,
                    _player.Samus.LiquidPhysics.FxYPosition,
                    _artwork.SamusBodyArt?.Spritemaps,
                    _artwork.SamusBodyArt?.Atmosphere,
                    _artwork.StandardObjects);

                // Drawing modes one and two differ only in OAM priority: one appends the
                // cannon before the body, while two appends it after. Mode zero suppresses
                // the independent object even if a HUD-driven cover frame remains nonzero.
                if (_player.Samus.ArmCannon.EffectiveDrawingMode is not 0 and
                    not 2)
                {
                    LastArmCannonDraw = _player.Samus.ArmCannon.Draw(
                        _addressSpace,
                        _sprites,
                        _player.Samus,
                        _room.Camera.PixelPosition,
                        _nmi.NmiFrameCounter);
                }
                LastSamusBodyDrawn = _player.Samus.Presentation.Draw(
                    _addressSpace,
                    _sprites,
                    _room.Camera.PixelPosition,
                    _nmi.NmiFrameCounter,
                    _display.ActiveSamusMode7Transform);
                if (_player.Samus.ArmCannon.EffectiveDrawingMode == 2)
                {
                    LastArmCannonDraw = _player.Samus.ArmCannon.Draw(
                        _addressSpace,
                        _sprites,
                        _player.Samus,
                        _room.Camera.PixelPosition,
                        _nmi.NmiFrameCounter);
                }

                if (grappleBeamSpecificPath)
                {
                    // Active `$90:EB86` deliberately omits `Samus_DrawEchoes`. It updates
                    // endpoint/segment tiles after Samus, increments flare time, and only
                    // then emits rope pieces when length is nonzero.
                    SamusGrappleMovement.DrawConnectedBeam(
                        _addressSpace,
                        _player.Samus.Grapple,
                        _sprites,
                        _room.Camera.PixelPosition,
                        _artwork.GrappleArtwork,
                        _player.Samus.Pose,
                        _artwork.StandardObjects);
                }
                else
                {
                    // Ordinary `$90:EB55` and grapple's signed-range fallback share this
                    // exact echo tail. The latter still suppresses charge flare and rope
                    // because `$90:EB52` and the active half of `$90:EB86` were bypassed.
                    _player.Samus.Presentation.DrawSpeedBoosterEchoes(
                        _addressSpace,
                        _sprites,
                        _room.Camera.PixelPosition);
                    _player.Samus.Presentation.DrawReleasedShinesparkCrashEchoes(
                        _addressSpace,
                        _sprites,
                        _room.Camera.PixelPosition,
                        _nmi.NmiFrameCounter);
                }
            }

            // `$90:EB3B` draws Samus first and immediately calls `$93:8254`. This is
            // deliberately after the grapple beam too in the translated composite pass;
            // all live beam art still receives later OAM indices than Samus's body.
            _projectiles.DrawLiveProjectiles(
                _addressSpace,
                _sprites,
                _room.Camera.PixelPosition,
                _nmi.NmiFrameCounter, ProjectileArtwork);
            // `$93:82F7` immediately follows the ordinary projectile draw with bank
            // `$90:B6A9`. Trails are detached world-space objects, so they must keep
            // animating after their source beam has collided or left the viewport.
            // Trails animate as a simulation phase, under time freeze too, then show where they are.
            _projectiles.AdvanceTrails(_addressSpace, _freeze.TimeIsFrozen, _artwork.TrailArtwork);
            _projectiles.DrawTrails(_sprites, _room.Camera.PixelPosition, _artwork.TrailArtwork, ProjectileArtwork);

            // The native post-draw input snapshot survives the next alpha pass. In
            // particular, Fire can first cancel a spin and then start Grapple without
            // requiring another physical press after the prospective pose is applied.
            _player.Samus.InputSnapshot.SnapshotDrawInput(_controller1.Current, _controller1.NewlyPressed);
            _player.Samus.InputSnapshot.UpdateHurtFlashFromHealthLoss();
            // $91:F1EC never replaces the demo input handler with auto-jump.
            if (_attractSession.IsAttractDemo) _player.Samus.InputSnapshot.AutoJumpInputPending = false;

            // `$90:F576` follows DrawSamusAndProjectiles. A counter-forty hurt update
            // may have armed this latch above; consuming it here preserves both the
            // same-frame charging sound and native ordering after projectile drawing.
            SamusPostDrawAudio.Step(
                _addressSpace,
                _player.Samus,
                _player.PreviousMovementTypeForXray,
                _controller1.Current);
        }

        if (!deathOwnsSamus && _enemies.IsLoaded)
        {
            // At phase three Samus/projectiles are emitted before enemy layer three;
            // ordinary enemy layers four and five follow without another insertion.
            _enemies.Drawing.AdvanceLayers(_room.Camera.PixelPosition, 3, 5);
            _enemies.Drawing.DrawLayers(_sprites, ObjectPage, _room.Camera.PixelPosition, 3, 5);
        }

        if (!deathOwnsSamus && _enemies.IsLoaded &&
            _player.Samus?.Xray.AreEnemyProjectilesSuspended != true)
        {
            _enemies.Projectiles.DrawLowPriorityEnemyProjectiles(
                _sprites, ObjectPage,
                _room.Camera.PixelPosition, _freeze.TimeIsFrozen);
        }

        if (!deathOwnsSamus && _enemies.IsLoaded)
        {
            _enemies.Drawing.AdvanceLayers(_room.Camera.PixelPosition, 6, 7);
            _enemies.Drawing.DrawLayers(_sprites, ObjectPage, _room.Camera.PixelPosition, 6, 7);
        }

        if (!deathOwnsSamus && _enemies.IsLoaded && _enemies.DraygonBehavior.Draygon is { } draygon)
        {
            // Native $A5:9342 runs AFTER drawing enemies. Camera scrolling has already
            // run, so BG2 must use the same final camera as the appendage OAM above.
            // Publishing during enemy AI instead detaches the torso whenever scrolling
            // changes the camera between AI and drawing (especially during a grab).
            _backgroundScroll.SetBg2ScrollRegisters(new Position16<Bg2Pixels>(
                unchecked((ushort)(draygon.BodyGraphicsDisplacement.X + _room.Camera.PixelPosition.X -
                    draygon.Body.Position.X.Whole - DraygonBackgroundData.HorizontalOrigin)),
                unchecked((ushort)(draygon.BodyGraphicsDisplacement.Y + _room.Camera.PixelPosition.Y -
                    draygon.Body.Position.Y.Whole - DraygonBackgroundData.VerticalOrigin))));
        }

        // Room scrolling must not replace the persistent body-owned register shadows.
        // Mother Brain moves BG2 inversely to her physical posture in bank $A9.
        if (!deathOwnsSamus && _enemies.MotherBrainEncounter.MotherBrain is { HasBg2ScrollOverride: true } motherBrain)
            _backgroundScroll.SetBg2ScrollRegisters(motherBrain.Bg2Scroll);

        if (!deathOwnsSamus && _enemies.IsLoaded)
        {
            // CeresRidley_Main installs $A6:A2F2 as EnemyGraphicsDrawnHook. It runs
            // after every ordinary enemy layer and is the only producer of the Baby
            // Metroid OBJ (plus the arena-door overlay). Keeping this after layer seven
            // preserves the hook's actual OAM position instead of inventing a Baby slot.
            _enemies.CeresRidleyBehavior.DrawCeresRidleyPostEnemyHook(
                _oam,
                _room.Camera.PixelPosition);
        }
    }

    /// <summary>
    /// Most recent beam-charge/post-shot body-palette branch from `$91:D743`. This remains
    /// distinct from the global Hyper Beam projectile palette object above: it owns Samus's
    /// OBJ palette four rather than projectile OBJ palette six.
    /// </summary>
    public SamusBeamChargePaletteStepResult LastBeamChargePaletteStep { get; internal set; }

    /// <summary>Most recent ordinary `$91:D83F` visor color-cycle step.</summary>
    public SamusVisorPaletteStepResult LastVisorPaletteStep { get; internal set; }

    /// <summary>
    /// Most recent call of the final-priority ordinary hurt palette handler. The result
    /// exposes both visible palette cadence and counter-forty audio recovery to a debugger.
    /// </summary>
    public SamusHurtFlashPaletteStepResult LastHurtFlashPaletteStep { get; internal set; }

    /// <summary>Most recent `$90:C5C4` arm-cannon cover state update.</summary>
    public SamusArmCannonUpdateResult LastArmCannonUpdate { get; internal set; }

    /// <summary>Most recent optional cannon OBJ and tile-$1F DMA publication.</summary>
    public SamusArmCannonDrawResult LastArmCannonDraw { get; internal set; }

    /// <summary>
    /// Whether the current main-loop frame actually appended Samus's body spritemaps.
    /// This differs from merely reaching the drawing phase: invincibility and the elevator
    /// display handler can intentionally suppress the body on odd accepted NMI frames.
    /// </summary>
    public bool LastSamusBodyDrawn { get; internal set; }

    /// <summary>
    /// True only when the current frame dispatched `$90:EBF3`, the deliberately minimal
    /// shinespark-crash body-plus-two-echo renderer. Exposed separately from phase state so
    /// a debugger can verify that handler installation and presentation agree on a frame.
    /// </summary>
    public bool LastShinesparkCrashDrawingHandlerActive { get; internal set; }

    /// <summary>
    /// True when this frame dispatched grapple's replacement `$90:EB86` Samus handler.
    /// Teardown phases remain true for their one pending frame even though the handler's
    /// signed range test selects ordinary body/cannon/echo rendering inside that routine.
    /// </summary>
    public bool LastGrappleDrawingHandlerActive { get; internal set; }

    /// <summary>
    /// True when `$90:EB86` took its beam-specific flare/body/rope half rather than the
    /// teardown fallback. This is intentionally narrower than handler installation.
    /// </summary>
    public bool LastGrappleBeamSpecificDrawingPath { get; internal set; }

    /// <summary>Whether grapple's bank-$93 flare passed its unsigned screen-Y gate.</summary>
    public bool LastGrappleFlareDrawn { get; internal set; }
}
