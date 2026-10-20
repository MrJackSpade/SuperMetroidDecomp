using SuperMetroid.Core.Input;
using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Samus.Palettes;
using SuperMetroid.Core.Samus.Artwork;

namespace SuperMetroid.Core.Samus.Movement;

/// <summary>
/// The fixed-point horizontal-speed registers used by Samus's bank-$90 movement code.
/// </summary>
/// <remarks>
/// This class is intentionally narrower than a general-purpose desktop physics body. Every
/// property corresponds to a native 16-bit WRAM word, and every operation below preserves
/// the SNES routine's word-sized wrapping and comparisons. Room/enemy collision consumes
/// the resulting displacement later; it is not guessed here.
/// </remarks>
public sealed class SamusHorizontalSpeedState
{
    /// <summary>Samus's base horizontal speed, <c>samus_x_base_speed</c>/<c>subspeed</c> at WRAM <c>$0B46</c>/<c>$0B48</c>.</summary>
    public SnesUnsignedSixteenSixteen BaseSpeed { get; set; }

    /// <summary>The run-button/speed-booster addition to horizontal speed at WRAM <c>$0B42</c>/<c>$0B44</c>.</summary>
    public SnesUnsignedSixteenSixteen ExtraRunSpeed { get; set; }

    /// <summary>
    /// Native <c>samus_has_momentum_flag</c>. Once dash begins, releasing Dash or leaving
    /// movement type one retains the accumulated extra component until a specific cancel
    /// routine (standing, turning, collision, etc.) clears WRAM <c>$0B3C</c>.
    /// </summary>
    public bool HasRunningMomentum { get; set; }

    /// <summary>
    /// Speed Booster charge counter, 0 through 65535: the boost stage times 256 plus the
    /// running-cadence countdown that advances it. Stages zero through four are reached by
    /// running; No-Speed-Booster Dash keeps it zero.
    /// </summary>
    /// <remarks>
    /// The value stays in the sixteen-bit range because its wrap is load-bearing: the cadence
    /// countdown borrows from the stage when it is decremented at zero, and a restored counter
    /// can carry any stage. It changes only through the wrapping cadence decrement in
    /// <see cref="TryAdvanceSpeedBoosterAnimationStage"/> and through whole-counter writes
    /// checked against that range.
    /// </remarks>
    public SamusSpeedBoostCounter SpeedBoostCounter { get; set; }

    /// <summary>Whether the speed-boost counter's high byte is the full fourth stage.</summary>
    public bool IsFullSpeedBoost => SpeedBoostCounter.HighByteIsStageFour;

    /// <summary>
    /// Whether the counter's stage field is four, the cartridge condition used by
    /// Speed Booster block setup and active-boost contact behavior.
    /// </summary>
    public bool IsActivelySpeedBoosting => SpeedBoostCounter.IsActivelySpeedBoosting;

    /// <summary>
    /// Byte offset of the current Speed Booster or Screw Attack palette within its six-entry
    /// cycle: an even number from 0 through 10, or 12 in fixtures that read past the list.
    /// Reset when Speed Booster momentum begins or cancels.
    /// </summary>
    public int SpecialPaletteFrame { get; set; }

    /// <summary>WRAM <c>$0AD0</c>, seeded to one on the first boosted running frame.</summary>
    public NativeCountdown SpecialPaletteTimer { get; set; }

    /// <summary>
    /// One-shot event set when `$90:852C` enters stage four. Audio can consume this flag
    /// later without making the movement translation depend on a host sound backend.
    /// </summary>
    public bool EchoSoundRequested { get; set; }

    /// <summary>
    /// WRAM $0B40, SamusEchoesSFXFlag: set by $90:859D before the start request,
    /// consumed by $90:F591 after boost ends, or cleared silently by quicksand $84:B40E. 0 through 65535.
    /// </summary>
    public int EchoSoundFlag
    {
        get;
        set => field = Ensure.BetweenInclusive(0, ushort.MaxValue, value);
    }

    /// <summary>Consumes speed stage four's native <c>QueueSfx3_Max6($03)</c> call.</summary>
    public bool ConsumeEchoSoundRequest()
    {
        bool requested = EchoSoundRequested;
        EchoSoundRequested = false;
        return requested;
    }

    /// <summary>Contact-damage selector published when the counter reaches stage four.</summary>
    public SamusContactDamageIndex ContactDamageIndex { get; set; }

    /// <summary>
    /// Host-side publication of the immediate suit-palette copy performed by
    /// <c>Cancel_SpeedBoosting</c> at <c>$91:DE53</c>. Movement owns the cancellation,
    /// while the runtime owns CGRAM, so this flag preserves the native same-frame handoff
    /// without passing a renderer through every collision and pose routine.
    /// </summary>
    public bool NormalSuitPaletteRestoreRequested { get; private set; }

    /// <summary>
    /// Speed echo index, 0 through 65535. Ordinary active Speed Booster echoes alternate
    /// between slot offsets zero and two. Cancellation writes the largest index while the two
    /// stored bodies travel back toward Samus; the shinespark crash temporarily shares the
    /// same value as its orbit radius plus 256 times its orbit stage, and that residue decides
    /// which slot the next boost captures first.
    /// </summary>
    public int SpeedEchoIndex { get; private set; }

    /// <summary>
    /// The index cancellation publishes while the echoes depart: every bit of the sixteen-bit
    /// index set, the largest value it can hold.
    /// </summary>
    private const int DepartingSpeedEchoIndex = ushort.MaxValue;

    /// <summary>
    /// Whether the echoes are departing toward Samus: the index lies in the upper half of its
    /// sixteen-bit range, which only cancellation writes.
    /// </summary>
    public bool SpeedEchoesDeparting => SpeedEchoIndex > short.MaxValue;

    /// <summary>First captured Speed Booster echo position: X at WRAM <c>$0AB0</c>, Y at <c>$0AB4</c>. X zero marks the slot empty.</summary>
    public Position16<RoomPixels> FirstSpeedEchoPosition { get; private set; }

    /// <summary>Second captured Speed Booster echo position: X at WRAM <c>$0AB2</c>, Y at <c>$0AB6</c>. X zero marks the slot empty.</summary>
    public Position16<RoomPixels> SecondSpeedEchoPosition { get; private set; }

    /// <summary>
    /// Whole-pixel per-frame velocity of departing echo zero; only X moves. Cancellation
    /// writes -8 while facing left or +8 otherwise.
    /// </summary>
    public Offset16<RoomPixels> FirstSpeedEchoVelocity { get; private set; }

    /// <summary>Whole-pixel per-frame velocity of departing echo one; only X moves.</summary>
    public Offset16<RoomPixels> SecondSpeedEchoVelocity { get; private set; }

    /// <summary>The total horizontal speed produced by <c>Samus_CalcSpeed_X</c> at WRAM <c>$0DBC</c>/<c>$0DBE</c>.</summary>
    public SnesUnsignedSixteenSixteen TotalSpeed { get; internal set; }

    /// <summary>
    /// Speed divisor, 0 through 65535: total speed is halved this many times, capped at four
    /// halvings however large the divisor is. It doubles as the dry-room animation delay.
    /// </summary>
    public int SpeedDivisor
    {
        get;
        set => field = Ensure.BetweenInclusive(0, ushort.MaxValue, value);
    }

    /// <summary>
    /// Acceleration mode at WRAM <c>$0B4A</c>: accelerating speeds up, while turning and decelerating both take the
    /// deceleration branch in <c>$90:9A7E</c>.
    /// </summary>
    public SamusAccelerationMode AccelerationMode { get; set; }

    /// <summary>
    /// Optional deceleration multiplier, 0 through 255. Zero selects the table's plain
    /// deceleration; nonzero multiplies the whole deceleration and only the upper half of its
    /// fraction, as the cartridge's asymmetric products do.
    /// </summary>
    public int DecelerationMultiplier
    {
        get;
        set => field = Ensure.BetweenInclusive(0, byte.MaxValue, value);
    }

    /// <summary>
    /// The table the current inside-block/environment reaction selected. Normal air, water, and
    /// lava/acid select three complete tables before movement type is added.
    /// </summary>
    public SamusLiquidMedium ActiveSpeedTable { get; internal set; } = SamusLiquidMedium.Air;

    /// <summary>
    /// Reproduces the ordinary-air assignment made by <c>$94:97D0</c> before movement.
    /// </summary>
    public void SelectNormalAirSpeedTable() => ActiveSpeedTable = SamusLiquidMedium.Air;

    /// <summary>
    /// Ports <c>Determine_Samus_X_Speed_Table_Entry_Pointer</c>'s environmental selection
    /// at <c>$90:9BD1</c>. The selected table's records are indexed later by movement type.
    /// </summary>
    public void SelectEnvironmentSpeedTable(SamusLiquidMedium liquidMedium)
    {
        _ = SamusHorizontalMotionDefinitions.FirstRow(liquidMedium);
        ActiveSpeedTable = liquidMedium;
    }

    /// <summary>
    /// Ports the dry-air portion of <c>Handle_Samus_XExtraRunSpeed</c> at <c>$90:973E</c>.
    /// Both routes accelerate by hexadecimal <c>0.1000</c>. Ordinary Dash caps at
    /// <c>2.0000</c>; equipped Speed Booster caps at <c>7.0000</c> and initializes its
    /// counter through `$91:B61F`. Their shared momentum flag survives release and jumps.
    /// </summary>
    public void HandleExtraRunSpeed(
        SamusMovementType movementType,
        SnesButton controllerInput,
        bool speedBoosterEquipped,
        bool liquidImpeded = false)
    {
        const SnesButton dashButton = SnesButton.B;
        // `$90:9746-$9763` diverts a non-Gravity submerged body to the same no-acceleration
        // branch as releasing Dash. Existing momentum retains its numeric extra component;
        // without momentum, both words are cleared. This is not a multiplier or hard reset.
        bool activelyDashing = !liquidImpeded &&
            movementType == SamusMovementType.Running &&
            controllerInput.HasAny(dashButton);
        if (!activelyDashing)
        {
            // `$90:9808` clears the extra pair only before momentum has been established.
            // A true flag carries the pair through airborne movement and a released button.
            if (!HasRunningMomentum)
            {
                ExtraRunSpeed = default;
            }
            // The native common epilogue also runs when Dash is released,
            // movement becomes airborne, or liquid suppresses acceleration.
            PublishBoostContactDamage();
            return;
        }

        if (speedBoosterEquipped)
        {
            if (!HasRunningMomentum)
            {
                // `$90:976C-$9780` initializes the counter's authored low byte.
                // Palette writes themselves remain renderer work, but their native
                // frame/timer state is movement-visible and therefore retained here.
                HasRunningMomentum = true;
                SpecialPaletteTimer = NativeCountdown.Start(1);
                SpecialPaletteFrame = 0;
                SpeedBoostCounter = new SamusSpeedBoostCounter(SamusRunningCadenceDefinitions.ReadResetWord(0));
            }

            if (ExtraRunSpeed.WordsAtLeast(new SnesUnsignedSixteenSixteen(7, 0)))
            {
                ExtraRunSpeed = new SnesUnsignedSixteenSixteen(7, 0);
                PublishBoostContactDamage();
                return;
            }

            AddExtraRunAcceleration();
            PublishBoostContactDamage();
            return;
        }

        if (!HasRunningMomentum)
        {
            HasRunningMomentum = true;
            SpeedBoostCounter = default;
        }

        // The cartridge compares the two words separately with signed BMI branches. This
        // intentionally is not a conventional unsigned 32-bit >= comparison. The retail
        // no-booster fractional cap is zero, so a normal progression reaches 2.0000 and
        // clamps there on the following call before another 0.1000 can be added.
        if (ExtraRunSpeed.WordsAtLeast(new SnesUnsignedSixteenSixteen(2, 0)))
        {
            ExtraRunSpeed = new SnesUnsignedSixteenSixteen(2, 0);
            PublishBoostContactDamage();
            return;
        }

        AddExtraRunAcceleration();
        PublishBoostContactDamage();
    }

    /// <summary>
    /// Executes the equipped-Speed-Booster interception at `$90:852C-$856B` when running
    /// reaches an animation command. True means the command was consumed and frame zero
    /// was restarted from the ROM-authored delay list for the new boost stage.
    /// </summary>
    /// <param name="mode7Residue">The session's Mode 7 transfer queue residue, which stage five's delay list aliases.</param>
    /// <param name="movementType">Current movement category; only running can consume this animation command.</param>
    /// <param name="controllerInput">Current held SNES button word; the native dash button must be held.</param>
    /// <param name="animationFrameBuffer">Native frame-delay adjustment added to the selected stage's first animation delay.</param>
    /// <param name="animationFrame">Current animation frame, reset to zero only when this call consumes the command.</param>
    /// <param name="animationFrameTimer">Zero when the command is not consumed; otherwise the new first-frame delay plus the buffer, wrapped to sixteen bits.</param>
    /// <param name="queueEchoSound">
    /// Synchronous native sound call, returning its accumulator after queue mutation.
    /// Supplying it reproduces the cartridge's queue-dependent table selection and
    /// replaces deferred sound publication. Null retains the legacy deferred path;
    /// standalone callers can omit it when no cartridge audio queue is attached.
    /// </param>
    public bool TryAdvanceSpeedBoosterAnimationStage(
        SuperMetroid.Core.Bosses.Ridley.Mode7TransferQueueResidue? mode7Residue,
        SamusMovementType movementType,
        SnesButton controllerInput,
        int animationFrameBuffer,
        ref ushort animationFrame,
        out ushort animationFrameTimer,
        Func<ushort>? queueEchoSound = null)
    {
        animationFrameTimer = 0;
        const SnesButton dashButton = SnesButton.B;
        SnesButton heldButtons = controllerInput;
        if (!HasRunningMomentum || movementType != SamusMovementType.Running || !heldButtons.HasAny(dashButton))
            return false;

        // The decrement wraps across the whole sixteen-bit counter, so a cadence countdown
        // already at zero borrows from the stage. A stage advances only when the decremented
        // cadence countdown is zero; that test leaves the stage unchanged.
        (SamusSpeedBoostCounter decremented, bool cadenceExpired) = SpeedBoostCounter.DecrementForAnimationCommand();
        SpeedBoostCounter = decremented;
        if (!cadenceExpired)
            return false;

        int tableSelection = SpeedBoostCounter.RawValue;
        if (!SpeedBoostCounter.HasActiveStageBit)
        {
            // Without the fourth-stage bit the counter is below 64512, so one more stage
            // cannot leave the sixteen-bit range.
            SpeedBoostCounter = SpeedBoostCounter.AdvanceStage();
            tableSelection = SpeedBoostCounter.RawValue;
            if (SpeedBoostCounter.HasActiveStageBit)
            {
                EchoSoundFlag = 1;
                if (queueEchoSound is null)
                    EchoSoundRequested = true;
                else
                {
                    // The cartridge does not preserve A across its sound call.
                    // Its high byte selects BOTH reset tables, independently of
                    // the stage word already stored above. Do not clamp this
                    // index: queue occupancy five reads adjacent native data.
                    tableSelection = queueEchoSound();
                }
            }
        }

        byte stage = SnesLittleEndian.HighByte(tableSelection);
        ushort nextCadence = SamusRunningCadenceDefinitions.ReadResetWord(stage);
        SpeedBoostCounter = SpeedBoostCounter.WithCadenceWord(nextCadence);

        ushort delayList = SamusRunningCadenceDefinitions.ReadSpeedBoostDelayListPointer(stage);
        animationFrame = 0;
        animationFrameTimer = unchecked((ushort)(
            animationFrameBuffer + SamusRunningCadenceDefinitions.ReadAnimationByte(mode7Residue, delayList, 0)));
        // Animation runs after movement. Native stage advancement changes cadence
        // and sound, not contact damage; the next movement epilogue publishes that.
        // Publishing here enables boost damage one frame before the cartridge.
        return true;
    }

    /// <summary>Reads one byte from the delay list selected by the counter's stage byte.</summary>
    public byte ReadSpeedBoosterAnimationByte(SuperMetroid.Core.Bosses.Ridley.Mode7TransferQueueResidue? mode7Residue, int byteIndex)
    {
        byte stage = checked((byte)SpeedBoostCounter.Stage);
        ushort delayList = SamusRunningCadenceDefinitions.ReadSpeedBoostDelayListPointer(stage);
        return SamusRunningCadenceDefinitions.ReadAnimationByte(mode7Residue, delayList, byteIndex);
    }

    /// <summary>
    /// Ports the dry-room Speed Booster branch of
    /// <c>Handle_ScrewAttack_SpeedBoosting_Palette</c> at <c>$91:D9B2</c>.
    /// </summary>
    /// <returns>True when this call copied a ROM-authored palette into CGRAM.</returns>
    public bool UpdateSpeedBoosterPalette(IPaletteColors cgram, SpeedBoosterPaletteFrame frame, SamusSuitColorCatalog? suitColors = null, SamusFullBodyCycleColorCatalog? cycleColors = null,
        SamusOverreadCatalog? overreads = null)
    {
        Ensure.NotNull(cgram);

        bool paletteCopied = ApplyPendingNormalSuitPaletteRestore(cgram, frame.EquippedItems, suitColors);
        ushort suitTableOffset = frame.EquippedItems.GetSuitPaletteTableOffset();

        // Stored-shine and shinespark handlers have priority in `$91:D6F7`. Cancellation's
        // immediate normal-palette copy above still occurs, but handler zero must not advance
        // its hidden four-frame Speed Booster cycle beneath handler one or six.
        if (frame.SuppressActiveSpeedBoosterPalette)
            return paletteCopied;

        // `$91:D9B2-$D9D8` tests the suit-palette Gravity bit before it asks whether
        // Samus's *bottom* boundary lies under water or lava/acid. Power and Varia Samus
        // return carry-set immediately when submerged: the current CGRAM colors, common
        // timer, and palette-list offset all remain untouched. Gravity Suit bypasses this
        // gate and continues into the ordinary Screw Attack / Speed Booster logic even
        // though the raw room-FX comparison still says the body is underwater. The caller
        // supplies the raw boundary result so this state owner does not invent a second,
        // subtly different interpretation of the room's FX words.
        bool gravitySuitEquipped = frame.EquippedItems.HasAny(SamusEquipmentFlags.GravitySuit);
        if (!gravitySuitEquipped && frame.BottomBoundarySubmerged)
            return paletteCopied;

        bool screwAttackEquipped = frame.EquippedItems.HasAny(SamusEquipmentFlags.ScrewAttack);
        if (frame.MovementType == SamusMovementType.SpinJumping && screwAttackEquipped)
        {
            if (frame.AnimationFrame == 0)
            {
                // `$91:DA43` resets only the palette-list byte offset. Frame zero retains
                // the already loaded normal suit colors; it does not emit a Screw palette.
                SpecialPaletteFrame = 0;
                return paletteCopied;
            }

            if (unchecked((short)(frame.AnimationFrame - 27)) >= 0)
            {
                // Carry clear at `$91:DA48` asks the outer palette dispatcher to copy
                // the normal suit palette for frames 27 onward. Frames 1..26 cycle below.
                // CGRAM is the desktop runtime's directly visible palette buffer.
                LoadNormalSuitPalette(cgram, frame.EquippedItems, suitColors);
                return true;
            }

            CopyAndAdvanceScrewAttackPalette(cgram, suitTableOffset, cycleColors);
            return true;
        }

        if (frame.MovementType == SamusMovementType.WallJumping)
        {
            if (!screwAttackEquipped)
                return paletteCopied;

            if (frame.AnimationFrame < 3)
            {
                // Wall-jump command `$FB` reserves frames 0..2 for the normal suit colors;
                // its Screw family begins on frame three and shares the same six pointers.
                SpecialPaletteFrame = 0;
                return paletteCopied;
            }

            CopyAndAdvanceScrewAttackPalette(cgram, suitTableOffset, cycleColors);
            return true;
        }

        if (!IsFullSpeedBoost)
            return paletteCopied;

        // DEC is a full 16-bit operation. BEQ/BPL mean zero and signed underflow both reload
        // four, though ordinary execution arrives with timer one and never underflows.
        NativeWordCounterStep timer = SpecialPaletteTimer.Decrement();
        SpecialPaletteTimer = timer.Remaining;
        if (!timer.IsZero && timer.IsNonNegative)
            return paletteCopied;

        SpecialPaletteTimer = NativeCountdown.Start(4);

        ushort palettePointer = SamusPaletteRomData.FullBodyCycles.ReadActiveSpeedBoosterPalettePointer(
            suitTableOffset, checked((ushort)SpecialPaletteFrame));
        SamusSpeedBoosterPalette.Apply(cgram, palettePointer, cycleColors, overreads);

        // The native clamp is AFTER the copy. Screw Attack can leave phase eight or ten
        // in this shared word; retain that one bounded overrun before pinning phase six.
        SpecialPaletteFrame = SpecialPaletteFrame >= 6
            ? 6
            : SpecialPaletteFrame + 2;
        return true;
    }

    /// <summary>
    /// Defers the normal 16-color suit copy until before the runtime's palette handlers.
    /// Native pose initialization performs this when leaving a Screw Attack spin family.
    /// </summary>
    public void RequestNormalSuitPaletteRestore() => NormalSuitPaletteRestoreRequested = true;

    /// <summary>
    /// Publishes the immediate suit copies requested by native momentum cancellation and
    /// pose initialization. These precede charge and special-palette handling, which may
    /// replace their colors in the same frame; they must not erase a later charge flash.
    /// </summary>
    public bool ApplyPendingNormalSuitPaletteRestore(
        IPaletteColors cgram, SuperMetroid.Core.Samus.SamusEquipmentFlags equippedItems,
        SamusSuitColorCatalog? suitColors = null)
    {
        Ensure.NotNull(cgram);
        if (!NormalSuitPaletteRestoreRequested)
            return false;

        SamusNormalSuitPalette.Load(cgram, equippedItems, suitColors);
        NormalSuitPaletteRestoreRequested = false;
        return true;
    }

    /// <summary>Copies one of the six ROM-authored Screw Attack palettes and wraps its offset.</summary>
    private void CopyAndAdvanceScrewAttackPalette(
        IPaletteColors cgram,
        ushort suitTableOffset,
        SamusFullBodyCycleColorCatalog? cycleColors)
    {
        ushort palettePointer = SamusPaletteRomData.FullBodyCycles.ReadScrewAttackPalettePointer(
            suitTableOffset, checked((ushort)SpecialPaletteFrame));
        (cycleColors ?? throw new InvalidOperationException(
            "Screw Attack palette requires installed Samus full-body cycle colors."))
            .Apply(cgram, palettePointer);

        // Offsets 0,2,4,6,8,10 form the six-frame cycle. The native CMP uses the current
        // offset, so ten wraps to zero only after its palette has been copied.
        SpecialPaletteFrame = SpecialPaletteFrame >= 10
            ? 0
            : SpecialPaletteFrame + 2;
    }

    /// <summary>Loads the normal Power/Varia/Gravity palette selected by the native suit index.</summary>
    private static void LoadNormalSuitPalette(
        IPaletteColors cgram,
        SuperMetroid.Core.Samus.SamusEquipmentFlags equippedItems,
        SamusSuitColorCatalog? suitColors)
    {
        SamusNormalSuitPalette.Load(cgram, equippedItems, suitColors);
    }

    /// <summary>
    /// Ports <c>Samus_UpdateSpeedEchoPos</c> at <c>$90:EEE7</c> for ordinary active boost.
    /// The gameplay clock, not NMI count, selects the four-frame sampling phase.
    /// </summary>
    /// <param name="gameTimeFrames">The gameplay clock's frame count, 0 to 65535; only its four-frame phase is read and nothing stores it.</param>
    /// <param name="position">Samus's whole-pixel room position sampled into the next echo slot.</param>
    public bool CaptureSpeedEchoPosition(
        int gameTimeFrames,
        Position16<RoomPixels> position)
    {
        Ensure.BetweenInclusive(0, ushort.MaxValue, gameTimeFrames);
        if (!IsFullSpeedBoost ||
            SpeedEchoesDeparting ||
            (gameTimeFrames & 3) != 0)
        {
            return false;
        }

        if (SpeedEchoIndex == 0)
            FirstSpeedEchoPosition = position;
        else
            SecondSpeedEchoPosition = position;

        // A capturing index is at most 32767 here, so stepping one slot stays in range.
        SpeedEchoIndex += 2;
        if (SpeedEchoIndex >= 4)
            SpeedEchoIndex = 0;
        return true;
    }

    /// <summary>
    /// Applies the observable echo-slot clears shared by shinespark windup at
    /// <c>$90:D068</c> and directional launch setup at <c>$91:F83E</c>.
    /// </summary>
    /// <remarks>
    /// Native also clears two later crash-circle velocity bytes. Those bytes are not part
    /// of the ordinary active-echo model yet; the X position words and alternating index
    /// below are the exact subset consumed by <see cref="CaptureSpeedEchoPosition"/> and
    /// the current renderer.
    /// </remarks>
    public void ResetSpeedEchoPositionsForShinespark()
    {
        SpeedEchoIndex = 0;
        FirstSpeedEchoVelocity = Offset16<RoomPixels>.Zero;
        SecondSpeedEchoVelocity = Offset16<RoomPixels>.Zero;
        FirstSpeedEchoPosition = FirstSpeedEchoPosition.WithX(0);
        SecondSpeedEchoPosition = SecondSpeedEchoPosition.WithX(0);
        // Launch marks slots empty through X alone. Native leaves the old Y words
        // intact until the next gameplay-clock sample overwrites them.
    }

    /// <summary>Clears active echo storage during native ResetProjectileData ($90:AD22).</summary>
    public void ResetSpeedEchoPositionsForRoomTransition()
    {
        ResetSpeedEchoPositionsForShinespark();
        FirstSpeedEchoPosition = FirstSpeedEchoPosition.WithY(0);
        SecondSpeedEchoPosition = SecondSpeedEchoPosition.WithY(0);
    }

    /// <summary>
    /// Publishes the overloaded `$0AAE-$0AB6` words owned by the shinespark-crash handler.
    /// </summary>
    /// <remarks>
    /// The cartridge reuses the ordinary boost-echo index as an 8-bit radius plus an
    /// 8-bit crash subphase, and reuses the two captured positions as orbiting sprites.
    /// Keeping the write centralized prevents desktop-only crash fields from drifting away
    /// from the exact words consumed by the translated draw handler.
    /// </remarks>
    public void SetShinesparkCrashEchoState(
        ushort encodedIndex,
        Position16<RoomPixels> first,
        Position16<RoomPixels> second)
    {
        SpeedEchoIndex = encodedIndex;
        FirstSpeedEchoPosition = first;
        SecondSpeedEchoPosition = second;
    }

    /// <summary>Adds the native 16.16 extra-run increment while retaining separate WRAM whole and fractional words.</summary>
    private void AddExtraRunAcceleration()
    {
        uint accelerated = unchecked(ExtraRunSpeed.RawValue + SamusMovementRomData.HorizontalMotion.ExtraRunAcceleration);
        ExtraRunSpeed = SnesUnsignedSixteenSixteen.FromRaw(accelerated);
    }

    /// <summary>Enables Speed Booster contact damage only at the fully charged counter's high-byte state.</summary>
    private void PublishBoostContactDamage()
    {
        if (IsFullSpeedBoost)
            ContactDamageIndex = SamusContactDamageIndex.SpeedBooster;
    }

    /// <summary>
    /// Ports <c>Samus_CancelSpeedBoost</c> at <c>$91:DE53</c>, including the transition
    /// from alternating captured positions into the high-bit echo-departure state.
    /// </summary>
    /// <param name="facing">
    /// Current pose-definition direction. Native treats exactly left (four) as facing left;
    /// every other direction uses the right-facing +8 echo velocity branch.
    /// </param>
    public void CancelRunningMomentum(SamusFacingDirection facing)
    {
        if (HasRunningMomentum)
        {
            HasRunningMomentum = false;
            SpeedBoostCounter = default;
            SpecialPaletteFrame = 0;
            SpecialPaletteTimer = NativeCountdown.Start(0);
            NormalSuitPaletteRestoreRequested = true;
        }

        // This block is intentionally outside the momentum test. Every native caller can
        // enter echo-departure mode even when `$0B3C` was already clear. Repeated calls while
        // `$0AAE` is negative must *not* reset the velocities or restart the two bodies.
        if (!SpeedEchoesDeparting)
        {
            SpeedEchoIndex = DepartingSpeedEchoIndex;
            Offset16<RoomPixels> velocity = new(facing == SamusFacingDirection.Left ? (short)-8 : (short)8, 0);
            FirstSpeedEchoVelocity = velocity;
            SecondSpeedEchoVelocity = velocity;
        }
    }

    /// <summary>
    /// Reconciles the speed/echo portion of $91:E633 after the equipment menu.
    /// Unlike ordinary momentum cancellation, unequipping Speed Booster erases
    /// the echoes outright and leaves the numeric run-speed pair until the next
    /// movement update observes the cleared momentum flag.
    /// </summary>
    public void ReconcilePauseSpeedBoosterState(bool speedBoosterEquipped)
    {
        if (speedBoosterEquipped)
        {
            if (HasRunningMomentum && SpeedBoostCounter == default)
            {
                SpecialPaletteTimer = NativeCountdown.Start(0);
                SpecialPaletteFrame = 0;
                SpeedBoostCounter = new SamusSpeedBoostCounter(SamusRunningCadenceDefinitions.ReadResetWord(0));
            }
            return;
        }

        HasRunningMomentum = false;
        SpecialPaletteTimer = NativeCountdown.Start(0);
        SpecialPaletteFrame = 0;
        SpeedBoostCounter = default;
        SpeedEchoIndex = 0;
        FirstSpeedEchoVelocity = SecondSpeedEchoVelocity = Offset16<RoomPixels>.Zero;
        FirstSpeedEchoPosition = SecondSpeedEchoPosition = Position16<RoomPixels>.Zero;
    }

    /// <summary>
    /// Performs the complete horizontal-momentum teardown shared by collision, standing,
    /// posture, aerial, and Morph-Ball handlers. The native seam first cancels Speed Booster
    /// bookkeeping/echoes, then clears the two extra-run words, two base-speed words, and
    /// acceleration mode; keeping that ordering here prevents five copies from diverging.
    /// </summary>
    public void ClearHorizontalMomentum(SamusFacingDirection facingDirection)
    {
        CancelRunningMomentum(facingDirection);
        ClearHorizontalVelocity();
    }

    /// <summary>
    /// Clears the five horizontal velocity/acceleration words without canceling
    /// boost bookkeeping. Crouching's $90:A57C cleanup performs only these stores;
    /// its angle-held input route can therefore preserve a temporary Blue Suit.
    /// </summary>
    public void ClearHorizontalVelocity()
    {
        ExtraRunSpeed = default;
        BaseSpeed = default;
        AccelerationMode = SamusAccelerationMode.Accelerating;
    }

    /// <summary>Runs input fallback command two: stop acceleration and cancel boost, without clearing velocity words.</summary>
    public void ApplyStoppedInputFallback(SamusFacingDirection facingDirection)
    {
        AccelerationMode = SamusAccelerationMode.Accelerating;
        CancelRunningMomentum(facingDirection);
    }

    /// <summary>
    /// Executes running / normal jumping / airborne ball fallback's alpha-selected command one or two.
    /// Command one rechecks speed after movement before folding extra momentum.
    /// </summary>
    public void ApplyDeceleratingInputFallback(bool selectedMomentumCommand, SamusFacingDirection facingDirection)
    {
        if (selectedMomentumCommand && BaseFixed != 0)
        {
            SetBaseFixed(unchecked(BaseFixed + ExtraRunSpeed.RawValue));
            ExtraRunSpeed = default;
            AccelerationMode = SamusAccelerationMode.Decelerating;
        }
        else
            AccelerationMode = SamusAccelerationMode.Accelerating;
        CancelRunningMomentum(facingDirection);
    }

    /// <summary>
    /// Advances one cancellation echo during <c>Samus_DrawEchoes</c> at
    /// <c>$90:87D3-$90:884B</c>.
    /// </summary>
    /// <remarks>
    /// This side effect really belongs to drawing in the cartridge: Y approaches the live
    /// Samus center by two pixels, X advances by the signed eight-pixel velocity, and the
    /// position is cleared on the exact frame it crosses Samus. Returning false means the
    /// slot is empty or crossed this frame and therefore must not emit OAM.
    /// </remarks>
    public bool AdvanceDepartingSpeedEcho(
        int slot,
        Position16<RoomPixels> samusPosition)
    {
        if (!SpeedEchoesDeparting)
            return false;
        if (slot is < 0 or > 1)
            throw new ArgumentOutOfRangeException(nameof(slot));

        Position16<RoomPixels> echo = slot == 0
            ? FirstSpeedEchoPosition
            : SecondSpeedEchoPosition;
        ushort xPosition = echo.X;
        if (xPosition == 0)
            return false;

        ushort yPosition = echo.Y;
        if (yPosition != samusPosition.Y)
        {
            yPosition = unchecked((short)(yPosition - samusPosition.Y)) < 0
                ? unchecked((ushort)(yPosition + 2))
                : unchecked((ushort)(yPosition - 2));
        }

        Offset16<RoomPixels> velocity = slot == 0
            ? FirstSpeedEchoVelocity
            : SecondSpeedEchoVelocity;
        xPosition = unchecked((ushort)(xPosition + velocity.X));
        bool crossedSamus = velocity.X < 0
            ? unchecked((short)(xPosition - samusPosition.X)) < 0
            : unchecked((short)(xPosition - samusPosition.X)) >= 0;
        if (crossedSamus)
            xPosition = 0;

        if (slot == 0)
            FirstSpeedEchoPosition = new(xPosition, yPosition);
        else
            SecondSpeedEchoPosition = new(xPosition, yPosition);

        return xPosition != 0;
    }

    /// <summary>
    /// Executes `$90:884E-$90:8854` after both departure slots have been considered.
    /// Once neither stored X position remains, the shared index returns to ordinary zero.
    /// </summary>
    public void FinishDepartingSpeedEchoFrame()
    {
        if (SpeedEchoesDeparting &&
            FirstSpeedEchoPosition.X == 0 &&
            SecondSpeedEchoPosition.X == 0)
        {
            SpeedEchoIndex = 0;
        }
    }

    /// <summary>
    /// Resolves the row of the entry selected by <c>Samus_DetermineSpeedTableEntryPtr_X</c> at
    /// <c>$90:9BD1</c> among the three adjacent tables, assuming the already-modeled inside-block
    /// reaction chose the table.
    /// </summary>
    public int ResolveEntryRow(SamusMovementType movementType) =>
        SamusHorizontalMotionDefinitions.FirstRow(ActiveSpeedTable) + (byte)movementType;

    /// <summary>
    /// Resolves authored speed entries without ROM access. Fields remain split into the
    /// same high/low words as the original table instead of becoming floating-point values.
    /// </summary>
    public SpeedTableEntry ReadEntry(SamusMovementType movementType) => ReadEntryAtRow(ResolveEntryRow(movementType));

    /// <summary>Resolves an authored speed record by row; a row past the tables reads beyond the authored records.</summary>
    internal static SpeedTableEntry ReadEntryAtRow(int row) =>
        SamusHorizontalMotionDefinitions.TryResolveRow(row, out SpeedTableEntry compiled)
            ? compiled
            : throw new InvalidDataException(
                $"Samus horizontal speed row {row} is past the {SamusHorizontalMotionDefinitions.RowCount} authored mechanics records.");

    /// <summary>
    /// Ports <c>Samus_CalcBaseSpeed_X</c> at <c>$90:9A7E</c> and returns its unsigned
    /// 16.16 result. It mutates the modeled WRAM speed words exactly once.
    /// </summary>
    public uint CalculateBaseSpeed(SamusMovementType movementType)
    {
        SpeedTableEntry entry = ReadEntry(movementType);
        return CalculateBaseSpeed(entry);
    }

    /// <summary>
    /// Executes `$90:9A7E` against a standalone record. Bomb jumps use the standalone record at
    /// `$90:9F25` instead of the movement-type-indexed normal table.
    /// </summary>
    public uint CalculateBaseSpeed(SamusStandaloneSpeedRecord record) =>
        CalculateBaseSpeed(SamusHorizontalMotionDefinitions.Standalone(record));

    /// <summary>Applies the table's acceleration or deceleration using native split-word arithmetic and updates base speed.</summary>
    internal uint CalculateBaseSpeed(SpeedTableEntry entry)
    {

        if (AccelerationMode != SamusAccelerationMode.Accelerating)
        {
            SetBaseFixed(unchecked(BaseFixed - (uint)ScaledDeceleration(entry).RawValue));

            // The native BMI tests only the signed high word. Once deceleration crosses
            // below zero it clears both halves and returns to acceleration mode zero.
            if (unchecked((short)BaseSpeed.Whole) < 0)
            {
                BaseSpeed = default;
                AccelerationMode = SamusAccelerationMode.Accelerating;
            }
        }
        else
        {
            SetBaseFixed(unchecked(BaseFixed + (uint)entry.Acceleration.RawValue));

            // $90:9A7E does not use a normal unsigned 32-bit >= comparison. Preserve its
            // signed word-subtraction quirk, including the fact that exact equality does
            // not count as greater and therefore is left untouched.
            if (IsGreaterThanQuirked(
                    BaseSpeed.Whole,
                    BaseSpeed.Fraction,
                    entry.MaximumSpeed.WholeWord,
                    entry.MaximumSpeed.Fraction))
            {
                SetBaseFixed(unchecked((uint)entry.MaximumSpeed.RawValue));
            }
        }

        return BaseFixed;
    }

    /// <summary>
    /// Ports <c>CalculateSamusXBaseSpeed_DecelerationDisallowed</c> at
    /// <c>$90:9B1F</c>. Despite its name, acceleration-mode bit zero still selects the
    /// turning/deceleration branch; modes zero and two both accelerate toward the table
    /// maximum. The returned flag is the routine's carry result and is important to spin
    /// jump's decision to retain horizontal motion.
    /// </summary>
    public AerialBaseSpeedResult CalculateBaseSpeedDecelerationDisallowed(
        SamusMovementType movementType)
    {
        SpeedTableEntry entry = ReadEntry(movementType);

        if (AccelerationMode == SamusAccelerationMode.Turning)
        {
            // This is byte-for-byte the same asymmetric multiplier calculation used by
            // the deceleration-allowed routine. The distinction between the two native
            // entry points is the bit test above and their carry result, not the subtract.
            SetBaseFixed(unchecked(BaseFixed - (uint)ScaledDeceleration(entry).RawValue));
            if (unchecked((short)BaseSpeed.Whole) < 0)
            {
                BaseSpeed = default;
                AccelerationMode = SamusAccelerationMode.Accelerating;
            }

            // Every path through $90:9B5E-$90:9BC3 exits with carry clear.
            return new AerialBaseSpeedResult(BaseFixed, ReachedMaximum: false);
        }

        SetBaseFixed(unchecked(BaseFixed + (uint)entry.Acceleration.RawValue));

        // CMP/BMI performs signed 16-bit comparisons. Exact equality in both halves is
        // deliberately *not* considered a cap: $90:9B5A returns carry clear in that case.
        ushort maximumWhole = entry.MaximumSpeed.WholeWord;
        ushort maximumFraction = entry.MaximumSpeed.Fraction;
        bool exceedsMaximum = unchecked((short)(BaseSpeed.Whole - maximumWhole)) > 0 ||
            (BaseSpeed.Whole == maximumWhole &&
             unchecked((short)(BaseSpeed.Fraction - maximumFraction)) > 0);
        if (exceedsMaximum)
        {
            SetBaseFixed(unchecked((uint)entry.MaximumSpeed.RawValue));
            return new AerialBaseSpeedResult(BaseFixed, ReachedMaximum: true);
        }

        return new AerialBaseSpeedResult(BaseFixed, ReachedMaximum: false);
    }

    /// <summary>
    /// Ports <c>Samus_CalcSpeed_X</c> at <c>$90:E4E6</c>: add extra run speed, shift by
    /// min(divisor, 4), and publish the total-speed WRAM pair used by animation/collision.
    /// </summary>
    public uint CalculateTotalSpeed(uint baseSpeed)
    {
        uint total = unchecked(baseSpeed + ExtraRunSpeed.RawValue);
        int shift = Math.Min(SpeedDivisor, 4);
        total >>= shift;
        TotalSpeed = SnesUnsignedSixteenSixteen.FromRaw(total);
        return total;
    }

    /// <summary>
    /// Ports the non-collision portion of <c>Samus_CalcDisplacementMoveRight</c> at
    /// <c>$90:E4AD</c>, including the ±15-pixel whole-word clamp from <c>$90:E430</c>.
    /// </summary>
    public int CalculateRightDisplacement(uint baseSpeed, int extraDisplacement = 0) =>
        ClampDisplacement(unchecked((int)CalculateTotalSpeed(baseSpeed) + extraDisplacement));

    /// <summary>
    /// Ports the non-collision portion of <c>Samus_CalcDisplacementMoveLeft</c> at
    /// <c>$90:E464</c>. Left is native <c>extra displacement - total speed</c>, not merely
    /// a sign bit attached to the rightward result.
    /// </summary>
    public int CalculateLeftDisplacement(uint baseSpeed, int extraDisplacement = 0) =>
        ClampDisplacement(unchecked(extraDisplacement - (int)CalculateTotalSpeed(baseSpeed)));

    /// <summary>Current base speed as the native high-word/low-word unsigned 16.16 pair.</summary>
    public uint BaseFixed => BaseSpeed.RawValue;

    /// <summary>Publishes a 16.16 result to the separate base-speed whole and fractional WRAM words.</summary>
    private void SetBaseFixed(uint value)
    {
        BaseSpeed = SnesUnsignedSixteenSixteen.FromRaw(value);
    }

    /// <summary>
    /// The deceleration one frame removes. Without a multiplier it is the table's value; with
    /// one, the whole part is multiplied in full but only the upper half of the fraction is,
    /// and each product keeps only what fits its half, as the cartridge's byte multiplies do.
    /// Combining them as ordinary fixed-point multiplication would be cleaner, but would not be
    /// what the cartridge executes.
    /// </summary>
    private SnesSignedSixteenSixteen ScaledDeceleration(SpeedTableEntry entry)
    {
        if (DecelerationMultiplier == 0)
            return entry.Deceleration;

        int wholeProduct = DecelerationMultiplier * entry.Deceleration.WholeWord;
        int fractionProduct = DecelerationMultiplier * SnesEightEight.WholeOf(entry.Deceleration.Fraction);
        return SnesSignedSixteenSixteen.FromParts(
            unchecked((short)SnesEightEight.WholeOf(wholeProduct)),
            unchecked((ushort)fractionProduct));
    }

    /// <summary>Compares split words with the cartridge's signed subtraction and strict low-word tie rule.</summary>
    private static bool IsGreaterThanQuirked(
        int valueHigh,
        int valueLow,
        int comparisonHigh,
        int comparisonLow)
    {
        if (unchecked((short)(valueHigh - comparisonHigh)) < 0)
            return false;

        return valueHigh != comparisonHigh ||
               (unchecked((short)(valueLow - comparisonLow)) >= 0 && valueLow != comparisonLow);
    }

    /// <summary>Clamps the whole-pixel part to plus or minus fifteen while preserving fractional displacement.</summary>
    private static int ClampDisplacement(int displacement)
    {
        short high = unchecked((short)(SnesSixteenSixteen.WholeOf(displacement)));
        ushort low = unchecked((ushort)displacement);

        if (high < 0)
        {
            if (unchecked((short)(high + 15)) < 0)
                high = -15;
        }
        else if (unchecked((short)(high - 16)) >= 0)
        {
            high = 15;
        }

        return unchecked((int)(SnesSixteenSixteen.Compose((ushort)high, low)));
    }

}
