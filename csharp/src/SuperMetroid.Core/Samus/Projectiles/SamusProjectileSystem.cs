using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Samus.Grapple;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Palettes;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Samus.Drawing;
using SuperMetroid.Core.Samus.Hud;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// Samus's ordinary projectile slots (see <see cref="ProjectileSlotTable"/> for their slot order).
/// </summary>
/// <remarks>
/// This class deliberately holds the game's own fields rather than a desktop particle
/// effect. Firing, damage and animation records, and terrain movement each keep
/// their own seam. Keeping those seams separate prevents a convenient host sprite from
/// replacing the actual projectile lifecycle.
///
/// The implemented producer covers ordinary and charged plain power beams. The shared slot
/// model and animation are intentionally general enough for the remaining beam
/// and missile families, but unsupported producers are rejected before allocating a slot.
/// </remarks>
[RoomLifetime]
public sealed class SamusProjectileSystem
{
    /// <summary>Tracks held charge, flare components, and charge palette state across room transitions.</summary>
    internal BeamChargeState Charge { get; }

    /// <summary>Holds the five ordinary projectile places and the shot count.</summary>
    internal ProjectileSlotTable SlotTable { get; }

    /// <summary>Collects impact-driven earthquake requests from projectile and terrain reactions.</summary>
    internal ProjectileEarthquakes Earthquakes { get; }

    /// <summary>Manages the special beam window that alters projectile handling.</summary>
    internal ChainsawPowerBombCallback Chainsaw { get; }

    /// <summary>Queues the sound selected for projectile contact with terrain or enemies.</summary>
    internal ProjectileImpactAudio ImpactAudio { get; }

    /// <summary>
    /// The update's audio requests the projectiles' firing, combination and impact sounds join as they are made: the
    /// runtime's, once it points the room's systems at it; the projectiles' own record before that.
    /// </summary>
    internal GameplayAudioRequests AudioRequests { get; private set; } = new();

    /// <summary>Points the projectiles' sounds at <paramref name="requests"/>, the update's audio requests of the runtime running them.</summary>
    internal void BindAudioRequests(GameplayAudioRequests requests)
    {
        AudioRequests = Ensure.NotNull(requests);
        ImpactAudio.BindAudioRequests(requests);
    }

    /// <summary>Owns the projectile visuals emitted by Shinespark echoes.</summary>
    internal ShinesparkEchoProjectile Shinespark { get; }

    /// <summary>Advances special beam combinations that share ordinary projectile storage.</summary>
    internal BeamCombos Combos { get; }

    /// <summary>Plays projectile records and draws their sprite compositions.</summary>
    internal ProjectileAnimation Animation { get; }

    /// <summary>Applies HUD, cooldown, equipment, and charge rules when allocating a shot.</summary>
    internal BeamFiring Firing { get; }

    /// <summary>Advances live projectile positions, camera culling, and trails.</summary>
    internal ProjectileMotion Motion { get; }

    /// <summary>Resolves projectile impact with blocks and breakable terrain.</summary>
    internal ProjectileTerrainCollision TerrainCollision { get; }

    /// <summary>Routes ordinary shots into enemy collision and damage callbacks.</summary>
    internal ProjectileEnemyImpacts EnemyImpacts { get; }

    /// <summary>The trails moving projectiles leave, which run and show until both their sparkles end.</summary>
    internal ProjectileTrails Trails { get; }

    /// <summary>
    /// Whether the posture-transition admission admits weapons for a pose outside its table, which the beam producer and
    /// the Grapple admission consult.
    /// </summary>
    internal PostureAdmissionBeyondTransitions<SamusPoseId> PostureAdmission { get; }

    /// <summary>The angles a Shinespark crash finishing in a drained pose releases its echoes into these slots at.</summary>
    internal DrainedCrashEchoDepartures<SamusPoseId, ShinesparkDepartureAngles> DrainedCrashDepartures { get; }

    /// <summary>Creates one room's five projectile places, its trails and the owners that run them.</summary>
    /// <param name="charge">The held beam charge, flare and charge palette, which outlive each room's projectiles.</param>
    /// <param name="samusOverreads">The installed services reproducing Samus's glitch reads; the projectiles consult their own group of them.</param>
    /// <param name="muzzleCarry">
    /// What each of the room's shot places keeps for its next projectile's horizontal sub-pixel position, created by the
    /// room's builder with the room's shot pool.
    /// </param>
    internal SamusProjectileSystem(BeamChargeState charge, SamusOverreadServices samusOverreads, LeftMuzzleSubpixelCarry muzzleCarry)
    {
        SamusProjectileOverreads overreads = Ensure.NotNull(samusOverreads).Projectiles;
        Charge = Ensure.NotNull(charge);
        PostureAdmission = overreads.PostureAdmission;
        DrainedCrashDepartures = overreads.DrainedCrashDepartures;
        SlotTable = new ProjectileSlotTable(muzzleCarry);
        Earthquakes = new ProjectileEarthquakes();
        ImpactAudio = new ProjectileImpactAudio(AudioRequests);
        Animation = new ProjectileAnimation(Charge, SlotTable, overreads.ChargeFlareDelays);
        TerrainCollision = new ProjectileTerrainCollision(Animation, Earthquakes, ImpactAudio, samusOverreads.StraddledTerrain);
        EnemyImpacts = new ProjectileEnemyImpacts(Animation, Earthquakes, SlotTable, TerrainCollision, overreads.UnauthoredBeamFiring);
        Trails = new ProjectileTrails();
        Chainsaw = new ChainsawPowerBombCallback(Animation, overreads.ChainsawFuseShift);
        Shinespark = new ShinesparkEchoProjectile(Animation, SlotTable);
        Combos = new BeamCombos(Animation, Charge, SlotTable, Trails, overreads.SpaceTimeOverwrite);
        Firing = new BeamFiring(Animation, Charge, Combos, SlotTable, TerrainCollision, overreads.UnauthoredBeamFiring);
        Motion = new ProjectileMotion(Animation, SlotTable, TerrainCollision, Trails);
    }

    /// <summary>
    /// Starts each shot place keeping the horizontal sub-pixel position the room this one replaces left
    /// (<see cref="RoomHandOver"/>, <see cref="LeftMuzzleSubpixelCarry"/>), before any shot is fired.
    /// </summary>
    internal void TakeOverFrom(RoomHandOver handOver)
    {
        Ensure.NotNull(handOver);
        for (int place = 0; place < SlotCount; place++)
            SlotTable.MuzzleCarry.Keep(place, handOver.ShotHorizontalFraction(place));
    }

    /// <summary>
    /// Leaves for the room that replaces this one the horizontal sub-pixel position each shot place keeps as the room's
    /// load clears Samus's shots (<see cref="RoomHandOver"/>, <see cref="LeftMuzzleSubpixelCarry"/>).
    /// </summary>
    internal void HandOverTo(RoomHandOver handOver) =>
        Ensure.NotNull(handOver).LeaveShots(Enumerable.Range(0, SlotCount).Select(SlotTable.SubpixelKeptOnClear));

    /// <summary>How many ordinary shots Samus can have out at once; bombs have their own slots.</summary>
    public const int SlotCount = 5;

    /// <summary>Each shot place's projectile, in place order: the live one, or the last one that ended there.</summary>
    public IReadOnlyList<SamusProjectile> Slots => SlotTable.Slots;

    /// <summary>The most recent charged-shot palette sub-handler result.</summary>
    public SamusBeamChargePaletteStepResult LastBeamChargePaletteStep
    {
        get => Charge.LastBeamChargePaletteStep;
        private set => Charge.LastBeamChargePaletteStep = value;
    }

    /// <summary>Nested visor result when inactive charging falls through to visor handling.</summary>
    public SamusVisorPaletteStepResult LastVisorPaletteStep
    {
        get => Charge.LastVisorPaletteStep;
        private set => Charge.LastVisorPaletteStep = value;
    }

    /// <summary>The last alpha-pass result, whose queued sounds the frontend routes after the frame.</summary>
    public SamusProjectileFrameResult LastFrameResult { get; private set; }

    /// <summary>
    /// Shows the equipped beams' colours: the beams show the shade of the combination they select (#1512). No pictures
    /// change: each projectile shows the sheet of the beams it is pictured with (#1586), and the installed sheets are
    /// pre-coloured in every combination's shade.
    /// </summary>
    public static void LoadBeamPalette(SamusColours colours, SuperMetroid.Core.Progression.SamusBeamFlags equippedBeams) =>
        LoadBeamPalette(colours, SamusBeamCombinations.Of(equippedBeams));

    /// <summary>
    /// Shows a beam combination's shade as the beams' colours: a retail combination's, the glitched Chainsaw beam's or
    /// the Spacetime beam's (#1512).
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">The combination has no shade the game can show.</exception>
    public static void LoadBeamPalette(SamusColours colours, SamusBeamCombination selection)
    {
        Ensure.NotNull(colours);
        switch (selection)
        {
            case ChainsawBeamGraphicsDefinitions.Selection:
            case SpacetimeBeamGraphicsDefinitions.Selection:
            case SamusBeamCombination.Power or SamusBeamCombination.Wave or SamusBeamCombination.Ice or
                SamusBeamCombination.IceWave or SamusBeamCombination.Spazer or SamusBeamCombination.SpazerWave or
                SamusBeamCombination.SpazerIce or SamusBeamCombination.SpazerIceWave or SamusBeamCombination.Plasma or
                SamusBeamCombination.PlasmaWave or SamusBeamCombination.PlasmaIce or SamusBeamCombination.PlasmaIceWave:
                colours.ShowBeams(SamusShades.Beam(selection));
                break;
            case SamusBeamCombination.SpazerPlasma or SamusBeamCombination.SpazerPlasmaIceWave:
                throw new ArgumentOutOfRangeException(nameof(selection), selection, "Beam combination has no translated palette.");
            default:
                throw new ArgumentOutOfRangeException(nameof(selection), selection, "Undefined beam combination.");
        }
    }

    /// <summary>
    /// Runs the beam charge colours against the
    /// modeled Samus palette.
    /// </summary>
    /// <remarks>
    /// This belongs to Samus's suit colouring, not charge-flare drawing. It changes
    /// Samus's colours, which the next displayed frame shows, so the same colours
    /// are written at this phase.
    /// The runtime executes special Speed Booster/Screw/Shinespark/Crystal Flash/X-ray
    /// handlers afterward unless glow expiry returns the normal-suit restoration action.
    /// That action models the restore branch, bypassing special handlers for one frame.
    /// </remarks>
    /// <param name="colours">Samus's shown colours.</param>
    /// <param name="samus">Samus.</param>
    /// <param name="roomVisorAnimation">Whether the room animates Samus's visor (<see cref="RoomFxOverlayState.VisorAnimation"/>).</param>
    public SamusBeamChargePaletteStepResult UpdateBeamChargePalette(
        SamusColours colours,
        SamusState samus,
        RoomVisorAnimation roomVisorAnimation = RoomVisorAnimation.Steady)
    {
        Ensure.NotNull(colours);
        Ensure.NotNull(samus);
        LastVisorPaletteStep = default;

        if (Charge.ChargedShotGlowTimer.IsZero)
        {
            // The charge palette admits the continuously cycling charge palette only while
            // grapple is inactive and the flare counter, which never passes its 120-update clamp, has reached 60.
            bool chargePaletteActive =
                samus.Grapple.Phase == GrapplePhase.Inactive &&
                Charge.FlareCounter >= 60;
            if (chargePaletteActive)
            {
                bool pseudoScrew = samus.HorizontalSpeed.ContactDamageIndex == SamusContactDamageIndex.PseudoScrew;
                // Each suit's list holds a shade for each of the bounded charge steps.
                if (!SamusChargePalettes.IsCompiledChargeStep(Charge.ChargePaletteStep))
                    throw new InvalidDataException(
                        $"Charge palette step {Charge.ChargePaletteStep} is not compiled.");
                colours.ShowSuit(SamusShades.Charge(new SamusChargePalette(
                    pseudoScrew ? SamusChargeKind.PseudoScrew : SamusChargeKind.ChargedBeam,
                    samus.EquippedItems.ShownSuit(), Charge.ChargePaletteStep)));

                Charge.ChargePaletteStep = Charge.ChargePaletteStep >= SamusChargePaletteDefinitions.ChargeSteps - 1
                    ? 0
                    : Charge.ChargePaletteStep + 1;
                LastBeamChargePaletteStep = new(
                    pseudoScrew
                        ? SamusBeamChargePaletteAction.PseudoScrewCycle
                        : SamusBeamChargePaletteAction.ChargeCycle);
                return LastBeamChargePaletteStep;
            }

            // The charge palette resets the sequence whenever charge is below threshold or grapple
            // owns its function. This guarantees the next eligible call starts at
            // the first charge colour step rather than resuming a partially completed cycle.
            Charge.ChargePaletteStep = 0;
            LastVisorPaletteStep = samus.VisorPalette.Update(
                colours,
                samus.Xray.SpecialPaletteKind,
                roomVisorAnimation);
            LastBeamChargePaletteStep = new(
                SamusBeamChargePaletteAction.Inactive);
            return LastBeamChargePaletteStep;
        }

        if (!samus.HyperBeamEnabled)
        {
            // The glow decrements before its zero test. Starting at four therefore produces
            // white on timers 3, 2, and 1. Timer 1 -> 0 takes the restore branch, causing
            // the outer handler to restore the complete suit palette on the fourth call.
            Charge.ChargedShotGlowTimer = Charge.ChargedShotGlowTimer.Decrement().Remaining;
            if (Charge.ChargedShotGlowTimer.IsZero)
            {
                ProjectileAnimation.LoadNormalSuitPalette(colours, samus);
                LastBeamChargePaletteStep = new(
                    SamusBeamChargePaletteAction.RestoredNormalSuit);
                return LastBeamChargePaletteStep;
            }

            // Every visible suit colour shows the glow's yellow; the import pre-colours the shade (#1512).
            colours.ShowSuit(SamusShades.ChargedShotGlow);

            LastBeamChargePaletteStep = new(
                SamusBeamChargePaletteAction.OrdinaryWhite);
            return LastBeamChargePaletteStep;
        }

        // Hyper's sign bit selects this branch but is not part of the glow step. Odd
        // values are pure hold calls; glow steps 10..1 show the Hyper Beam glow's frames 9..0.
        // Each frame consequently remains visible for exactly two calls.
        if (Charge.ChargedShotGlowTimer.IsInAlternatePhase(1))
        {
            Charge.ChargedShotGlowTimer = Charge.ChargedShotGlowTimer.Decrement().Remaining;
            LastBeamChargePaletteStep = new(
                SamusBeamChargePaletteAction.HyperHold);
            return LastBeamChargePaletteStep;
        }

        int glowStep = Charge.ChargedShotGlowTimer.CyclePhase(2, 16);
        if (glowStep == 0)
        {
            // Hyper's final call branches before the decrement. The game explicitly zeroes the
            // already-32768 timer and reports the glow finished so the outer handler performs the suit copy.
            Charge.ChargedShotGlowTimer = WrappingCountdown.Start(0);
            ProjectileAnimation.LoadNormalSuitPalette(colours, samus);
            LastBeamChargePaletteStep = new(
                SamusBeamChargePaletteAction.RestoredNormalSuit);
            return LastBeamChargePaletteStep;
        }

        if (!SamusChargePalettes.IsCompiledHyperShot(glowStep))
            throw new InvalidDataException($"Hyper-shot glow step {glowStep} is not compiled.");
        colours.ShowSuit(SamusShades.HyperShot(SamusChargePalettes.HyperShotFrame(glowStep)));
        Charge.ChargedShotGlowTimer = Charge.ChargedShotGlowTimer.Decrement().Remaining;
        LastBeamChargePaletteStep = new(
            SamusBeamChargePaletteAction.HyperPalette);
        return LastBeamChargePaletteStep;
    }

    /// <summary>
    /// Runs the power-beam HUD producer and then handles all allocated ordinary slots: <see cref="FireFromHud"/>, then
    /// <see cref="HandleShots"/>.
    /// </summary>
    /// <param name="terrain">SamusTerrain</param>
    /// <param name="samus">Mutable player owner supplying pose, equipment, charge, and HUD selection.</param>
    /// <param name="buttons">The buttons Samus's projectile producer reads: held buttons, this frame's new presses, and the previous frame's new presses (#633).</param>
    /// <param name="cameraPosition">Horizontal and vertical camera origin in whole room pixels, used by projectile offscreen handling.</param>
    /// <param name="sharedProjectiles">
    /// Bomb-slot owner carrying the shared projectile cooldown. The runtime steps that
    /// clock once, immediately before calling this method; this method may replace it after
    /// a successful shot just as the game does.
    /// </param>
    public SamusProjectileFrameResult StepFrame(SamusTerrain terrain, SamusState samus, ProjectileButtonInput buttons, Position16<RoomPixels> cameraPosition, SamusBombProjectileSystem sharedProjectiles)
    {
        FireFromHud(terrain, samus, buttons, sharedProjectiles, projectileProducerEnabled: true);
        HandleShots(terrain, samus, cameraPosition, sharedProjectiles);
        return LastFrameResult;
    }

    /// <summary>
    /// Runs the power-beam HUD producer, the beam and missile share of Samus's HUD handler, which the game runs before it
    /// handles any projectile: the producer fires and queues its firing sound as it fires (#1620), so the sound is tested
    /// against the power bomb as it stood before the update's bombs ran. The pass's result reports the firing sound.
    /// </summary>
    /// <param name="terrain">SamusTerrain</param>
    /// <param name="samus">Mutable player owner supplying pose, equipment, charge, and HUD selection.</param>
    /// <param name="buttons">The buttons Samus's projectile producer reads: held buttons, this frame's new presses, and the previous frame's new presses (#633).</param>
    /// <param name="sharedProjectiles">Bomb-slot owner carrying the shared projectile cooldown, which a successful shot replaces.</param>
    /// <param name="projectileProducerEnabled">Whether to admit the HUD weapon producer; the shots are handled whether or not it runs.</param>
    public void FireFromHud(SamusTerrain terrain, SamusState samus, ProjectileButtonInput buttons, SamusBombProjectileSystem sharedProjectiles, bool projectileProducerEnabled)
    {
        Ensure.NotNull(terrain.Level);
        Ensure.NotNull(samus);
        Ensure.NotNull(sharedProjectiles);

        SoundEffectId? queuedSound = null;
        SoundRequestPriority? queuedSoundMaximum = null;

        // Humanoid projectile dispatch is selected by the live HUD item. Indices zero/three
        // share the beam producer; index one reaches the missile producer. Ball poses still
        // dispatch to the companion bomb owner, and the debug grapple route disables this
        // producer explicitly while it owns Shoot.
        // The forward-facing HUD handler branches straight to projectile handling for either
        // forward-facing pose. In particular an elevator may keep accepting physical
        // controller samples without allowing them to charge or fire a weapon.
        // Spin, wall-jump, hurt and special entries dispatch the jumping HUD selection handler, not
        // the normal beam producer. With grapple inactive that handler does
        // nothing: held/released Shoot must leave the charge untouched.
        // Existing projectiles still advance below. Pose input can leave spin
        // first, making the normal producer eligible on the following alpha.
        SamusHudHandler hudHandler = SamusHudObservations.MovementHandler(SamusPoseMetadata.ReadMovementType(samus));
        bool preservesJumpHandlerCharge = samus.Grapple.Phase == GrapplePhase.Inactive &&
            hudHandler == SamusHudHandler.Jump;
        // Turning HUD dispatch waits for an explicit pose-initializer muzzle handoff.
        // With no handoff it neither increments charge nor releases it on a Shoot edge.
        bool preservesTurnCharge = hudHandler == SamusHudHandler.Turning &&
            samus.PendingTransitionShot is null;
        // Posture-transition dispatch preserves the carried charge during morph art;
        // ordinary crouch/stand transitions still run their beam producer.
        bool preservesPostureCharge = hudHandler == SamusHudHandler.Transition &&
            !SamusHudInput.PostureTransitionAdmitsWeapons(samus.Pose,
                grappleActive: samus.Grapple.Phase != GrapplePhase.Inactive, PostureAdmission);
        if (projectileProducerEnabled && !preservesJumpHandlerCharge && !preservesTurnCharge && !preservesPostureCharge && !SamusPoseMetadata.IsForwardFacingPose(samus.Pose) &&
            !SamusPoseMetadata.IsStableBallPose(samus.Pose))
        {
            // The X-ray HUD handler calls the normal beam handler when selected X-ray's Run
            // action is released. Holding Run owns this dispatch even if setup is
            // rejected; selection alone must not consume Shoot. Inputs are normalized
            // by the runtime, so the canonical Run bit also honors remapped bindings.
            bool xrayBeamFallback = samus.SelectedHudItem == HudItem.XRay &&
                (buttons.ControllerInput & ControllerButton.B) == ControllerButton.None;
            if (samus.SelectedHudItem is HudItem.Beam or HudItem.PowerBomb || xrayBeamFallback)
            {
                (_, queuedSound, queuedSoundMaximum) = Firing.HandleBeamInput(
                    terrain.Level,
                    samus,
                    buttons.ControllerInput,
                    // Uncharged firing tests both current and prior new-input latches.
                    buttons.ControllerNewInput | buttons.ControllerPreviousNewInput,
                    sharedProjectiles,
                    terrain.RoomObjects);
            }
            else if (samus.SelectedHudItem is HudItem.Missile or HudItem.SuperMissile)
            {
                (_, queuedSound) = Firing.TryFireMissile(
                    samus,
                    buttons.ControllerNewInput,
                    buttons.ControllerPreviousNewInput,
                    sharedProjectiles);
                if (queuedSound is not null)
                    queuedSoundMaximum = SoundRequestPriority.Normal;
            }
        }
        // Firing queues its sound as it fires, before any projectile moves.
        if (queuedSound is { } firingSound)
        {
            AudioRequests.QueueSound(firingSound,
                queuedSoundMaximum ?? throw new InvalidDataException("A projectile sound was queued without its queue limit."));
        }

        LastFrameResult = new SamusProjectileFrameResult(queuedSound, queuedSoundMaximum);
    }

    /// <summary>
    /// Handles all allocated ordinary slots, in update order, after the bombs (<see cref="SamusBombProjectileSystem.HandleBombs"/>):
    /// the shots' share of the game's projectile handling.
    /// </summary>
    /// <param name="terrain">SamusTerrain</param>
    /// <param name="samus">Mutable player owner supplying pose, equipment and the X-ray freeze.</param>
    /// <param name="cameraPosition">Horizontal and vertical camera origin in whole room pixels, used by projectile offscreen handling.</param>
    /// <param name="sharedProjectiles">Bomb-slot owner carrying the shared cooldown and the power bomb the shots read.</param>
    public void HandleShots(SamusTerrain terrain, SamusState samus, Position16<RoomPixels> cameraPosition, SamusBombProjectileSystem sharedProjectiles)
    {
        Ensure.NotNull(terrain.Level);
        Ensure.NotNull(samus);
        Ensure.NotNull(sharedProjectiles);

        bool collisionStartedExplosion = false;
        bool projectileDeleted = false;

        // Bombs were handled by the companion class first; the shots run in update order here because a delete cue
        // changes the counter. The HUD producer can activate X-ray during this pass, so the freeze is checked
        // before each slot, not only at entry.
        foreach (SamusProjectile slot in SlotTable.InUpdateOrder)
        {
            if (samus.Xray.TimeIsFrozen)
                break;
            if (slot.Playback is null)
                continue;

            if (slot.FrameBehavior == SamusProjectileFrameBehavior.ShinesparkEcho)
            {
                Shinespark.StepShinesparkEcho(samus, slot, cameraPosition);
                projectileDeleted |= !slot.IsActive;
            }
            else if (slot.FrameBehavior is SamusProjectileFrameBehavior.IceCombo or
                SamusProjectileFrameBehavior.IceComboOutward or SamusProjectileFrameBehavior.WaveCombo or
                SamusProjectileFrameBehavior.SpazerCombo or SamusProjectileFrameBehavior.SpazerComboFalling or
                SamusProjectileFrameBehavior.PlasmaCombo)
            {
                SoundEffectId? comboSound = null;
                if (slot.FrameBehavior is SamusProjectileFrameBehavior.IceCombo or SamusProjectileFrameBehavior.IceComboOutward)
                    comboSound = Combos.StepIceCombo(samus, slot, sharedProjectiles, cameraPosition);
                else if (slot.FrameBehavior == SamusProjectileFrameBehavior.WaveCombo)
                    comboSound = Combos.StepWaveCombo(samus, slot, sharedProjectiles);
                else if (slot.FrameBehavior == SamusProjectileFrameBehavior.PlasmaCombo)
                    Combos.StepPlasmaCombo(samus, slot, sharedProjectiles, cameraPosition);
                else comboSound = Combos.StepSpazerCombo(samus, slot, sharedProjectiles, cameraPosition.Y);
                projectileDeleted |= !slot.IsActive;
                if (comboSound is SoundEffectId sound)
                    AudioRequests.QueueSound(sound, SoundRequestPriority.Normal);
            }
            else if (slot.FrameBehavior == SamusProjectileFrameBehavior.NoWaveBeam)
            {
                collisionStartedExplosion |= Motion.RunNoWaveBeamFrameBehavior(
                    terrain.Level,
                    slot,
                    cameraPosition,
                    terrain.RoomObjects);
            }
            else if (slot.FrameBehavior is
                SamusProjectileFrameBehavior.WaveBeamThreeFrameTrail or
                SamusProjectileFrameBehavior.WaveBeamFourFrameTrail)
            {
                Motion.RunWaveBeamFrameBehavior(terrain.Level, slot, cameraPosition, terrain.RoomObjects);
            }
            else if (slot.FrameBehavior == SamusProjectileFrameBehavior.HyperBeam)
            {
                Motion.RunHyperBeamFrameBehavior(terrain.Level, slot, cameraPosition, terrain.RoomObjects);
            }
            else if (slot.FrameBehavior == SamusProjectileFrameBehavior.Missile)
            {
                collisionStartedExplosion |= Motion.RunMissileFrameBehavior(
                    terrain.Level,
                    samus,
                    slot,
                    cameraPosition,
                    sharedProjectiles,
                    terrain.RoomObjects);
            }
            else if (slot.FrameBehavior == SamusProjectileFrameBehavior.SuperMissile)
            {
                collisionStartedExplosion |= Motion.RunSuperMissileFrameBehavior(
                    terrain.Level,
                    samus,
                    slot,
                    cameraPosition,
                    sharedProjectiles,
                    terrain.RoomObjects);
            }
            else if (slot.FrameBehavior == SamusProjectileFrameBehavior.SuperMissileLink)
            {
                Motion.RunSuperMissileLinkFrameBehavior(slot);
            }
            else if (slot.FrameBehavior == SamusProjectileFrameBehavior.SpacetimePaletteCopyTail)
            {
                Combos.RunSpacetimePaletteCopyTail(terrain.Progression, slot);
            }
            else if (slot.FrameBehavior == SamusProjectileFrameBehavior.ChainsawPowerBomb)
            {
                Chainsaw.RunPowerBombFrameBehavior(
                    terrain.Level,
                    samus,
                    slot,
                    sharedProjectiles,
                    terrain.RoomObjects);
                projectileDeleted |= !slot.IsActive;
            }
            else if (slot.FrameBehavior ==
                SamusProjectileFrameBehavior.ChargedChainsawOverrun)
            {
                // Charged combination thirteen does not point at game code. Its table
                // overrun reads a value of Samus's state, so the game jumps there and begins
                // decoding the cached previous-Super-Missile value as code. This semantic
                // model cannot execute arbitrary mutable data as code; fail at the
                // same callback-dispatch seam instead of silently substituting Wave motion.
                throw new NotSupportedException(
                    "Charged Chainsaw callback enters Samus's mutable state as code; reproducing it needs full " +
                    "memory and processor emulation, which is outside the port's scope.");
            }
            else if (slot.FrameBehavior ==
                SamusProjectileFrameBehavior.MurderBeamMisalignedExecution)
            {
                // Safe left-facing Murder Beam has no animation and never
                // enters the projectile update. Other directions retain an animation;
                // the overread callback then jumps into unrelated code and the
                // game stops making forward progress. Keep that unsafe execution
                // explicit instead of silently substituting ordinary beam motion.
                throw new NotSupportedException(
                    "Unsafe Murder Beam callback enters the middle of unrelated game code; reproducing it needs full " +
                    "memory and processor emulation, which is outside the port's scope. Only the safe left-facing " +
                    "zero-list shot is translated.");
            }

            // Killing a projectile replaces rather than clears a live beam. Consequently the
            // explosion's first record is selected in this same handler pass.
            if (slot.Playback is not null)
                projectileDeleted |= Animation.RunProjectileInstructionHandler(slot);
        }
    }

    /// <summary>
    /// Advances the flare draw's three charge-flare animation components as a simulation phase and reports which
    /// components this frame shows, in order. Nothing here depends on the display.
    /// </summary>
    public ChargeFlareComponents AdvanceChargeFlare(SamusState samus)
    {
        Ensure.NotNull(samus);

        if (Charge.FlareCounter == 0)
            return default;

        if (samus.HyperBeamEnabled)
        {
            // Hyper Beam replaces the ordinary count-up flare with the flare's descending
            // three-component program. The game iterates fast spark -> slow spark -> main
            // flare (offsets four, two, zero), so preserve that reverse drawing order.
            for (int component = 2; component >= 0; component--)
            {
                CountdownStep timer = Charge.DecrementFlareTimer(component);
                if (timer.IsZeroOrNegative)
                {
                    bool finalFrame = Charge.FlareFrame(component) == 1;
                    Charge.SetFlareFrame(component, WrappingCounter.DecrementCount(Charge.FlareFrame(component)));
                    if (finalFrame)
                    {
                        // The fast-spark component, the third, owns the global
                        // completion signal. Other zero frames merely stop rearming.
                        if (component == 2)
                            Charge.FlareCounter = 0;
                    }
                    else
                    {
                        Charge.StartFlareTimer(component, 3);
                    }
                }
            }
            // Every component is shown, the fast spark first, even on the frame that ends the flare.
            return new ChargeFlareComponents(3, Descending: true);
        }

        // Calls 1..14 initialize/retain the hidden animation state. Call 15 begins drawing
        // the central flare; calls 30+ additionally draw both orbiting spark components.
        if (Charge.FlareCounter == 1)
        {
            Charge.StartChargeFlare();
        }
        if (Charge.FlareCounter < 15)
            return default;

        int componentCount = Charge.FlareCounter < 30 ? 1 : 3;
        for (int component = 0; component < componentCount; component++)
            Animation.AdvanceFlareComponent(component);
        return new ChargeFlareComponents(componentCount, Descending: false);
    }

    /// <summary>Publishes the charge-flare components <see cref="AdvanceChargeFlare"/> reported, before Samus.</summary>
    public void DrawChargeFlare(
        SpriteFrameBuffer sprites,
        SamusState samus,
        Position16<RoomPixels> cameraPosition,
        ChargeFlareComponents components,
        TurningFloorTransform? turningFloor,
        ChargeFlareArtwork artwork)
    {
        Ensure.NotNull(sprites);
        Ensure.NotNull(samus);
        foreach (int component in components.DrawOrder)
            Animation.DrawFlareComponent(sprites, samus, cameraPosition, component, turningFloor, artwork);
    }

    /// <summary>Draws the live ordinary-projectile subset after Samus.</summary>
    public void DrawLiveProjectiles(
        SpriteFrameBuffer sprites,
        Position16<RoomPixels> cameraPosition,
        int frameCounter,
        ProjectileDrawArtwork artwork)
    {
        Ensure.NotNull(sprites);

        foreach (SamusProjectile slot in SlotTable.InUpdateOrder)
        {
            if (!slot.IsActive ||
                slot.Type.Family.IsBombOrExplosion)
                continue;

            // Only an ordinary uncharged beam reaches the flicker branches; a charged beam,
            // missile or explosion remains visible on every frame parity.
            if (slot.Type.Family == SamusProjectileFamily.Beam && !slot.Type.IsChargedBeam)
            {
                // Shots in odd and even slots flicker on opposite frames. Spazer/plasma combinations flicker every
                // two updates and with the opposite phase from power/ice/wave, which flicker every update. Keeping these
                // branches separate is what prevents the wider beam art from disappearing on the wrong video field.
                bool oddSlot = (slot.Place & 1) != 0;
                bool skip = slot.Type.HasAnyBeam(SamusBeamFlags.Spazer | SamusBeamFlags.Plasma)
                    ? oddSlot != SamusCadenceDefinitions.WideBeamDrawnInOddSlot.IsSetIn(frameCounter)
                    : oddSlot == SamusCadenceDefinitions.BeamDrawnInEvenSlot.IsSetIn(frameCounter);
                if (skip)
                    continue;
            }

            // Unlike the bomb/explosion pass, the game's live projectile drawing does not
            // cull by world X. The sprite is published wherever it lies and the display clips it.
            // This also preserves sprite ordering/capacity when a combo orbits offscreen.
            if (slot.Sprite is not null)
                ProjectileAnimation.DrawSlot(sprites, new SpriteSource(SpriteSourceKind.SamusProjectile, slot.Place),
                    slot, cameraPosition, artwork);
        }
    }

    /// <summary>
    /// Advances every trail one update, after the ordinary projectile pass, and removes the trails that have ended. Each
    /// side owns its own timer, program position and position.
    /// </summary>
    /// <param name="timeIsFrozen">
    /// Whether time is frozen. Frozen trails retain their current record and are still drawn;
    /// their timers and embedded position commands do not advance.
    /// </param>
    public void AdvanceTrails(bool timeIsFrozen) => Trails.Advance(timeIsFrozen);

    /// <summary>Publishes every live trail side, the newest trail first.</summary>
    /// <param name="sprites">The frame's sprite description.</param>
    /// <param name="cameraPosition">Whole-pixel room camera origin subtracted from trail coordinates.</param>
    /// <param name="objects">The projectile sheet the trail poses are drawn from.</param>
    public void DrawTrails(
        SpriteFrameBuffer sprites,
        Position16<RoomPixels> cameraPosition,
        ProjectileDrawArtwork objects) => Trails.Draw(sprites, cameraPosition, objects);

    /// <summary>
    /// Draws beam explosions in enemy collision's earlier bomb/explosion phase.
    /// </summary>
    public void DrawExplosions(
        SpriteFrameBuffer sprites,
        Position16<RoomPixels> cameraPosition,
        ProjectileDrawArtwork artwork)
    {
        Ensure.NotNull(sprites);

        foreach (SamusProjectile slot in SlotTable.InUpdateOrder)
        {
            SamusProjectileFamily family = slot.Type.Family;
            if (!slot.IsActive || family is not (
                    SamusProjectileFamily.BeamExplosion or
                    SamusProjectileFamily.MissileExplosion))
                continue;

            ProjectileAnimation.DrawSlot(sprites, new SpriteSource(SpriteSourceKind.SamusProjectile, slot.Place),
                slot, cameraPosition, artwork);
        }
    }

    /// <summary>
    /// Fully clears projectile storage and held-charge presentation for forced owners such as
    /// elevator boarding and the intro flashback. A room's load builds a new system instead,
    /// whose held charge is the session's.
    /// </summary>
    public void Reset(SamusState? samus = null)
    {
        ClearProjectileData(samus);
        Charge.FlareCounter = 0;
        Charge.PreviousBeamChargeCounter = 0;
        Charge.ChargedShotGlowTimer = WrappingCountdown.Start(0);
        Charge.ClearFlare();
        LastBeamChargePaletteStep = default;
        LastVisorPaletteStep = default;
        Charge.ChargePaletteStep = 0;
    }

    /// <summary>
    /// Clears projectile storage without the flare
    /// counter, flare animation state or charge palette state, which <see cref="Reset"/> clears after it.
    /// </summary>
    private void ClearProjectileData(SamusState? samus)
    {
        if (samus is not null)
        {
            samus.HorizontalSpeed.ClearSpeedEchoes();
            samus.Shinespark.ResetProjectileEchoStorage();
        }
        Combos.ComboState = 0;
        SlotTable.ClearAll();
        Trails.Clear();
        SlotTable.ProjectileCounter = 0;
        Firing.ProjectileInvincibilityTimer = 0;
        LastFrameResult = default;
    }

}
