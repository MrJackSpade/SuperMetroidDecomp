using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

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
    /// <summary>The <see cref="BeamChargeState"/> owner of this SamusProjectileSystem.</summary>
    internal BeamChargeState Charge { get; }

    /// <summary>The <see cref="ProjectileSlotTable"/> owner of this SamusProjectileSystem.</summary>
    internal ProjectileSlotTable SlotTable { get; }

    /// <summary>The <see cref="ProjectileEarthquakes"/> owner of this SamusProjectileSystem.</summary>
    internal ProjectileEarthquakes Earthquakes { get; }

    /// <summary>The <see cref="ChainsawWindow"/> owner of this SamusProjectileSystem.</summary>
    internal ChainsawWindow Chainsaw { get; }

    /// <summary>The <see cref="ProjectileImpactAudio"/> owner of this SamusProjectileSystem.</summary>
    internal ProjectileImpactAudio ImpactAudio { get; }

    /// <summary>The <see cref="ShinesparkEchoProjectile"/> owner of this SamusProjectileSystem.</summary>
    internal ShinesparkEchoProjectile Shinespark { get; }

    /// <summary>The <see cref="BeamCombos"/> owner of this SamusProjectileSystem.</summary>
    internal BeamCombos Combos { get; }

    /// <summary>The <see cref="ProjectileAnimation"/> owner of this SamusProjectileSystem.</summary>
    internal ProjectileAnimation Animation { get; }

    /// <summary>The <see cref="BeamFiring"/> owner of this SamusProjectileSystem.</summary>
    internal BeamFiring Firing { get; }

    /// <summary>The <see cref="ProjectileMotion"/> owner of this SamusProjectileSystem.</summary>
    internal ProjectileMotion Motion { get; }

    /// <summary>The <see cref="ProjectileTerrainCollision"/> owner of this SamusProjectileSystem.</summary>
    internal ProjectileTerrainCollision TerrainCollision { get; }

    /// <summary>The <see cref="ProjectileEnemyImpacts"/> owner of this SamusProjectileSystem.</summary>
    internal ProjectileEnemyImpacts EnemyImpacts { get; }

    /// <summary>The <see cref="ProjectileTrails"/> owner of this SamusProjectileSystem.</summary>
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
    /// Newly allocated projectile before its first pre-instruction, or null when this frame
    /// did not fire. This deliberately survives same-frame collision/window deletion.
    /// </summary>
    public SamusProjectileSpawnSnapshot? LastFiredProjectileSnapshot { get; private set; }

    /// <summary>
    /// Executes <c>Update_Beam_Tiles_and_Palette</c> at $90:AC8D for the equipped beam.
    /// </summary>
    /// <remarks>
    /// The 256 tile bytes are DMAed from bank $9A to VRAM word $6300. Sixteen palette
    /// words are copied from bank $90 into sprite palette six, CGRAM colors $E0-$EF.
    /// Projectile spritemaps already reference those hardware locations; omitting this
    /// room/equipment-time upload produces valid OAM whose pixels are all transparent.
    /// </remarks>
    public static void LoadBeamTilesAndPalette(
        ISnesAddressSpace bus,
        SnesVram vram,
        SnesCgram cgram,
        ushort equippedBeams,
        Assets.BeamTileCatalog? artwork = null)
    {
        Ensure.NotNull(bus);
        Ensure.NotNull(vram);
        Ensure.NotNull(cgram);

        SamusBeamCombination beamType = SelectedCombination(equippedBeams);
        LoadBeamTiles(bus, vram, equippedBeams, artwork);

        LoadSelectedBeamPalette(bus, cgram, beamType, artwork?.Palettes);
    }

    /// <summary>
    /// $90:AC8D/$90:ACCD index their tables by the equipped-beam word's low twelve bits; an
    /// index above $F (unknown bits 4-11 set) selects no beam combination.
    /// </summary>
    private static SamusBeamCombination SelectedCombination(ushort equippedBeams) =>
        new SamusBeamLoadoutWord(equippedBeams).Combination ?? throw new ArgumentOutOfRangeException(
            nameof(equippedBeams), equippedBeams, "Equipped-beam index is outside the beam-combination tables.");

    private static void LoadSelectedBeamPalette(ISnesAddressSpace bus, SnesCgram cgram,
        SamusBeamCombination selection, Assets.BeamPaletteCatalog? palettes)
    {
        switch (selection)
        {
            case ChainsawBeamGraphicsDefinitions.Selection:
                ChainsawBeamGraphicsDefinitions.LoadPalette(bus, cgram);
                break;
            case SpacetimeBeamGraphicsDefinitions.Selection:
                SpacetimeBeamGraphicsDefinitions.LoadPalette(bus, cgram);
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
    /// <summary>Replays the tile-only half of $90:AC8D after external OBJ artwork is rebound.</summary>
    public static void LoadBeamTiles(ISnesAddressSpace bus, SnesVram vram,
        ushort equippedBeams, Assets.BeamTileCatalog? artwork = null)
    {
        Ensure.NotNull(bus);
        Ensure.NotNull(vram);
        SamusBeamCombination beamType = SelectedCombination(equippedBeams);
        Assets.BeamTileCatalog installed = artwork ?? throw new InvalidOperationException(
            "Beam tiles require installed artwork.");
        new SamusVram(vram).BeamTiles.Load(installed.Resolve(Assets.BeamTileCatalog.AssetFor(beamType)).Span);
    }

    /// <summary>
    /// Queues the tile half of $90:AC8D after the room's standard-sprite upload and applies
    /// the matching final palette to the modeled CGRAM buffer.
    /// </summary>
    public static void QueueBeamTilesAndLoadPalette(
        ISnesAddressSpace bus,
        VramWriteQueue writes,
        SnesCgram cgram,
        ushort equippedBeams,
        Assets.BeamTileCatalog? artwork = null,
        Assets.BeamPaletteCatalog? palettes = null)
    {
        Ensure.NotNull(bus);
        Ensure.NotNull(writes);
        Ensure.NotNull(cgram);

        SamusBeamCombination beamType = SelectedCombination(equippedBeams);

        // Preserve the native queue position and seven-byte tail increment. Unsupported
        // beam combinations retain physical adjacent-table reads used by glitch paths.
        _ = artwork ?? throw new InvalidOperationException("Beam tiles require installed artwork.");
        writes.EnqueueAsset(Assets.BeamTileCatalog.AssetFor(beamType),
            Assets.BeamTileAtlasDefinitions.ByteCount, SamusVram.BeamTilesDma);

        palettes ??= artwork?.Palettes;
        LoadSelectedBeamPalette(bus, cgram, beamType, palettes);
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
        ISnesAddressSpace bus,
        SnesCgram cgram,
        SamusState samus,
        LayerBlendingConfiguration layerBlendingDefaultConfig =
            LayerBlendingConfiguration.NormalGameplay)
    {
        Ensure.NotNull(bus);
        Ensure.NotNull(cgram);
        Ensure.NotNull(samus);
        LastVisorPaletteStep = default;

        ushort timerBefore = Charge.ChargedShotGlowTimer;
        if (timerBefore == 0)
        {
            // `$91:D748-$D793` admits the continuously cycling charge palette only while
            // grapple's function pointer is exactly inactive and the flare counter has
            // reached 60. The 65816 BMI is a signed subtraction test, retained literally
            // so Hyper's `$8000` sentinel follows cartridge behavior too.
            bool chargePaletteActive =
                samus.Grapple.Phase == GrapplePhase.Inactive &&
                Charge.FlareCounter != 0 &&
                unchecked((short)(Charge.FlareCounter - 0x003c)) >= 0;
            if (chargePaletteActive)
            {
                bool pseudoScrew = samus.HorizontalSpeed.ContactDamageIndex == SamusContactDamageIndex.PseudoScrew;
                ushort suitOffset = ProjectileAnimation.GetSuitPaletteOffset(samus.EquippedItems);

                // The installed path compiles the two pointer levels for bounded native
                // offsets. Restored non-catalog offsets still execute both indirect reads.
                ushort palettePointer = 0;
                bool catalogued = samus.Presentation.ChargeColors is not null &&
                    (pseudoScrew
                        ? SamusChargePalettePointerDefinitions.TryPseudoScrew(
                            suitOffset, Charge.SamusChargePaletteIndex, out palettePointer)
                        : SamusChargePalettePointerDefinitions.TryChargedBeam(
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
                bus,
                cgram,
                samus.Xray.SpecialPaletteKind,
                layerBlendingDefaultConfig);
            LastBeamChargePaletteStep = new(
                SamusBeamChargePaletteAction.Inactive);
            return LastBeamChargePaletteStep;
        }

        if (samus.HyperBeam == 0)
        {
            // `$91:D799` decrements before its BEQ. Starting at four therefore produces
            // white on timers 3, 2, and 1. Timer 1 -> 0 takes carry-set `$D7AE`, causing
            // outer `$D717` to restore the complete suit palette on the fourth call.
            Charge.ChargedShotGlowTimer = unchecked((ushort)(Charge.ChargedShotGlowTimer - 1));
            if (Charge.ChargedShotGlowTimer == 0)
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
        if ((Charge.ChargedShotGlowTimer & 1) != 0)
        {
            Charge.ChargedShotGlowTimer = unchecked((ushort)(Charge.ChargedShotGlowTimer - 1));
            LastBeamChargePaletteStep = new(
                SamusBeamChargePaletteAction.HyperHold);
            return LastBeamChargePaletteStep;
        }

        ushort tableOffset = unchecked((ushort)(Charge.ChargedShotGlowTimer & 0x001e));
        if (tableOffset == 0)
        {
            // `$91:D7C1` branches before DEC at `$D7CB`. Native explicitly zeroes the
            // already-`$8000` timer and returns carry so `$D717` performs the suit copy.
            Charge.ChargedShotGlowTimer = 0;
            ushort normalPointer = ProjectileAnimation.LoadNormalSuitPalette(cgram, samus);
            LastBeamChargePaletteStep = new(
                SamusBeamChargePaletteAction.RestoredNormalSuit);
            return LastBeamChargePaletteStep;
        }

        ushort hyperPointer = 0;
        bool cataloguedHyper = samus.Presentation.ChargeColors is not null &&
            SamusChargePalettePointerDefinitions.TryHyperShot(tableOffset, out hyperPointer);
        if (!cataloguedHyper)
            throw new InvalidDataException($"Hyper Beam palette offset {tableOffset} is not compiled.");
        int hyperPaletteIndex = (0x14 - tableOffset) / 2;
        (samus.Presentation.ChargeColors ?? throw new InvalidOperationException(
            "Hyper Beam palette requires installed colors."))
            .ApplyHyper(cgram, hyperPaletteIndex);
        Charge.ChargedShotGlowTimer = unchecked((ushort)(Charge.ChargedShotGlowTimer - 1));
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
    /// <param name="layer1.X">Horizontal camera origin in whole room pixels, used by projectile offscreen handling.</param>
    /// <param name="layer1.Y">Vertical camera origin in whole room pixels, used by projectile offscreen handling.</param>
    /// <param name="sharedProjectiles">
    /// Bomb-slot owner carrying shared WRAM cooldown <c>$0CCC</c>. The runtime steps that
    /// clock once, immediately before calling this method; this method may replace it after
    /// a successful shot just as bank $90 does.
    /// </param>
    /// <param name="projectileProducerEnabled">Whether to admit the HUD weapon producer; existing ordinary slots still run when this is false, subject to live X-ray freeze.</param>
    /// <param name="producerSoundSuppressed">Optional captured sound-suppression decision at production; null uses the shared Power Bomb owner's current active state.</param>
    public SamusProjectileFrameResult StepFrame(SamusTerrain terrain, SamusState samus, ProjectileButtonInput buttons, Position16<RoomPixels> layer1, SamusBombProjectileSystem sharedProjectiles, bool projectileProducerEnabled = true, bool? producerSoundSuppressed = null)
    {
        Ensure.NotNull(terrain.Bus);
        Ensure.NotNull(terrain.Level);
        Ensure.NotNull(samus);
        Ensure.NotNull(sharedProjectiles);

        int? firedSlot = null;
        ushort queuedSound = 0;
        byte queuedSoundMaximum = 0;
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
        SamusHudHandler hudHandler = SamusHudDefinitions.MovementHandler(samus.PoseMetadata.ReadMovementType(terrain.Bus));
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
            bool xrayBeamFallback = samus.SelectedHudItem == SamusXrayRomData.SelectedHudItem &&
                (buttons.ControllerInput & (ushort)SnesButton.B) == 0;
            if (samus.SelectedHudItem is 0 or 3 || xrayBeamFallback)
            {
                (firedSlot, queuedSound, queuedSoundMaximum) = Firing.HandleBeamInput(
                    terrain.Bus,
                    terrain.Level,
                    samus,
                    buttons.ControllerInput,
                    // FireUnchargedBeam tests both current and prior new-input latches.
                    unchecked((ushort)(buttons.ControllerNewInput | buttons.ControllerPreviousNewInput)),
                    sharedProjectiles,
                    terrain.Plms);
            }
            else if (samus.SelectedHudItem is 1 or 2)
            {
                (firedSlot, queuedSound) = Firing.TryFireMissile(
                    terrain.Bus,
                    samus,
                    buttons.ControllerNewInput,
                    buttons.ControllerPreviousNewInput,
                    sharedProjectiles);
                if (queuedSound != 0)
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

        LastFiredProjectileSnapshot = firedSlot is { } newSlot
            ? new SamusProjectileSpawnSnapshot()
            : null;

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
                    comboSound = Combos.StepIceCombo(terrain.Bus, samus, slot, sharedProjectiles, layer1);
                else if (slot.PreInstruction == SamusProjectilePreInstruction.WaveCombo)
                    comboSound = Combos.StepWaveCombo(terrain.Bus, samus, slot, sharedProjectiles);
                else if (slot.PreInstruction == SamusProjectilePreInstruction.PlasmaCombo)
                    Combos.StepPlasmaCombo(samus, slot, sharedProjectiles, layer1);
                else comboSound = Combos.StepSpazerCombo(terrain.Bus, samus, slot, sharedProjectiles, layer1.Y);
                projectileDeleted |= !slot.IsActive;
                if (comboSound != 0)
                    (comboSounds ??= []).Add(new SamusSoundRequest(
                        SoundEffectId.FromCartridge(SoundEffectLibrary.Library1, comboSound), 6,
                        SoundSuppressed: sharedProjectiles.PowerBombExplosion.IsActive));
            }
            else if (slot.PreInstruction == SamusProjectilePreInstruction.NoWaveBeam)
            {
                collisionStartedExplosion |= Motion.RunNoWaveBeamPreInstruction(
                    terrain.Bus,
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
                Motion.RunWaveBeamPreInstruction(terrain.Bus, terrain.Level, slot, layer1, terrain.Plms);
            }
            else if (slot.PreInstruction == SamusProjectilePreInstruction.HyperBeam)
            {
                Motion.RunHyperBeamPreInstruction(terrain.Level, slot, layer1, terrain.Plms);
            }
            else if (slot.PreInstruction == SamusProjectilePreInstruction.Missile)
            {
                collisionStartedExplosion |= Motion.RunMissilePreInstruction(
                    terrain.Bus,
                    terrain.Level,
                    slot,
                    layer1,
                    sharedProjectiles,
                    terrain.Plms);
            }
            else if (slot.PreInstruction == SamusProjectilePreInstruction.SuperMissile)
            {
                collisionStartedExplosion |= Motion.RunSuperMissilePreInstruction(
                    terrain.Bus,
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
                persistentMemoryCorrupted |= BeamCombos.RunSpacetimePaletteCopyTail(terrain.Bus, slot);
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
            queuedSound == 0
                ? null
                : SoundEffectId.FromCartridge(SoundEffectLibrary.Library1, queuedSound),
            queuedSoundMaximum,
            comboSounds?.ToArray(),
            soundSuppressedAtProduction,
            persistentMemoryCorrupted);
        return LastFrameResult;
    }

    /// <summary>
    /// Runs and draws <c>$90:BAFC</c>'s three charge-flare animation components before Samus.
    /// </summary>
    public void HandleChargeFlareAndDraw(
        ISnesAddressSpace bus,
        OamBuffer oam,
        SamusState samus,
        Position16<RoomPixels> layer1,
        SamusMode7Transform? mode7Transform = null,
        Assets.ChargeFlarePlacementCatalog? placement = null,
        Assets.ChargeFlareSpriteCatalog? compositions = null)
    {
        Ensure.NotNull(bus);
        Ensure.NotNull(oam);
        Ensure.NotNull(samus);

        if (Charge.FlareCounter == 0)
            return;

        if (samus.HyperBeam != 0)
        {
            // Hyper Beam replaces the ordinary count-up flare with `$90:BAFC`'s descending
            // three-component program. Native iteration is fast spark -> slow spark -> main
            // flare (WRAM offsets four, two, zero), so preserve that reverse OAM order.
            for (int component = 2; component >= 0; component--)
            {
                Charge._flareTimers[component] = unchecked((ushort)(Charge._flareTimers[component] - 1));
                if (Charge._flareTimers[component] == 0 || (Charge._flareTimers[component] & 0x8000) != 0)
                {
                    bool finalFrame = Charge._flareFrames[component] == 1;
                    Charge._flareFrames[component] = unchecked((ushort)(Charge._flareFrames[component] - 1));
                    if (finalFrame)
                    {
                        // The fast-spark component at native offset four owns the global
                        // completion signal. Other zero frames merely stop rearming.
                        if (component == 2)
                            Charge.FlareCounter = 0;
                    }
                    else
                    {
                        Charge._flareTimers[component] = 3;
                    }
                }

                Animation.DrawFlareComponent(bus, oam, samus, layer1, component, mode7Transform, new ChargeFlareArtwork { Placement = placement, Compositions = compositions });
            }
            return;
        }

        // Calls 1..14 initialize/retain the hidden animation state. Call 15 begins drawing
        // the central flare; calls 30+ additionally draw both orbiting spark components.
        if (Charge.FlareCounter == 1)
        {
            Charge._flareFrames[0] = Charge._flareFrames[1] = Charge._flareFrames[2] = 0;
            Charge._flareTimers[0] = 3;
            Charge._flareTimers[1] = 5;
            Charge._flareTimers[2] = 4;
        }
        if (Charge.FlareCounter < 15)
            return;

        int componentCount = Charge.FlareCounter < 30 ? 1 : 3;
        for (int component = 0; component < componentCount; component++)
        {
            Animation.AdvanceFlareComponent(component);
            Animation.DrawFlareComponent(bus, oam, samus, layer1, component, mode7Transform, new ChargeFlareArtwork { Placement = placement, Compositions = compositions });
        }
    }

    /// <summary>Draws `$93:8254`'s live ordinary-projectile subset after Samus.</summary>
    public void DrawLiveProjectiles(
        ISnesAddressSpace bus,
        OamBuffer oam,
        Position16<RoomPixels> layer1,
        ushort nmiFrameCounter,
        Assets.ProjectileSpriteCatalog? compositions = null)
    {
        Ensure.NotNull(bus);
        Ensure.NotNull(oam);

        for (int slotIndex = SlotCount - 1; slotIndex >= 0; slotIndex--)
        {
            SamusProjectileSlot slot = SlotTable._slots[slotIndex];
            if (!slot.IsActive ||
                slot.PackedType.FamilyValue >= (ushort)SamusProjectileFamily.PowerBomb)
                continue;

            // `$93:8268` first exempts every charged/projectile-family word selected by
            // mask `$0F10`. Only an ordinary uncharged beam reaches the flicker branches;
            // a charged beam must remain visible on every NMI parity.
            if ((slot.Type & 0x0f10) == 0)
            {
                // Native X is the even byte index `$00,$02,...,$08`, so native `X & 2`
                // is exactly the parity of this host slot index. Spazer/plasma combinations
                // (low mask `$0C`) use NMI bit one and the opposite phase from power/ice/
                // wave, which use NMI bit zero. Keeping these branches separate is what
                // prevents the wider beam art from disappearing on the wrong video field.
                bool oddNativeSlot = (slotIndex & 1) != 0;
                bool skip = (slot.Type & 0x000c) != 0
                    ? oddNativeSlot != ((nmiFrameCounter & 2) != 0)
                    : oddNativeSlot == ((nmiFrameCounter & 1) != 0);
                if (skip)
                    continue;
            }

            // Unlike the bomb/explosion pass, native live projectile drawing does not
            // cull by world X. OAM retains the nine-bit X and lets the PPU clip it.
            // This also preserves sprite ordering/capacity when a combo orbits offscreen.
            if ((slot.SpritemapPointer & 0x8000) != 0)
                ProjectileAnimation.DrawSlot(oam, slot, layer1, horizontalMargin: null, compositions);
        }
    }

    /// <summary>
    /// Runs and draws <c>$90:B6A9</c>'s eighteen projectile-trail slots after the ordinary
    /// projectile pass. Each side owns its own timer, instruction pointer, position, and OBJ.
    /// </summary>
    /// <param name="bus">Address space passed to the trail instruction and draw helpers.</param>
    /// <param name="oam">Mutable OBJ destination appended in descending trail-slot order, left side before right.</param>
    /// <param name="layer1.X">Whole-pixel room camera X origin subtracted from trail coordinates when drawing.</param>
    /// <param name="layer1.Y">Whole-pixel room camera Y origin subtracted from trail coordinates when drawing.</param>
    /// <param name="timeIsFrozen">
    /// WRAM <c>$0A78</c>. Frozen trails retain their current record and are still drawn;
    /// their timers and embedded position commands do not advance.
    /// </param>
    /// <param name="artwork">Installed trail frame artwork; required by a trail side when it resolves a timed display record.</param>
    public void HandleTrailsAndDraw(
        ISnesAddressSpace bus,
        OamBuffer oam,
        Position16<RoomPixels> layer1,
        bool timeIsFrozen,
        Assets.ProjectileTrailCatalog? artwork = null)
    {
        Ensure.NotNull(bus);
        Ensure.NotNull(oam);

        // Native Y starts at byte index $22, so the most recently preferred allocation slot
        // is also handled first. Preserve that order because overlapping 8x8 OBJs reveal it.
        for (int slotIndex = TrailSlotCount - 1; slotIndex >= 0; slotIndex--)
        {
            SamusProjectileTrailSlot slot = SlotTable._trailSlots[slotIndex];
            ProjectileTrails.HandleTrailSideAndDraw(bus, oam, slot, layer1, timeIsFrozen, isLeft: true, artwork);
            ProjectileTrails.HandleTrailSideAndDraw(bus, oam, slot, layer1, timeIsFrozen, isLeft: false, artwork);
        }
    }

    /// <summary>
    /// Draws beam explosions in bank-$A0's earlier bomb/explosion phase.
    /// </summary>
    public void DrawExplosions(
        ISnesAddressSpace bus,
        OamBuffer oam,
        Position16<RoomPixels> layer1,
        Assets.ProjectileSpriteCatalog? compositions = null)
    {
        Ensure.NotNull(bus);
        Ensure.NotNull(oam);

        for (int slotIndex = SlotCount - 1; slotIndex >= 0; slotIndex--)
        {
            SamusProjectileSlot slot = SlotTable._slots[slotIndex];
            SamusProjectileFamily family = slot.PackedType.Family;
            if (!slot.IsActive || family is not (
                    SamusProjectileFamily.BeamExplosion or
                    SamusProjectileFamily.MissileExplosion))
                continue;

            ProjectileAnimation.DrawSlot(oam, slot, layer1, horizontalMargin: 48, compositions);
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
        Charge.ChargedShotGlowTimer = 0;
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
        Firing.ProjectileInvincibilityTimer = 0;
        Earthquakes.EarthquakeType = 0;
        Earthquakes.EarthquakeTimer = 0;
        LastFrameResult = default;
        LastFiredProjectileSnapshot = null;
    }

}
