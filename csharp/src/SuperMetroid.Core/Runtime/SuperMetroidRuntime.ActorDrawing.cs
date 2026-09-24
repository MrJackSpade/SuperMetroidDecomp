using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Runtime;

public sealed partial class SuperMetroidRuntime
{
    /// <summary>Shared enemy-layer/Samus/projectile drawing, independently of movement.</summary>
    private void DrawGameplayActors(bool deathOwnsSamus,
        Action<OamBuffer>? drawHighPriorityEnemyProjectiles = null,
        Action<OamBuffer>? drawLowPriorityEnemyProjectiles = null,
        bool advanceSamusPalette = true)
    {
        if (Samus is null || Camera is null) return;
        BindGrapplePresentation();
        // $A0:884D draws bomb/projectile explosions before reaching the enemy-layer
        // phase that calls DrawSamusAndProjectiles. Preserve that OAM ordering.
        if (!deathOwnsSamus)
        {
            BombProjectiles.Draw(_addressSpace, Oam, Camera.XPosition, Camera.YPosition, ProjectileCompositions);
            Projectiles.DrawExplosions(_addressSpace, Oam, Camera.XPosition, Camera.YPosition, ProjectileCompositions);
        }

        // `$A0:885D` calls `$86:8390` after bomb/projectile explosions and before the
        // layer loop reaches Samus at layer three. Room-specific actors may supply this
        // pass without teaching the reusable Landing Site runtime how to own enemies.
        if (!deathOwnsSamus && !Samus.Xray.AreEnemyProjectilesSuspended)
        {
            // Both Ceres elevator definitions carry properties $3000, including bit
            // $1000 selected by Draw_HighPriority_EnemyProjectile at `$86:8390`.
            // That named pass occurs here, before enemy layers zero through two and
            // Samus; lower OAM indices retain their native same-priority overlap win.
            CeresElevatorArrival?.Draw(Oam, Camera.XPosition, Camera.YPosition);
            if (Enemies.IsLoaded)
            {
                Enemies.DrawHighPriorityEnemyProjectiles(
                    Oam,
                    Camera.XPosition,
                    Camera.YPosition,
                    TimeIsFrozen);
            }
            drawHighPriorityEnemyProjectiles?.Invoke(Oam);
        }

        // The global layer loop emits layers zero through two before its phase-three
        // call to DrawSamusAndProjectiles. Landing Site's gunship definition selects
        // layer two, so its hull correctly precedes (and can sit behind) Samus OAM.
        if (!deathOwnsSamus && Enemies.IsLoaded)
            Enemies.DrawLayers(Oam, Camera.XPosition, Camera.YPosition, 0, 2);

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
                Samus.HorizontalSpeed.ApplyPendingNormalSuitPaletteRestore(
                    _addressSpace, Cgram, Samus.EquippedItems, mapPresentation?.SamusSuitColors);
            bool drainedOwnsSamusPalette = !deathOwnsSamus &&
                Samus.Drained.UpdatePalette(_addressSpace, Cgram, Samus.EquippedItems,
                    mapPresentation?.SamusSuitColors);
            LastHurtFlashPaletteStep = default;
            LastVisorPaletteStep = default;
            bool chargeGlowRestoredNormalPalette = false;
            if (!deathOwnsSamus && !drainedOwnsSamusPalette)
            {
                // `$91:D708` always runs charge/post-shot handling before dispatching the
                // selected special Samus palette. Ordinary charged shots paint colors 1-15
                // white for three calls and restore the ROM suit on call four; Hyper shots
                // step ten complete bank-$9B palettes on alternating calls before restore.
                LastBeamChargePaletteStep = Projectiles.UpdateBeamChargePalette(
                    _addressSpace,
                    Cgram,
                    Samus,
                    LayerBlendingDefaultConfig);
                LastVisorPaletteStep = Projectiles.LastVisorPaletteStep;
                // Native carry from HandleBeamChargePalettes bypasses the entire special
                // dispatcher on glow expiry. In particular, stored shine must not tick
                // or overwrite the restored suit on this frame.
                chargeGlowRestoredNormalPalette =
                    LastBeamChargePaletteStep.Action == SamusBeamChargePaletteAction.RestoredNormalSuit;

                if (!chargeGlowRestoredNormalPalette)
                    Samus.HorizontalSpeed.UpdateSpeedBoosterPalette(
                    _addressSpace,
                    Cgram,
                    Samus.ReadMovementType(_addressSpace),
                    Samus.AnimationFrame,
                    Samus.EquippedItems,
                    suppressActiveSpeedBoosterPalette:
                        Samus.Shinespark.PaletteType != 0 ||
                        Samus.CrystalFlash.SpecialPaletteKind ==
                            SamusSpecialPaletteType.CrystalFlash ||
                        Samus.Xray.SpecialPaletteKind == SamusSpecialPaletteType.Xray,
                    bottomBoundarySubmerged:
                        Samus.LiquidPhysics.IsBottomBoundarySubmerged(Samus),
                    suitColors: mapPresentation?.SamusSuitColors,
                    cycleColors: mapPresentation?.SamusFullBodyCycleColors);
            }
            // Palette handlers one and six run at the same `$91:D6F7` dispatch point. They
            // execute only when charge handling returned carry clear.
            if (!deathOwnsSamus && !drainedOwnsSamusPalette && !chargeGlowRestoredNormalPalette)
            {
                Samus.Shinespark.UpdatePalette(
                    _addressSpace,
                    Cgram,
                    Samus.EquippedItems,
                    mapPresentation?.SamusSuitColors,
                    mapPresentation?.SamusFullBodyCycleColors);
            }
            // Handler seven owns all sixteen colors of sprite palette six during Crystal
            // Flash. It is mutually exclusive with shinespark/X-ray special handlers but
            // intentionally runs at the same `$91:D6F7` dispatch point.
            if (!deathOwnsSamus && !drainedOwnsSamusPalette && !chargeGlowRestoredNormalPalette)
            {
                Samus.CrystalFlash.UpdatePalette(
                    _addressSpace,
                    Cgram,
                    Samus,
                    BeamArtwork?.Palettes);
            }
            // Handler eight changes only visor color four while active; `$FFFF` teardown
            // restores the complete ROM-selected Power/Varia/Gravity suit palette once.
            if (!deathOwnsSamus && !drainedOwnsSamusPalette && !chargeGlowRestoredNormalPalette)
            {
                Samus.Xray.UpdatePalette(
                    _addressSpace,
                    Cgram,
                    Samus.EquippedItems,
                    mapPresentation?.SamusSuitColors);
            }
            // `$91:D8A5` runs after every charge and special-palette family. A Metroid's
            // nonnegative super-special flag takes the alternating boost/normal branch and
            // returns before ordinary hurt flash, just as drained rainbow's negative flag
            // took the routine's earlier branch.
            bool metroidOwnsSamusPalette = !deathOwnsSamus &&
                !drainedOwnsSamusPalette &&
                SamusSpecialSuperPalette.Update(_addressSpace, Cgram, Samus);
            if (!deathOwnsSamus && !drainedOwnsSamusPalette && !metroidOwnsSamusPalette)
            {
                LastHurtFlashPaletteStep = SamusHurtFlashPalette.Update(
                    _addressSpace,
                    Cgram,
                    Samus,
                    Controller1.Current,
                    mapPresentation?.SamusHurtColors);
            }

        }

        if (deathOwnsSamus)
        {
            if (LastDeathSequenceStep is { DrawPose: true })
            {
                LastSamusBodyDrawn = Samus.Draw(
                    _addressSpace,
                    Oam,
                    Camera.XPosition,
                    Camera.YPosition,
                    mode7Transform: ActiveSamusMode7Transform);
            }
            else if (LastDeathSequenceStep is { DrawExplosion: true })
                Samus.DeathSequence.DrawExplosion(_addressSpace, Oam);
        }
        else
        {
            // `DrawSamusSprites` begins with `$90:C5C4` before dispatching the default
            // drawing handler. HUD-selection stability, cover transition, and the
            // pose-authored before/after-body mode must therefore settle before charge
            // flare, atmospheric, or body OAM is appended.
            LastArmCannonUpdate = Samus.ArmCannon.Update(_addressSpace, Samus);
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
                ElevatorStatus != 0 && SamusState.IsForwardFacingPose(Samus.Pose);
            if (elevatorOwnsSamusDrawing)
            {
                if (ShouldDrawSamusOnElevator(NmiFrameCounter))
                {
                    // Omitting the live counter intentionally selects `Draw`'s default
                    // even value. Native `$90:EB86` calls `$90:85D2`, below `$85E2`'s
                    // ordinary invincibility test, so this path cannot flicker twice.
                    LastSamusBodyDrawn = Samus.Draw(
                        _addressSpace,
                        Oam,
                        Camera.XPosition,
                        Camera.YPosition,
                        mode7Transform: ActiveSamusMode7Transform);
                }
            }
            else if (Samus.Shinespark.Phase is
                     ShinesparkPhase.Crash or ShinesparkPhase.CrashEchoCircle)
            {
                // `$90:EBF3` is installed by `$90:D2BA` at the instant an active
                // shinespark crashes. Unlike the default handler, it intentionally
                // omits charging flare/audio, atmosphere, both arm-cannon priority
                // paths, ordinary departing speed echoes, and grapple graphics. It
                // draws the live body first, then crash slot one before slot zero on
                // odd NMIs. The shared projectile/trail tail below still follows.
                LastShinesparkCrashDrawingHandlerActive = true;
                LastSamusBodyDrawn = Samus.Draw(
                    _addressSpace,
                    Oam,
                    Camera.XPosition,
                    Camera.YPosition,
                    NmiFrameCounter,
                    ActiveSamusMode7Transform);
                Samus.DrawShinesparkCrashEchoes(
                    _addressSpace,
                    Oam,
                    Camera.XPosition,
                    Camera.YPosition,
                    NmiFrameCounter);
            }
            else
            {
                bool grappleHandlerInstalled =
                    SamusGrappleMovement.UsesGrappleDrawingHandler(Samus.Grapple.Phase);
                bool grappleBeamSpecificPath = grappleHandlerInstalled &&
                    SamusGrappleMovement.UsesBeamSpecificDrawingPath(Samus.Grapple.Phase);
                LastGrappleDrawingHandlerActive = grappleHandlerInstalled;
                LastGrappleBeamSpecificDrawingPath = grappleBeamSpecificPath;

                if (grappleBeamSpecificPath)
                {
                    // Active `$90:EB86` begins with `$9B:C036`. Firing also refreshes
                    // its hand origins here from Samus's post-movement position. This
                    // flare is a different animation owner from the charge flare below.
                    LastGrappleFlareDrawn = SamusGrappleMovement.DrawFlareBeforeSamus(
                        _addressSpace,
                        Samus,
                        Oam,
                        Camera.XPosition,
                        Camera.YPosition,
                        ChargeFlareCompositions);
                }
                else if (!grappleHandlerInstalled)
                {
                    // `$90:EB52` advances/emits the ordinary charge flare before it
                    // falls through to `$90:EB55`. Grapple's handler never calls this,
                    // including its cancel/release fallback frames.
                    Projectiles.HandleChargeFlareAndDraw(
                        _addressSpace,
                        Oam,
                        Samus,
                        Camera.XPosition,
                        Camera.YPosition,
                        ActiveSamusMode7Transform,
                        ChargeFlarePlacement,
                        ChargeFlareCompositions);
                }

                // `$90:EB55` begins here for all three routes: ordinary, active grapple,
                // and grapple teardown. Reverse atmospheric-slot order is therefore
                // earlier in OAM than cannon/body, but later than either applicable
                // flare. This exact overlap order is visible through transparent pixels.
                Samus.LiquidPhysics.AtmosphericEffects.UpdateAndDraw(
                    _addressSpace,
                    Oam,
                    Camera.XPosition,
                    Camera.YPosition,
                    Samus.LiquidPhysics.FxYPosition);

                // Drawing modes one and two differ only in OAM priority: one appends the
                // cannon before the body, while two appends it after. Mode zero suppresses
                // the independent object even if a HUD-driven cover frame remains nonzero.
                if (Samus.ArmCannon.EffectiveDrawingMode != 0 &&
                    Samus.ArmCannon.EffectiveDrawingMode != 2)
                {
                    LastArmCannonDraw = Samus.ArmCannon.Draw(
                        _addressSpace,
                        Oam,
                        VramWrites,
                        Samus,
                        Camera.XPosition,
                        Camera.YPosition,
                        NmiFrameCounter);
                }
                LastSamusBodyDrawn = Samus.Draw(
                    _addressSpace,
                    Oam,
                    Camera.XPosition,
                    Camera.YPosition,
                    NmiFrameCounter,
                    ActiveSamusMode7Transform);
                if (Samus.ArmCannon.EffectiveDrawingMode == 2)
                {
                    LastArmCannonDraw = Samus.ArmCannon.Draw(
                        _addressSpace,
                        Oam,
                        VramWrites,
                        Samus,
                        Camera.XPosition,
                        Camera.YPosition,
                        NmiFrameCounter);
                }

                if (grappleBeamSpecificPath)
                {
                    // Active `$90:EB86` deliberately omits `Samus_DrawEchoes`. It updates
                    // endpoint/segment tiles after Samus, increments flare time, and only
                    // then emits rope pieces when length is nonzero.
                    SamusGrappleMovement.DrawConnectedBeam(
                        _addressSpace,
                        Samus.Grapple,
                        Oam,
                        VramWrites,
                        Camera.XPosition,
                        Camera.YPosition,
                        GrappleArtwork,
                        Samus.Pose);
                }
                else
                {
                    // Ordinary `$90:EB55` and grapple's signed-range fallback share this
                    // exact echo tail. The latter still suppresses charge flare and rope
                    // because `$90:EB52` and the active half of `$90:EB86` were bypassed.
                    Samus.DrawSpeedBoosterEchoes(
                        _addressSpace,
                        Oam,
                        Camera.XPosition,
                        Camera.YPosition);
                    Samus.DrawReleasedShinesparkCrashEchoes(
                        _addressSpace,
                        Oam,
                        Camera.XPosition,
                        Camera.YPosition,
                        NmiFrameCounter);
                }
            }

            // `$90:EB3B` draws Samus first and immediately calls `$93:8254`. This is
            // deliberately after the grapple beam too in the translated composite pass;
            // all live beam art still receives later OAM indices than Samus's body.
            Projectiles.DrawLiveProjectiles(
                _addressSpace,
                Oam,
                Camera.XPosition,
                Camera.YPosition,
                NmiFrameCounter, ProjectileCompositions);
            // `$93:82F7` immediately follows the ordinary projectile draw with bank
            // `$90:B6A9`. Trails are detached world-space objects, so they must keep
            // animating after their source beam has collided or left the viewport.
            Projectiles.HandleTrailsAndDraw(
                _addressSpace,
                Oam,
                Camera.XPosition,
                Camera.YPosition,
                TimeIsFrozen, TrailArtwork);

            // The native post-draw input snapshot survives the next alpha pass. In
            // particular, Fire can first cancel a spin and then start Grapple without
            // requiring another physical press after the prospective pose is applied.
            Samus.SnapshotDrawInput(Controller1.Current, Controller1.NewlyPressed);
            // $91:F1EC never replaces the demo input handler with auto-jump.
            if (IsAttractDemo) Samus.AutoJumpInputPending = false;

            // `$90:F576` follows DrawSamusAndProjectiles. A counter-forty hurt update
            // may have armed this latch above; consuming it here preserves both the
            // same-frame charging sound and native ordering after projectile drawing.
            SamusPostDrawAudio.Step(
                _addressSpace,
                Samus,
                PreviousMovementTypeForXray,
                Controller1.Current);
        }

        if (!deathOwnsSamus && Enemies.IsLoaded)
        {
            // At phase three Samus/projectiles are emitted before enemy layer three;
            // ordinary enemy layers four and five follow without another insertion.
            Enemies.DrawLayers(Oam, Camera.XPosition, Camera.YPosition, 3, 5);
        }

        // Phase six inserts high-priority enemy projectiles before layer-six actors.
        // Keep the historical delegate name for API compatibility even though the
        // native source calls this pass DrawHighPriorityEprojs.
        if (!deathOwnsSamus && Samus?.Xray.AreEnemyProjectilesSuspended != true)
            drawLowPriorityEnemyProjectiles?.Invoke(Oam);

        if (!deathOwnsSamus && Enemies.IsLoaded &&
            Samus?.Xray.AreEnemyProjectilesSuspended != true)
        {
            Enemies.DrawLowPriorityEnemyProjectiles(
                Oam,
                Camera.XPosition,
                Camera.YPosition,
                TimeIsFrozen);
        }

        if (!deathOwnsSamus && Enemies.IsLoaded)
            Enemies.DrawLayers(Oam, Camera.XPosition, Camera.YPosition, 6, 7);

        if (!deathOwnsSamus && Enemies.IsLoaded && Enemies.Draygon is { } draygon)
        {
            // Native $A5:9342 runs AFTER drawing enemies. Camera scrolling has already
            // run, so BG2 must use the same final camera as the appendage OAM above.
            // Publishing during enemy AI instead detaches the torso whenever scrolling
            // changes the camera between AI and drawing (especially during a grab).
            BackgroundScroll.SetBg2ScrollRegisters(
                unchecked((ushort)(draygon.BodyGraphicsXDisplacement + Camera.XPosition -
                    draygon.Body.XPosition - DraygonBackgroundData.HorizontalOrigin)),
                unchecked((ushort)(draygon.BodyGraphicsYDisplacement + Camera.YPosition -
                    draygon.Body.YPosition - DraygonBackgroundData.VerticalOrigin)));
        }

        // Room scrolling must not replace the persistent body-owned register shadows.
        // Mother Brain moves BG2 inversely to her physical posture in bank $A9.
        if (!deathOwnsSamus && Enemies.MotherBrain is { HasBg2ScrollOverride: true } motherBrain)
            BackgroundScroll.SetBg2ScrollRegisters(motherBrain.Bg2XScroll, motherBrain.Bg2YScroll);

        if (!deathOwnsSamus && Enemies.IsLoaded)
        {
            // CeresRidley_Main installs $A6:A2F2 as EnemyGraphicsDrawnHook. It runs
            // after every ordinary enemy layer and is the only producer of the Baby
            // Metroid OBJ (plus the arena-door overlay). Keeping this after layer seven
            // preserves the hook's actual OAM position instead of inventing a Baby slot.
            Enemies.DrawCeresRidleyPostEnemyHook(
                Oam,
                Camera.XPosition,
                Camera.YPosition);
        }
    }
}
