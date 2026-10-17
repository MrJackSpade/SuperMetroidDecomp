using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Samus.Grapple;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Palettes;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Samus.Drawing;
using SuperMetroid.Core.Samus.Hud;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// The five ordinary Samus-projectile slots at WRAM byte indices <c>$00-$08</c>.
/// </summary>
/// <remarks>
/// This class is deliberately a field-for-field translation rather than a desktop particle
/// effect. Firing comes from bank $90, damage and animation records come from bank $93,
/// and terrain movement comes from bank $94. Keeping those seams visible makes every value
/// inspectable in a C# debugger and prevents a convenient host sprite from replacing the
/// actual cartridge projectile lifecycle.
///
/// The implemented producer covers ordinary and charged plain power beams. The shared slot
/// model and bank-$93 interpreter are intentionally general enough for the remaining beam
/// and missile families, but unsupported producers are rejected before allocating a slot.
/// </remarks>
public sealed class SamusProjectileSystem
{
    /// <summary>Tracks held charge, flare components, and charge palette state across room transitions.</summary>
    internal BeamChargeState Charge { get; }

    /// <summary>Holds the five ordinary projectile slots and eighteen two-sided trail slots.</summary>
    internal ProjectileSlotTable SlotTable { get; }

    /// <summary>Collects impact-driven earthquake requests from projectile and terrain reactions.</summary>
    internal ProjectileEarthquakes Earthquakes { get; }

    /// <summary>Manages the special beam window that alters projectile handling.</summary>
    internal ChainsawWindow Chainsaw { get; }

    /// <summary>Queues the sound selected for projectile contact with terrain or enemies.</summary>
    internal ProjectileImpactAudio ImpactAudio { get; }

    /// <summary>Owns the projectile visuals emitted by Shinespark echoes.</summary>
    internal ShinesparkEchoProjectile Shinespark { get; }

    /// <summary>Advances special beam combinations that share ordinary projectile storage.</summary>
    internal BeamCombos Combos { get; }

    /// <summary>Interprets bank-$93 projectile records and draws their sprite compositions.</summary>
    internal ProjectileAnimation Animation { get; }

    /// <summary>Applies HUD, cooldown, equipment, and charge rules when allocating a shot.</summary>
    internal BeamFiring Firing { get; }

    /// <summary>Advances live projectile positions, camera culling, and trails.</summary>
    internal ProjectileMotion Motion { get; }

    /// <summary>Resolves projectile impact with blocks and breakable terrain.</summary>
    internal ProjectileTerrainCollision TerrainCollision { get; }

    /// <summary>Routes ordinary shots into enemy collision and damage callbacks.</summary>
    internal ProjectileEnemyImpacts EnemyImpacts { get; }

    /// <summary>Allocates and advances the two-sided trail records left by moving projectiles.</summary>
    internal ProjectileTrails Trails { get; }

    /// <summary>Creates the five projectile slots, eighteen trail slots and the owners that run them.</summary>
    public SamusProjectileSystem()
    {
        Charge = new BeamChargeState();
        SlotTable = new ProjectileSlotTable();
        Earthquakes = new ProjectileEarthquakes();
        ImpactAudio = new ProjectileImpactAudio();
        Animation = new ProjectileAnimation(Charge, SlotTable);
        TerrainCollision = new ProjectileTerrainCollision(Animation, Earthquakes, ImpactAudio);
        EnemyImpacts = new ProjectileEnemyImpacts(Animation, Earthquakes, SlotTable, TerrainCollision);
        Trails = new ProjectileTrails(SlotTable);
        Chainsaw = new ChainsawWindow(Animation);
        Shinespark = new ShinesparkEchoProjectile(Animation, SlotTable);
        Combos = new BeamCombos(Animation, Charge, SlotTable, Trails);
        Firing = new BeamFiring(Animation, Charge, Combos, SlotTable, TerrainCollision);
        Motion = new ProjectileMotion(Animation, SlotTable, TerrainCollision, Trails);
    }
    /// <summary>Native ordinary-projectile capacity; bombs occupy the other five slots.</summary>
    public const int SlotCount = 5;

    /// <summary>
    /// WRAM allocates byte indices <c>$00-$22</c>, inclusive, for projectile trails.
    /// Because every parallel array is word-indexed, that is eighteen independent slots.
    /// </summary>
    public const int TrailSlotCount = 18;

    /// <summary>The slots in native low-to-high byte-index order: $00, $02, ... $08.</summary>
    public IReadOnlyList<SamusProjectileSlot> Slots => SlotTable._slots;

    /// <summary>The most recent `$91:D743` charged-shot palette sub-handler result.</summary>
    public SamusBeamChargePaletteStepResult LastBeamChargePaletteStep { get; private set; }

    /// <summary>Nested `$91:D83F` result when inactive charging falls through to visor handling.</summary>
    public SamusVisorPaletteStepResult LastVisorPaletteStep { get; private set; }

    /// <summary>The last alpha-pass result, retained for debugger watches and verification.</summary>
    public SamusProjectileFrameResult LastFrameResult { get; private set; }

    /// <summary>
    /// Executes the palette half of <c>Update_Beam_Tiles_and_Palette</c> at $90:AC8D for the equipped beam: sixteen
    /// words into sprite palette six, CGRAM colors $E0-$EF. Its characters are not copied: the projectile OBJ
    /// pages bind the equipped beam's characters (<see cref="SuperMetroid.Core.Samus.Artwork.ProjectileObjectPages"/>).
    /// </summary>
    public static void LoadBeamPalette(
        SnesCgram cgram,
        SuperMetroid.Core.Samus.Projectiles.SamusBeamFlags equippedBeams,
        SuperMetroid.Core.Samus.Artwork.BeamPaletteCatalog? palettes)
    {
        Ensure.NotNull(cgram);
        LoadSelectedBeamPalette(cgram, SelectedCombination(equippedBeams), palettes);
    }

    /// <summary>
    /// $90:AC8D/$90:ACCD index their tables by the equipped-beam word's low twelve bits; an
    /// index above $F (unknown bits 4-11 set) selects no beam combination.
    /// </summary>
    private static SamusBeamCombination SelectedCombination(SuperMetroid.Core.Samus.Projectiles.SamusBeamFlags equippedBeams) =>
        new SamusBeamLoadoutWord(equippedBeams).Combination ?? throw new ArgumentOutOfRangeException(
            nameof(equippedBeams), equippedBeams, "Equipped-beam index is outside the beam-combination tables.");

    /// <summary>Loads a selected beam's final CGRAM palette from installed art or the special beam definitions.</summary>
    private static void LoadSelectedBeamPalette(SnesCgram cgram,
        SamusBeamCombination selection, SuperMetroid.Core.Samus.Artwork.BeamPaletteCatalog? palettes)
    {
        switch (selection)
        {
            case ChainsawBeamGraphicsDefinitions.Selection:
                LoadChainsawPalette(cgram);
                break;
            case SpacetimeBeamGraphicsDefinitions.Selection:
                SpacetimeBeamGraphicsDefinitions.LoadPalette(cgram);
                break;
            case SamusBeamCombination.Power or SamusBeamCombination.Wave or SamusBeamCombination.Ice or
                SamusBeamCombination.IceWave or SamusBeamCombination.Spazer or SamusBeamCombination.SpazerWave or
                SamusBeamCombination.SpazerIce or SamusBeamCombination.SpazerIceWave or SamusBeamCombination.Plasma or
                SamusBeamCombination.PlasmaWave or SamusBeamCombination.PlasmaIce or SamusBeamCombination.PlasmaIceWave:
                (palettes ?? throw new InvalidOperationException("Beam palette requires installed artwork.")).LoadTo(cgram, selection);
                break;
            case SamusBeamCombination.SpazerPlasma or SamusBeamCombination.SpazerPlasmaIceWave:
                throw new ArgumentOutOfRangeException(nameof(selection), selection, "Beam combination has no translated palette.");
            default:
                throw new ArgumentOutOfRangeException(nameof(selection), selection, "Undefined beam combination.");
        }
    }

    /// <summary>Writes the glitched Chainsaw beam palette's sixteen colours into the beam palette.</summary>
    private static void LoadChainsawPalette(SnesCgram cgram)
    {
        var beam = new SamusPalettes(cgram).Beam;
        for (int color = 0; color < ChainsawBeamGraphicsDefinitions.Palette.Count; color++)
            beam.Write(color, ChainsawBeamGraphicsDefinitions.Palette[color]);
    }
    /// <summary>
    /// Runs <c>HandleBeamChargePalettes</c> at <c>$91:D743-$D7D4</c> against the
    /// modeled Samus CGRAM palette.
    /// </summary>
    /// <remarks>
    /// This belongs to the Samus palette handler, not charge-flare drawing. On hardware the
    /// main thread edits palette-buffer colors 192-207 and a later NMI uploads them. The
    /// software PPU exposes CGRAM directly, so the same words are written at this phase.
    /// The runtime executes special Speed Booster/Screw/Shinespark/Crystal Flash/X-ray
    /// handlers afterward unless glow expiry returns the normal-suit restoration action.
    /// That action models carry set at `$91:D70B`, bypassing special handlers for one frame.
    /// </remarks>
    public SamusBeamChargePaletteStepResult UpdateBeamChargePalette(
        SnesCgram cgram,
        SamusState samus,
        LayerBlendingConfiguration layerBlendingDefaultConfig =
            LayerBlendingConfiguration.NormalGameplay)
    {
        Ensure.NotNull(cgram);
        Ensure.NotNull(samus);
        LastVisorPaletteStep = default;

        ushort timerBefore = Charge.ChargedShotGlowTimer.RawValue;
        if (timerBefore == 0)
        {
            // `$91:D748-$D793` admits the continuously cycling charge palette only while
            // grapple's function pointer is exactly inactive and the flare counter has
            // reached 60. The 65816 BMI is a signed subtraction test, retained literally
            // so Hyper's `$8000` sentinel follows cartridge behavior too.
            bool chargePaletteActive =
                samus.Grapple.Phase == GrapplePhase.Inactive &&
                Charge.FlareCounter != 0 &&
                unchecked((short)(Charge.FlareCounter - 60)) >= 0;
            if (chargePaletteActive)
            {
                bool pseudoScrew = samus.HorizontalSpeed.ContactDamageIndex == SamusContactDamageIndex.PseudoScrew;
                ushort suitOffset = ProjectileAnimation.GetSuitPaletteOffset(samus.EquippedItems);

                // The installed path compiles the two pointer levels for bounded native
                // offsets. Restored non-catalog offsets still execute both indirect reads.
                ushort palettePointer = 0;
                bool catalogued = samus.Presentation.ChargeColors is not null &&
                    (pseudoScrew
                        ? SamusChargePaletteDefinitions.TryPseudoScrew(
                            suitOffset, Charge.SamusChargePaletteIndex, out palettePointer)
                        : SamusChargePaletteDefinitions.TryChargedBeam(
                            suitOffset, Charge.SamusChargePaletteIndex, out palettePointer));
                if (!catalogued)
                    throw new InvalidDataException(
                        $"Charge palette offset {Charge.SamusChargePaletteIndex} is not compiled.");
                int paletteIndex = Charge.SamusChargePaletteIndex / 2;
                (samus.Presentation.ChargeColors ?? throw new InvalidOperationException(
                    "Charge palette requires installed colors."))
                    .ApplyCharge(cgram, pseudoScrew, suitOffset / 2, paletteIndex);

                Charge.SamusChargePaletteIndex = Charge.SamusChargePaletteIndex >= 10
                    ? (ushort)0
                    : unchecked((ushort)(Charge.SamusChargePaletteIndex + 2));
                LastBeamChargePaletteStep = new(
                    pseudoScrew
                        ? SamusBeamChargePaletteAction.PseudoScrewCycle
                        : SamusBeamChargePaletteAction.ChargeCycle);
                return LastBeamChargePaletteStep;
            }

            // `$91:D7B0` resets the sequence whenever charge is below threshold or grapple
            // owns its function pointer. This guarantees the next eligible call starts at
            // palette zero rather than resuming a partially completed cycle.
            Charge.SamusChargePaletteIndex = 0;
            LastVisorPaletteStep = samus.VisorPalette.Update(
                cgram,
                samus.Xray.SpecialPaletteKind,
                layerBlendingDefaultConfig);
            LastBeamChargePaletteStep = new(
                SamusBeamChargePaletteAction.Inactive);
            return LastBeamChargePaletteStep;
        }

        if (!samus.HyperBeamEnabled)
        {
            // `$91:D799` decrements before its BEQ. Starting at four therefore produces
            // white on timers 3, 2, and 1. Timer 1 -> 0 takes carry-set `$D7AE`, causing
            // outer `$D717` to restore the complete suit palette on the fourth call.
            Charge.ChargedShotGlowTimer = Charge.ChargedShotGlowTimer.Decrement().Remaining;
            if (Charge.ChargedShotGlowTimer.IsZero)
            {
                ushort normalPointer = ProjectileAnimation.LoadNormalSuitPalette(cgram, samus);
                LastBeamChargePaletteStep = new(
                    SamusBeamChargePaletteAction.RestoredNormalSuit);
                return LastBeamChargePaletteStep;
            }

            // Native starts at `Palettes_SpriteP4C1 + $1C` and walks backward by words.
            // That is exactly colors 15..1: transparent color zero is intentionally left
            // untouched while every visible Samus color becomes yellow `$03FF`.
            for (int color = 1; color < 16; color++)
                new SamusPalettes(cgram).Suit.Write(color, new Bgr555(31, 31, 0));

            LastBeamChargePaletteStep = new(
                SamusBeamChargePaletteAction.OrdinaryWhite);
            return LastBeamChargePaletteStep;
        }

        // Hyper's sign bit selects this branch but is not part of the table index. Odd
        // values are pure hold calls; even low-five-bit values 20..2 select palette 0..9.
        // Each selected ROM palette consequently remains visible for exactly two calls.
        if ((Charge.ChargedShotGlowTimer.RawValue & 1) != 0)
        {
            Charge.ChargedShotGlowTimer = Charge.ChargedShotGlowTimer.Decrement().Remaining;
            LastBeamChargePaletteStep = new(
                SamusBeamChargePaletteAction.HyperHold);
            return LastBeamChargePaletteStep;
        }

        ushort tableOffset = unchecked((ushort)(Charge.ChargedShotGlowTimer.RawValue & 30));
        if (tableOffset == 0)
        {
            // `$91:D7C1` branches before DEC at `$D7CB`. Native explicitly zeroes the
            // already-`$8000` timer and returns carry so `$D717` performs the suit copy.
            Charge.ChargedShotGlowTimer = NativeCountdown.Start(0);
            ushort normalPointer = ProjectileAnimation.LoadNormalSuitPalette(cgram, samus);
            LastBeamChargePaletteStep = new(
                SamusBeamChargePaletteAction.RestoredNormalSuit);
            return LastBeamChargePaletteStep;
        }

        ushort hyperPointer = 0;
        bool cataloguedHyper = samus.Presentation.ChargeColors is not null &&
            SamusChargePaletteDefinitions.TryHyperShot(tableOffset, out hyperPointer);
        if (!cataloguedHyper)
            throw new InvalidDataException($"Hyper Beam palette offset {tableOffset} is not compiled.");
        int hyperPaletteIndex = (20 - tableOffset) / 2;
        (samus.Presentation.ChargeColors ?? throw new InvalidOperationException(
            "Hyper Beam palette requires installed colors."))
            .ApplyHyper(cgram, hyperPaletteIndex);
        Charge.ChargedShotGlowTimer = Charge.ChargedShotGlowTimer.Decrement().Remaining;
        LastBeamChargePaletteStep = new(
            SamusBeamChargePaletteAction.HyperPalette);
        return LastBeamChargePaletteStep;
    }

    /// <summary>
    /// Runs the power-beam HUD producer and then handles all allocated ordinary slots.
    /// </summary>
    /// <param name="terrain">SamusTerrain</param>
    /// <param name="samus">Mutable player owner supplying pose, equipment, charge, and HUD selection.</param>
    /// <param name="buttons">The controller words Samus's projectile producer reads: held buttons, this frame's new presses, and the previous frame's new presses (#633).</param>
    /// <param name="layer1">Horizontal and vertical camera origin in whole room pixels, used by projectile offscreen handling.</param>
    /// <param name="sharedProjectiles">
    /// Bomb-slot owner carrying shared WRAM cooldown <c>$0CCC</c>. The runtime steps that
    /// clock once, immediately before calling this method; this method may replace it after
    /// a successful shot just as bank $90 does.
    /// </param>
    /// <param name="projectileProducerEnabled">Whether to admit the HUD weapon producer; existing ordinary slots still run when this is false, subject to live X-ray freeze.</param>
    /// <param name="producerSoundSuppressed">Optional captured sound-suppression decision at production; null uses the shared Power Bomb owner's current active state.</param>
    public SamusProjectileFrameResult StepFrame(SamusTerrain terrain, SamusState samus, ProjectileButtonInput buttons, Position16<RoomPixels> layer1, SamusBombProjectileSystem sharedProjectiles, bool projectileProducerEnabled = true, bool? producerSoundSuppressed = null)
    {

        Ensure.NotNull(terrain.Level);
        Ensure.NotNull(samus);
        Ensure.NotNull(sharedProjectiles);

        int? firedSlot = null;
        SoundEffectId? queuedSound = null;
        int queuedSoundMaximum = 0;
        bool soundSuppressedAtProduction = producerSoundSuppressed ?? sharedProjectiles.PowerBombExplosion.IsActive;

        // Humanoid projectile dispatch is selected by the live HUD item. Indices zero/three
        // share `$90:B80D`; index one reaches `$90:BE62`'s missile producer. Ball poses still
        // dispatch to the companion bomb owner, and the debug grapple route disables this
        // producer explicitly while it owns Shoot.
        // $90:DCE0-DCE8 branches straight to Handle_Projectiles for either
        // forward-facing pose. In particular an elevator may keep accepting physical
        // controller samples without allowing them to charge or fire a weapon.
        // Spin, wall-jump, hurt and special entries dispatch HudSelectionHandler_JumpEtc, not
        // the normal beam producer. With grapple inactive that handler does
        // nothing: held/released Shoot must leave the charge word untouched.
        // Existing projectiles still advance below. Pose input can leave spin
        // first, making the normal producer eligible on the following alpha.
        SamusHudHandler hudHandler = SamusHudDefinitions.MovementHandler(samus.PoseMetadata.ReadMovementType());
        bool preservesJumpHandlerCharge = samus.Grapple.Phase == GrapplePhase.Inactive &&
            hudHandler == SamusHudHandler.Jump;
        // Turning HUD dispatch waits for an explicit pose-initializer muzzle handoff.
        // With no handoff it neither increments charge nor releases it on a Shoot edge.
        bool preservesTurnCharge = hudHandler == SamusHudHandler.Turning &&
            samus.PoseTransitionShotDirection == 0;
        // Posture-transition dispatch preserves the carried charge during morph art;
        // ordinary crouch/stand transitions still run their native beam producer.
        bool preservesPostureCharge = hudHandler == SamusHudHandler.Transition &&
            !SamusHudInput.PostureTransitionAdmitsWeapons(samus.Pose,
                grappleActive: samus.Grapple.Phase != GrapplePhase.Inactive);
        if (projectileProducerEnabled && !preservesJumpHandlerCharge && !preservesTurnCharge && !preservesPostureCharge && !SamusPoseMetadata.IsForwardFacingPose(samus.Pose) &&
            !SamusPoseMetadata.IsStableBallPose(samus.Pose))
        {
            // $90:DDC8 calls the normal beam handler when selected X-ray's Run
            // action is released. Holding Run owns this dispatch even if setup is
            // rejected; selection alone must not consume Shoot. Inputs are normalized
            // by the runtime, so the canonical Run bit also honors remapped bindings.
            bool xrayBeamFallback = samus.SelectedHudItem == HudItem.XRay &&
                (buttons.ControllerInput & SnesButton.B) == 0;
            if (samus.SelectedHudItem is HudItem.Beam or HudItem.PowerBomb || xrayBeamFallback)
            {
                (firedSlot, queuedSound, queuedSoundMaximum) = Firing.HandleBeamInput(
                    terrain.Level,
                    samus,
                    buttons.ControllerInput,
                    // FireUnchargedBeam tests both current and prior new-input latches.
                    buttons.ControllerNewInput | buttons.ControllerPreviousNewInput,
                    sharedProjectiles,
                    terrain.Plms);
            }
            else if (samus.SelectedHudItem is HudItem.Missile or HudItem.SuperMissile)
            {
                (firedSlot, queuedSound) = Firing.TryFireMissile(
                    samus,
                    buttons.ControllerNewInput,
                    buttons.ControllerPreviousNewInput,
                    sharedProjectiles);
                if (queuedSound is not null)
                    queuedSoundMaximum = 6;
            }
        }

        // FireUnchargedBeam/FireChargedBeam perform one zero-speed block dispatch before
        // installing ordinary movement. A muzzle born against a solid door can therefore
        // already be an explosion before the descending alpha-pass loop begins below.
        bool collisionStartedExplosion = firedSlot is { } initialCollisionSlot &&
            SlotTable._slots[initialCollisionSlot].PackedType.IsFamily(
                SamusProjectileFamily.BeamExplosion);
        bool projectileDeleted = false;
        bool persistentMemoryCorrupted = false;
        List<SamusSoundRequest>? comboSounds = null;

        // `$90:AECE` walks the complete ten-slot arrays from byte index $12 down to zero.
        // Bombs were handled by the companion class first; preserve the ordinary half's
        // descending $08->$00 order here because instruction deletion changes the counter.
        // The HUD producer can activate X-ray during this alpha pass. Native checks
        // the newly published freeze flag before HandleProjectile, not only at entry.
        for (int slotIndex = SlotCount - 1; !samus.Xray.TimeIsFrozen && slotIndex >= 0; slotIndex--)
        {
            SamusProjectileSlot slot = SlotTable._slots[slotIndex];
            if (slot.InstructionPointer == 0)
                continue;

            if (slot.PreInstruction == SamusProjectilePreInstruction.ShinesparkEcho)
            {
                Shinespark.StepShinesparkEcho(samus, slot, layer1);
                projectileDeleted |= !slot.IsActive;
            }
            else if (slot.PreInstruction is SamusProjectilePreInstruction.IceCombo or
                SamusProjectilePreInstruction.IceComboOutward or SamusProjectilePreInstruction.WaveCombo or
                SamusProjectilePreInstruction.SpazerCombo or SamusProjectilePreInstruction.SpazerComboFalling or
                SamusProjectilePreInstruction.PlasmaCombo)
            {
                ushort comboSound = 0;
                if (slot.PreInstruction is SamusProjectilePreInstruction.IceCombo or SamusProjectilePreInstruction.IceComboOutward)
                    comboSound = Combos.StepIceCombo(samus, slot, sharedProjectiles, layer1);
                else if (slot.PreInstruction == SamusProjectilePreInstruction.WaveCombo)
                    comboSound = Combos.StepWaveCombo(samus, slot, sharedProjectiles);
                else if (slot.PreInstruction == SamusProjectilePreInstruction.PlasmaCombo)
                    Combos.StepPlasmaCombo(samus, slot, sharedProjectiles, layer1);
                else comboSound = Combos.StepSpazerCombo(samus, slot, sharedProjectiles, layer1.Y);
                projectileDeleted |= !slot.IsActive;
                if (comboSound != 0)
                    (comboSounds ??= []).Add(new SamusSoundRequest(
                        SoundEffectId.FromCartridge(SoundEffectLibrary.Library1, comboSound), 6,
                        SoundSuppressed: sharedProjectiles.PowerBombExplosion.IsActive));
            }
            else if (slot.PreInstruction == SamusProjectilePreInstruction.NoWaveBeam)
            {
                collisionStartedExplosion |= Motion.RunNoWaveBeamPreInstruction(
                    terrain.Level,
                    slot,
                    layer1,
                    terrain.Plms,
                    sharedProjectiles.PowerBombExplosion);
            }
            else if (slot.PreInstruction is
                SamusProjectilePreInstruction.WaveBeamThreeFrameTrail or
                SamusProjectilePreInstruction.WaveBeamFourFrameTrail)
            {
                Motion.RunWaveBeamPreInstruction(terrain.Level, slot, layer1, terrain.Plms);
            }
            else if (slot.PreInstruction == SamusProjectilePreInstruction.HyperBeam)
            {
                Motion.RunHyperBeamPreInstruction(terrain.Level, slot, layer1, terrain.Plms);
            }
            else if (slot.PreInstruction == SamusProjectilePreInstruction.Missile)
            {
                collisionStartedExplosion |= Motion.RunMissilePreInstruction(
                    terrain.Level,
                    samus,
                    slot,
                    layer1,
                    sharedProjectiles,
                    terrain.Plms);
            }
            else if (slot.PreInstruction == SamusProjectilePreInstruction.SuperMissile)
            {
                collisionStartedExplosion |= Motion.RunSuperMissilePreInstruction(
                    terrain.Level,
                    samus,
                    slot,
                    layer1,
                    sharedProjectiles,
                    terrain.Plms);
            }
            else if (slot.PreInstruction == SamusProjectilePreInstruction.SuperMissileLink)
            {
                Motion.RunSuperMissileLinkPreInstruction(slot);
            }
            else if (slot.PreInstruction == SamusProjectilePreInstruction.SpacetimePaletteCopyTail)
            {
                persistentMemoryCorrupted |= BeamCombos.RunSpacetimePaletteCopyTail(terrain.ProgressionMirror, slot);
            }
            else if (slot.PreInstruction == SamusProjectilePreInstruction.ChainsawWindowStoreThenPowerBomb)
            {
                Chainsaw.RunChainsawWindowStoreThenPowerBombPreInstruction(
                    terrain.Level,
                    samus,
                    slot,
                    sharedProjectiles,
                    terrain.Plms);
                projectileDeleted |= !slot.IsActive;
            }
            else if (slot.PreInstruction ==
                SamusProjectilePreInstruction.ChargedChainsawLowWramExecution)
            {
                // Charged combination thirteen does not point at ROM code. Its table
                // overrun stores $0A0A, so native JSR enters the low-WRAM mirror and begins
                // decoding the cached previous-Super-Missile word as opcodes. This semantic
                // port cannot execute arbitrary mutable data as 65C816 code; fail at the
                // same callback-dispatch seam instead of silently substituting Wave motion.
                throw new NotSupportedException(
                    "Charged Chainsaw callback $90:0A0A enters mutable low WRAM at " +
                    "$7E:0A0A; arbitrary data-as-65C816 execution is not translated.");
            }
            else if (slot.PreInstruction ==
                SamusProjectilePreInstruction.MurderBeamMisalignedExecution)
            {
                // Safe left-facing Murder Beam has instruction pointer zero and never
                // enters HandleProjectile. Other directions retain an instruction list;
                // the overread callback then jumps into unrelated bank-$90 code and the
                // cartridge stops making forward progress. Keep that unsafe execution
                // explicit instead of silently substituting ordinary beam motion.
                throw new NotSupportedException(
                    "Unsafe Murder Beam callback $90:A4AA enters unrelated bank-$90 " +
                    "execution; only the cartridge-safe left-facing zero-list shot is translated.");
            }

            // Kill_Projectile replaces rather than clears a live beam. Consequently the
            // explosion's first bank-$93 record is selected in this same handler pass.
            if (slot.InstructionPointer != 0)
                projectileDeleted |= Animation.RunProjectileInstructionHandler(slot);
        }

        LastFrameResult = new SamusProjectileFrameResult(
            queuedSound,
            queuedSoundMaximum,
            comboSounds?.ToArray(),
            soundSuppressedAtProduction,
            persistentMemoryCorrupted);
        return LastFrameResult;
    }

    /// <summary>
    /// Advances <c>$90:BAFC</c>'s three charge-flare animation components as a simulation phase and reports which
    /// components this frame shows, in native order. Nothing here depends on the display.
    /// </summary>
    public ChargeFlareComponents AdvanceChargeFlare(SamusState samus)
    {
        Ensure.NotNull(samus);

        if (Charge.FlareCounter == 0)
            return default;

        if (samus.HyperBeamEnabled)
        {
            // Hyper Beam replaces the ordinary count-up flare with `$90:BAFC`'s descending
            // three-component program. Native iteration is fast spark -> slow spark -> main
            // flare (WRAM offsets four, two, zero), so preserve that reverse OAM order.
            for (int component = 2; component >= 0; component--)
            {
                NativeWordCounterStep timer = Charge._flareTimers[component].Decrement();
                Charge._flareTimers[component] = timer.Remaining;
                if (timer.IsZeroOrNegative)
                {
                    bool finalFrame = Charge._flareFrames[component] == 1;
                    Charge._flareFrames[component] = NativeWordCounter.Decrement(Charge._flareFrames[component]).Value;
                    if (finalFrame)
                    {
                        // The fast-spark component at native offset four owns the global
                        // completion signal. Other zero frames merely stop rearming.
                        if (component == 2)
                            Charge.FlareCounter = 0;
                    }
                    else
                    {
                        Charge._flareTimers[component] = NativeCountdown.Start(3);
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
            Charge._flareFrames[0] = Charge._flareFrames[1] = Charge._flareFrames[2] = 0;
            Charge._flareTimers[0] = NativeCountdown.Start(3);
            Charge._flareTimers[1] = NativeCountdown.Start(5);
            Charge._flareTimers[2] = NativeCountdown.Start(4);
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
        Position16<RoomPixels> layer1,
        ChargeFlareComponents components,
        SamusMode7Transform? mode7Transform,
        ChargeFlareArtwork artwork)
    {
        Ensure.NotNull(sprites);
        Ensure.NotNull(samus);
        foreach (int component in components.DrawOrder)
            Animation.DrawFlareComponent(sprites, samus, layer1, component, mode7Transform, artwork);
    }

    /// <summary>Draws `$93:8254`'s live ordinary-projectile subset after Samus.</summary>
    public void DrawLiveProjectiles(
        SpriteFrameBuffer sprites,
        Position16<RoomPixels> layer1,
        int nmiFrameCounter,
        ProjectileDrawArtwork artwork)
    {
        Ensure.NotNull(sprites);

        for (int slotIndex = SlotCount - 1; slotIndex >= 0; slotIndex--)
        {
            SamusProjectileSlot slot = SlotTable._slots[slotIndex];
            if (!slot.IsActive ||
                slot.PackedType.FamilyValue >= (ushort)SamusProjectileFamily.PowerBomb)
                continue;

            // `$93:8268` first exempts every charged/projectile-family word selected by
            // mask `$0F10`. Only an ordinary uncharged beam reaches the flicker branches;
            // a charged beam must remain visible on every NMI parity.
            if ((slot.Type & (SamusProjectileTypeWord.FamilyMask | SamusProjectileTypeWord.ChargedBeamMarker)) == 0)
            {
                // Native X is the even byte index `$00,$02,...,$08`, so native `X & 2`
                // is exactly the parity of this host slot index. Spazer/plasma combinations
                // (low mask `$0C`) use NMI bit one and the opposite phase from power/ice/
                // wave, which use NMI bit zero. Keeping these branches separate is what
                // prevents the wider beam art from disappearing on the wrong video field.
                bool oddNativeSlot = (slotIndex & 1) != 0;
                bool skip = new SamusProjectileTypeWord(slot.Type).HasAnyBeam(SamusBeamFlags.Spazer | SamusBeamFlags.Plasma)
                    ? oddNativeSlot != ((nmiFrameCounter & 2) != 0)
                    : oddNativeSlot == ((nmiFrameCounter & 1) != 0);
                if (skip)
                    continue;
            }

            // Unlike the bomb/explosion pass, native live projectile drawing does not
            // cull by world X. OAM retains the nine-bit X and lets the PPU clip it.
            // This also preserves sprite ordering/capacity when a combo orbits offscreen.
            if (unchecked((short)slot.SpritemapPointer) < 0)
                ProjectileAnimation.DrawSlot(sprites, new SpriteSource(SpriteSourceKind.SamusProjectile, slotIndex),
                    slot, layer1, horizontalMargin: null, artwork);
        }
    }

    /// <summary>
    /// Advances <c>$90:B6A9</c>'s eighteen projectile-trail slots as a simulation phase, after the ordinary
    /// projectile pass. Each side owns its own timer, instruction pointer and position.
    /// </summary>
    /// <param name="timeIsFrozen">
    /// WRAM <c>$0A78</c>. Frozen trails retain their current record and are still drawn;
    /// their timers and embedded position commands do not advance.
    /// </param>
    /// <param name="artwork">Installed trail frame artwork; required by a trail side when it resolves a timed display record.</param>
    public void AdvanceTrails(
        bool timeIsFrozen,
        SuperMetroid.Core.Samus.Artwork.ProjectileTrailCatalog? artwork = null)
    {

        // Native Y starts at byte index $22, so the most recently preferred allocation slot
        // is also handled first, left side before right.
        for (int slotIndex = TrailSlotCount - 1; slotIndex >= 0; slotIndex--)
        {
            SamusProjectileTrailSlot slot = SlotTable._trailSlots[slotIndex];
            ProjectileTrails.AdvanceTrailSide(slot, timeIsFrozen, isLeft: true, artwork);
            ProjectileTrails.AdvanceTrailSide(slot, timeIsFrozen, isLeft: false, artwork);
        }
    }

    /// <summary>Publishes every live trail side, in the advance's slot and side order.</summary>
    /// <param name="sprites">The frame's sprite description.</param>
    /// <param name="layer1">Whole-pixel room camera origin subtracted from trail coordinates.</param>
    /// <param name="artwork">Installed trail frame artwork.</param>
    /// <param name="objects">The projectile OBJ page the trail characters are bound in.</param>
    public void DrawTrails(
        SpriteFrameBuffer sprites,
        Position16<RoomPixels> layer1,
        SuperMetroid.Core.Samus.Artwork.ProjectileTrailCatalog? artwork,
        ProjectileDrawArtwork objects)
    {
        Ensure.NotNull(sprites);
        for (int slotIndex = TrailSlotCount - 1; slotIndex >= 0; slotIndex--)
        {
            SamusProjectileTrailSlot slot = SlotTable._trailSlots[slotIndex];
            ProjectileTrails.DrawTrailSide(sprites, slot, slotIndex, layer1, isLeft: true, artwork, objects);
            ProjectileTrails.DrawTrailSide(sprites, slot, slotIndex, layer1, isLeft: false, artwork, objects);
        }
    }

    /// <summary>
    /// Draws beam explosions in bank-$A0's earlier bomb/explosion phase.
    /// </summary>
    public void DrawExplosions(
        SpriteFrameBuffer sprites,
        Position16<RoomPixels> layer1,
        ProjectileDrawArtwork artwork)
    {
        Ensure.NotNull(sprites);

        for (int slotIndex = SlotCount - 1; slotIndex >= 0; slotIndex--)
        {
            SamusProjectileSlot slot = SlotTable._slots[slotIndex];
            SamusProjectileFamily family = slot.PackedType.Family;
            if (!slot.IsActive || family is not (
                    SamusProjectileFamily.BeamExplosion or
                    SamusProjectileFamily.MissileExplosion))
                continue;

            ProjectileAnimation.DrawSlot(sprites, new SpriteSource(SpriteSourceKind.SamusProjectile, slotIndex),
                slot, layer1, horizontalMargin: 48, artwork);
        }
    }

    /// <summary>
    /// Fully clears projectile storage and held-charge presentation for session resets
    /// and forced owners such as elevator boarding. Ordinary room transitions must use
    /// <see cref="ResetForRoomTransition"/> because native projectile deletion preserves charge.
    /// </summary>
    public void Reset(SamusState? samus = null)
    {
        ResetForRoomTransition(samus);
        Charge.FlareCounter = 0;
        Charge.PreviousBeamChargeCounter = 0;
        Charge.ChargedShotGlowTimer = NativeCountdown.Start(0);
        Array.Clear(Charge._flareFrames);
        Array.Clear(Charge._flareTimers);
        LastBeamChargePaletteStep = default;
        LastVisorPaletteStep = default;
        Charge.SamusChargePaletteIndex = 0;
    }

    /// <summary>
    /// Clears departing projectile storage without cancelling held beam charge.
    /// Native ResetProjectileData ($90:AD22) does not write the flare counter,
    /// flare animation words or charge palette state. Full session/elevator reset
    /// callers still use <see cref="Reset"/> when those owners must also be cleared.
    /// </summary>
    public void ResetForRoomTransition(SamusState? samus = null)
    {
        if (samus is not null)
        {
            samus.HorizontalSpeed.ResetSpeedEchoPositionsForRoomTransition();
            samus.Shinespark.ResetProjectileEchoStorage();
        }
        Combos.ComboState = 0;
        foreach (SamusProjectileSlot slot in SlotTable._slots)
            slot.ClearForProjectileDataReset();
        foreach (SamusProjectileTrailSlot trail in SlotTable._trailSlots)
            trail.ClearFields();
        SlotTable.ProjectileCounter = 0;
        Firing.ProjectileInvincibilityTimer = NativeCountdown.Start(0);
        Earthquakes.EarthquakeType = 0;
        Earthquakes.EarthquakeTimer = 0;
        LastFrameResult = default;
    }

}
