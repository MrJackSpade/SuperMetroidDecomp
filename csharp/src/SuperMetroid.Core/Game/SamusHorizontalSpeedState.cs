using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;

namespace SuperMetroid.Core.Game;

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
    /// <summary>Whole part of <c>samus_x_base_speed</c> at WRAM <c>$0B46</c>.</summary>
    public ushort BaseSpeed { get; set; }

    /// <summary>Fractional part of <c>samus_x_base_subspeed</c> at WRAM <c>$0B48</c>.</summary>
    public ushort BaseSubspeed { get; set; }

    /// <summary>Whole part of the run-button/speed-booster addition at WRAM <c>$0B42</c>.</summary>
    public ushort ExtraRunSpeed { get; set; }

    /// <summary>Fractional part of the run-button/speed-booster addition at WRAM <c>$0B44</c>.</summary>
    public ushort ExtraRunSubspeed { get; set; }

    /// <summary>
    /// Native <c>samus_has_momentum_flag</c>. Once dash begins, releasing Dash or leaving
    /// movement type one retains the accumulated extra component until a specific cancel
    /// routine (standing, turning, collision, etc.) clears WRAM <c>$0B3C</c>.
    /// </summary>
    public bool HasRunningMomentum { get; set; }

    /// <summary>
    /// Native speed-booster timing word. The high byte is stage zero through four and the
    /// low byte is a ROM-authored animation-loop countdown. No-Speed-Booster Dash keeps it zero.
    /// </summary>
    public ushort SpeedBoostCounter { get; set; }

    /// <summary>WRAM <c>$0ACE</c>, reset when Speed Booster momentum begins/cancels.</summary>
    public ushort SpecialPaletteFrame { get; set; }

    /// <summary>WRAM <c>$0AD0</c>, seeded to one on the first boosted running frame.</summary>
    public ushort SpecialPaletteTimer { get; set; }

    /// <summary>
    /// One-shot event set when `$90:852C` enters stage four. Audio can consume this flag
    /// later without making the movement translation depend on a host sound backend.
    /// </summary>
    public bool EchoSoundRequested { get; set; }

    /// <summary>Consumes speed stage four's native <c>QueueSfx3_Max6($03)</c> call.</summary>
    public bool ConsumeEchoSoundRequest()
    {
        bool requested = EchoSoundRequested;
        EchoSoundRequested = false;
        return requested;
    }

    /// <summary>Contact-damage selector published when the counter reaches stage four.</summary>
    public ushort ContactDamageIndex { get; set; }

    /// <summary>
    /// Host-side publication of the immediate suit-palette copy performed by
    /// <c>Cancel_SpeedBoosting</c> at <c>$91:DE53</c>. Movement owns the cancellation,
    /// while the runtime owns CGRAM, so this flag preserves the native same-frame handoff
    /// without passing a renderer through every collision and pose routine.
    /// </summary>
    public bool NormalSuitPaletteRestoreRequested { get; private set; }

    /// <summary>
    /// Alternating word index at WRAM <c>$0AAE</c>. Ordinary active Speed Booster echoes
    /// use only values zero and two. Cancellation writes <c>$FFFF</c> while the two stored
    /// bodies travel back toward Samus; the shinespark crash temporarily overloads the same
    /// word as an eight-bit radius plus an eight-bit phase.
    /// </summary>
    public ushort SpeedEchoIndex { get; private set; }

    /// <summary>First captured Speed Booster echo X word at WRAM <c>$0AB0</c>.</summary>
    public ushort FirstSpeedEchoXPosition { get; private set; }

    /// <summary>Second captured Speed Booster echo X word at WRAM <c>$0AB2</c>.</summary>
    public ushort SecondSpeedEchoXPosition { get; private set; }

    /// <summary>First captured Speed Booster echo Y word at WRAM <c>$0AB4</c>.</summary>
    public ushort FirstSpeedEchoYPosition { get; private set; }

    /// <summary>Second captured Speed Booster echo Y word at WRAM <c>$0AB6</c>.</summary>
    public ushort SecondSpeedEchoYPosition { get; private set; }

    /// <summary>
    /// Signed whole-pixel X velocity for ordinary departing echo zero at WRAM
    /// <c>$0AC0</c>. Cancellation writes -8 while facing left or +8 otherwise.
    /// </summary>
    public ushort FirstSpeedEchoXSpeed { get; private set; }

    /// <summary>Signed whole-pixel X velocity for departing echo one at WRAM <c>$0AC2</c>.</summary>
    public ushort SecondSpeedEchoXSpeed { get; private set; }

    /// <summary>Whole part produced by <c>Samus_CalcSpeed_X</c> at WRAM <c>$0B48</c>.</summary>
    public ushort TotalSpeed { get; private set; }

    /// <summary>Fractional part produced by <c>Samus_CalcSpeed_X</c> at WRAM <c>$0B46</c>.</summary>
    public ushort TotalSubspeed { get; private set; }

    /// <summary>
    /// Right-shift count at WRAM <c>$0A66</c>. The native code caps the effective shift at
    /// four, even if this stored word is larger.
    /// </summary>
    public ushort SpeedDivisor { get; set; }

    /// <summary>
    /// Acceleration mode at WRAM <c>$0B4A</c>: zero accelerates, while every nonzero value
    /// takes the deceleration branch in <c>$90:9A7E</c>.
    /// </summary>
    public ushort AccelerationMode { get; set; }

    /// <summary>
    /// Optional eight-bit multiplier at WRAM <c>$0B4C</c>. Zero selects the table's plain
    /// deceleration pair; nonzero preserves the original's asymmetric byte products.
    /// </summary>
    public byte DecelerationMultiplier { get; set; }

    /// <summary>
    /// Bank-$90 offset stored by the current inside-block/environment reaction. Normal air,
    /// water, and lava/acid select three complete ROM tables before movement type is added.
    /// </summary>
    public ushort ActiveSpeedTableBaseAddress { get; private set; } =
        SamusMovementRomData.HorizontalMotion.NormalAirSpeedTable;

    /// <summary>
    /// Reproduces the ordinary-air assignment made by <c>$94:97D0</c> before movement.
    /// </summary>
    public void SelectNormalAirSpeedTable() =>
        ActiveSpeedTableBaseAddress = SamusMovementRomData.HorizontalMotion.NormalAirSpeedTable;

    /// <summary>
    /// Ports <c>Determine_Samus_X_Speed_Table_Entry_Pointer</c>'s environmental selection
    /// at <c>$90:9BD1</c>. The returned table still contains 12-byte records indexed later
    /// by movement type; no acceleration or cap is copied into host constants.
    /// </summary>
    public void SelectEnvironmentSpeedTable(ushort liquidMedium)
    {
        ActiveSpeedTableBaseAddress = liquidMedium switch
        {
            SamusLiquidPhysicsState.Water => SamusMovementRomData.HorizontalMotion.WaterSpeedTable,
            SamusLiquidPhysicsState.LavaAcid => SamusMovementRomData.HorizontalMotion.LavaAcidSpeedTable,
            _ => SamusMovementRomData.HorizontalMotion.NormalAirSpeedTable,
        };
    }

    /// <summary>
    /// Ports the dry-air portion of <c>Handle_Samus_XExtraRunSpeed</c> at <c>$90:973E</c>.
    /// Both routes accelerate by hexadecimal <c>0.1000</c>. Ordinary Dash caps at
    /// <c>2.0000</c>; equipped Speed Booster caps at <c>7.0000</c> and initializes its
    /// counter through `$91:B61F`. Their shared momentum flag survives release and jumps.
    /// </summary>
    public void HandleExtraRunSpeed(
        SamusMovementType movementType,
        ushort controllerInput,
        bool speedBoosterEquipped,
        ISnesAddressSpace? bus = null,
        bool liquidImpeded = false)
    {
        const SnesButton dashButton = SnesButton.B;
        // `$90:9746-$9763` diverts a non-Gravity submerged body to the same no-acceleration
        // branch as releasing Dash. Existing momentum retains its numeric extra component;
        // without momentum, both words are cleared. This is not a multiplier or hard reset.
        bool activelyDashing = !liquidImpeded &&
            movementType == SamusMovementType.Running &&
            SnesButtons.FromRaw(controllerInput, "extra run-speed input").HasAny(dashButton);
        if (!activelyDashing)
        {
            // `$90:9808` clears the extra pair only before momentum has been established.
            // A true flag carries the pair through airborne movement and a released button.
            if (!HasRunningMomentum)
            {
                ExtraRunSpeed = 0;
                ExtraRunSubspeed = 0;
            }
            return;
        }

        if (speedBoosterEquipped)
        {
            ArgumentNullException.ThrowIfNull(bus);
            if (!HasRunningMomentum)
            {
                // `$90:976C-$9780` initializes the counter's low byte from the live ROM
                // table. Palette writes themselves remain renderer work, but their native
                // frame/timer state is movement-visible and therefore retained here.
                HasRunningMomentum = true;
                SpecialPaletteTimer = 1;
                SpecialPaletteFrame = 0;
                SpeedBoostCounter = ReadWord(
                    bus,
                    SamusMovementRomData.HorizontalMotion.SpeedBoostCounterLowBytes);
            }

            if (unchecked((short)(ExtraRunSpeed - 7)) >= 0 &&
                unchecked((short)ExtraRunSubspeed) >= 0)
            {
                ExtraRunSpeed = 7;
                ExtraRunSubspeed = 0;
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
            SpeedBoostCounter = 0;
        }

        // The cartridge compares the two words separately with signed BMI branches. This
        // intentionally is not a conventional unsigned 32-bit >= comparison. The retail
        // no-booster fractional cap is zero, so a normal progression reaches 2.0000 and
        // clamps there on the following call before another 0.1000 can be added.
        if (unchecked((short)(ExtraRunSpeed - 2)) >= 0 &&
            unchecked((short)ExtraRunSubspeed) >= 0)
        {
            ExtraRunSpeed = 2;
            ExtraRunSubspeed = 0;
            return;
        }

        AddExtraRunAcceleration();
    }

    /// <summary>
    /// Executes the equipped-Speed-Booster interception at `$90:852C-$856B` when running
    /// reaches an animation command. True means the command was consumed and frame zero
    /// was restarted from the ROM-authored delay list for the new boost stage.
    /// </summary>
    public bool TryAdvanceSpeedBoosterAnimationStage(
        ISnesAddressSpace bus,
        SamusMovementType movementType,
        ushort controllerInput,
        ushort animationFrameBuffer,
        ref ushort animationFrame,
        out ushort animationFrameTimer)
    {
        ArgumentNullException.ThrowIfNull(bus);
        animationFrameTimer = 0;
        const SnesButton dashButton = SnesButton.B;
        SnesButton heldButtons = SnesButtons.FromRaw(controllerInput, "speed-boost animation input");
        if (!HasRunningMomentum || movementType != SamusMovementType.Running || !heldButtons.HasAny(dashButton))
            return false;

        // DEC is 16-bit, but native then changes A to eight-bit before BNE. A stage advances
        // only when the decremented low byte is zero; the high byte remains the stage index.
        SpeedBoostCounter = unchecked((ushort)(SpeedBoostCounter - 1));
        if ((byte)SpeedBoostCounter != 0)
            return false;

        ushort stagedCounter = SpeedBoostCounter;
        if ((stagedCounter & 0x0400) == 0)
        {
            stagedCounter = unchecked((ushort)(stagedCounter + 0x0100));
            SpeedBoostCounter = stagedCounter;
            if ((stagedCounter & 0x0400) != 0)
                EchoSoundRequested = true;
        }

        byte stage = unchecked((byte)(stagedCounter >> 8));
        ushort nextLowByte = ReadWord(
            bus,
            SamusMovementRomData.HorizontalMotion.SpeedBoostCounterLowBytes + stage * 2);
        SpeedBoostCounter = unchecked((ushort)((SpeedBoostCounter & 0xff00) | nextLowByte));

        ushort delayList = ReadWord(
            bus,
            SamusMovementRomData.HorizontalMotion.SpeedBoostAnimationDelayListPointers + stage * 2);
        animationFrame = 0;
        animationFrameTimer = unchecked((ushort)(
            animationFrameBuffer + bus.ReadByte((int)new SnesAddress(0x91, delayList))));
        PublishBoostContactDamage();
        return true;
    }

    /// <summary>Reads one byte from the delay list selected by the counter's stage byte.</summary>
    public byte ReadSpeedBoosterAnimationByte(ISnesAddressSpace bus, ushort byteIndex)
    {
        ArgumentNullException.ThrowIfNull(bus);
        byte stage = unchecked((byte)(SpeedBoostCounter >> 8));
        ushort delayList = ReadWord(
            bus,
            SamusMovementRomData.HorizontalMotion.SpeedBoostAnimationDelayListPointers + stage * 2);
        int address = SamusMovementRomData.Banks.Pose |
            unchecked((ushort)(delayList + byteIndex));
        return bus.ReadByte(address);
    }

    /// <summary>
    /// Ports the dry-room Speed Booster branch of
    /// <c>Handle_ScrewAttack_SpeedBoosting_Palette</c> at <c>$91:D9B2</c>.
    /// </summary>
    /// <returns>True when this call copied a ROM-authored palette into CGRAM.</returns>
    public bool UpdateSpeedBoosterPalette(
        ISnesAddressSpace bus,
        SnesCgram cgram,
        SamusMovementType movementType,
        ushort animationFrame,
        ushort equippedItems,
        bool suppressActiveSpeedBoosterPalette = false,
        bool bottomBoundarySubmerged = false)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(cgram);

        bool paletteCopied = false;
        ushort suitTableOffset = equippedItems.GetSuitPaletteTableOffset();
        if (NormalSuitPaletteRestoreRequested)
        {
            // `$91:DE6A-$DE8A` picks Gravity, then Varia, then Power Suit and invokes the
            // same 32-byte bank-$9B copy used by normal palette handling. Table `$91:D727`
            // expresses that choice directly and keeps every color cartridge-authored.
            ushort normalPalette = ReadWord(
                bus,
                SamusPaletteRomData.Common.NormalSuitPointers + suitTableOffset);
            cgram.LoadFromBus(
                bus,
                SamusPaletteRomData.Banks.Palette | normalPalette,
                colorCount: SamusPaletteRomData.Common.ColorsPerObjPalette,
                destinationIndex: SamusPaletteRomData.Common.SamusObjPaletteStart);
            NormalSuitPaletteRestoreRequested = false;
            paletteCopied = true;
        }

        // Stored-shine and shinespark handlers have priority in `$91:D6F7`. Cancellation's
        // immediate normal-palette copy above still occurs, but handler zero must not advance
        // its hidden four-frame Speed Booster cycle beneath handler one or six.
        if (suppressActiveSpeedBoosterPalette)
            return paletteCopied;

        // `$91:D9B2-$D9D8` tests the suit-palette Gravity bit before it asks whether
        // Samus's *bottom* boundary lies under water or lava/acid. Power and Varia Samus
        // return carry-set immediately when submerged: the current CGRAM colors, common
        // timer, and palette-list offset all remain untouched. Gravity Suit bypasses this
        // gate and continues into the ordinary Screw Attack / Speed Booster logic even
        // though the raw room-FX comparison still says the body is underwater. The caller
        // supplies the raw boundary result so this state owner does not invent a second,
        // subtly different interpretation of the room's FX words.
        bool gravitySuitEquipped = equippedItems.HasAny(SamusEquipmentFlags.GravitySuit);
        if (!gravitySuitEquipped && bottomBoundarySubmerged)
            return paletteCopied;

        bool screwAttackEquipped = (equippedItems & 0x0008) != 0;
        if (movementType == SamusMovementType.SpinJumping && screwAttackEquipped)
        {
            if (animationFrame == 0)
            {
                // `$91:D9F8` resets only the palette-list byte offset. Frame zero retains
                // the already loaded normal suit colors; it does not emit a Screw palette.
                SpecialPaletteFrame = 0;
                return paletteCopied;
            }

            if (animationFrame < 0x1b)
            {
                // Returning zero at `$91:D9EC` asks the outer palette dispatcher to copy
                // the normal suit palette for Screw frames 1..26. Perform that copy here
                // because CGRAM is the desktop runtime's directly visible palette buffer.
                LoadNormalSuitPalette(bus, cgram, suitTableOffset);
                return true;
            }

            CopyAndAdvanceScrewAttackPalette(bus, cgram, suitTableOffset);
            return true;
        }

        if (movementType == SamusMovementType.WallJumping)
        {
            if (!screwAttackEquipped)
                return paletteCopied;

            if (animationFrame < 3)
            {
                // Wall-jump command `$FB` reserves frames 0..2 for the normal suit colors;
                // its Screw family begins on frame three and shares the same six pointers.
                SpecialPaletteFrame = 0;
                return paletteCopied;
            }

            CopyAndAdvanceScrewAttackPalette(bus, cgram, suitTableOffset);
            return true;
        }

        if ((SpeedBoostCounter & 0xff00) != 0x0400)
            return paletteCopied;

        // DEC is a full 16-bit operation. BEQ/BPL mean zero and signed underflow both reload
        // four, though ordinary execution arrives with timer one and never underflows.
        NativeWordCounterStep timer = NativeWordCounter.Decrement(SpecialPaletteTimer);
        SpecialPaletteTimer = timer.Value;
        if (!timer.IsZero && timer.IsNonNegative)
            return paletteCopied;

        SpecialPaletteTimer = 4;

        // `$91:DAA9` selects a bank-$91 list for the active suit. The list entry selected by
        // `$0ACE` is in turn a bank-$9B palette pointer. This double indirection is retained
        // instead of copying the four retail addresses into C# constants.
        ushort paletteList = ReadWord(
            bus,
            SamusPaletteRomData.FullBodyCycles.SpeedBoosterLists + suitTableOffset);
        ushort palettePointer = ReadWord(
            bus,
            SamusPaletteRomData.Banks.Movement |
                unchecked((ushort)(paletteList + SpecialPaletteFrame)));
        cgram.LoadFromBus(
            bus,
            SamusPaletteRomData.Banks.Palette | palettePointer,
            colorCount: SamusPaletteRomData.Common.ColorsPerObjPalette,
            destinationIndex: SamusPaletteRomData.Common.SamusObjPaletteStart);

        // Native advances offsets 0,2,4,6 and then pins six. No out-of-range lookup occurs
        // in reachable play because initialization and cancellation both reset the word.
        SpecialPaletteFrame = SpecialPaletteFrame >= 6
            ? (ushort)6
            : unchecked((ushort)(SpecialPaletteFrame + 2));
        return true;
    }

    /// <summary>
    /// Defers the normal 16-color suit copy to the runtime's `$91:D6F7` palette phase.
    /// Native pose initialization performs this when leaving a Screw Attack spin family.
    /// </summary>
    public void RequestNormalSuitPaletteRestore() => NormalSuitPaletteRestoreRequested = true;

    /// <summary>Copies one of the six ROM-authored Screw Attack palettes and wraps its offset.</summary>
    private void CopyAndAdvanceScrewAttackPalette(
        ISnesAddressSpace bus,
        SnesCgram cgram,
        ushort suitTableOffset)
    {
        // `$91:DA4A` contains one bank-$91 pointer list per suit. Each selected word is a
        // bank-$9B address for a complete 16-color Samus palette, exactly like Speed Boost.
        ushort paletteList = ReadWord(
            bus,
            SamusPaletteRomData.FullBodyCycles.ScrewAttackLists + suitTableOffset);
        ushort palettePointer = ReadWord(
            bus,
            SamusPaletteRomData.Banks.Movement |
                unchecked((ushort)(paletteList + SpecialPaletteFrame)));
        cgram.LoadFromBus(
            bus,
            SamusPaletteRomData.Banks.Palette | palettePointer,
            colorCount: SamusPaletteRomData.Common.ColorsPerObjPalette,
            destinationIndex: SamusPaletteRomData.Common.SamusObjPaletteStart);

        // Offsets 0,2,4,6,8,10 form the six-frame cycle. The native CMP uses the current
        // offset, so ten wraps to zero only after its palette has been copied.
        SpecialPaletteFrame = SpecialPaletteFrame >= 10
            ? (ushort)0
            : unchecked((ushort)(SpecialPaletteFrame + 2));
    }

    /// <summary>Loads the normal Power/Varia/Gravity palette selected by the native suit index.</summary>
    private static void LoadNormalSuitPalette(
        ISnesAddressSpace bus,
        SnesCgram cgram,
        ushort suitTableOffset)
    {
        ushort normalPalette = ReadWord(
            bus,
            SamusPaletteRomData.Common.NormalSuitPointers + suitTableOffset);
        cgram.LoadFromBus(
            bus,
            SamusPaletteRomData.Banks.Palette | normalPalette,
            colorCount: SamusPaletteRomData.Common.ColorsPerObjPalette,
            destinationIndex: SamusPaletteRomData.Common.SamusObjPaletteStart);
    }

    /// <summary>
    /// Ports <c>Samus_UpdateSpeedEchoPos</c> at <c>$90:EEE7</c> for ordinary active boost.
    /// The native game-time counter is one-to-one with accepted frames in this runtime.
    /// </summary>
    public bool CaptureSpeedEchoPosition(
        ushort gameTimeFrames,
        ushort xPosition,
        ushort yPosition)
    {
        if ((SpeedBoostCounter & 0xff00) != 0x0400 ||
            (SpeedEchoIndex & 0x8000) != 0 ||
            (gameTimeFrames & 3) != 0)
        {
            return false;
        }

        if (SpeedEchoIndex == 0)
        {
            FirstSpeedEchoXPosition = xPosition;
            FirstSpeedEchoYPosition = yPosition;
        }
        else
        {
            SecondSpeedEchoXPosition = xPosition;
            SecondSpeedEchoYPosition = yPosition;
        }

        SpeedEchoIndex = unchecked((ushort)(SpeedEchoIndex + 2));
        if (unchecked((short)(SpeedEchoIndex - 4)) >= 0)
            SpeedEchoIndex = 0;
        return true;
    }

    /// <summary>
    /// Applies the observable echo-slot clears shared by shinespark windup at
    /// <c>$90:D068</c> and directional launch setup at <c>$91:F83E</c>.
    /// </summary>
    /// <remarks>
    /// Native also clears two later crash-circle velocity bytes. Those bytes are not part
    /// of the ordinary active-echo model yet; the four position words and alternating index
    /// below are the exact subset consumed by <see cref="CaptureSpeedEchoPosition"/> and
    /// the current renderer.
    /// </remarks>
    public void ResetSpeedEchoPositionsForShinespark()
    {
        SpeedEchoIndex = 0;
        FirstSpeedEchoXSpeed = 0;
        SecondSpeedEchoXSpeed = 0;
        FirstSpeedEchoXPosition = 0;
        SecondSpeedEchoXPosition = 0;
        FirstSpeedEchoYPosition = 0;
        SecondSpeedEchoYPosition = 0;
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
        ushort firstX,
        ushort secondX,
        ushort firstY,
        ushort secondY)
    {
        SpeedEchoIndex = encodedIndex;
        FirstSpeedEchoXPosition = firstX;
        SecondSpeedEchoXPosition = secondX;
        FirstSpeedEchoYPosition = firstY;
        SecondSpeedEchoYPosition = secondY;
    }

    private void AddExtraRunAcceleration()
    {
        uint accelerated = unchecked(Compose(ExtraRunSpeed, ExtraRunSubspeed) + 0x00001000u);
        ExtraRunSpeed = unchecked((ushort)(accelerated >> 16));
        ExtraRunSubspeed = unchecked((ushort)accelerated);
    }

    private void PublishBoostContactDamage()
    {
        if ((SpeedBoostCounter & 0xff00) == 0x0400)
            ContactDamageIndex = 1;
    }

    /// <summary>
    /// Ports <c>Samus_CancelSpeedBoost</c> at <c>$91:DE53</c>, including the transition
    /// from alternating captured positions into the high-bit echo-departure state.
    /// </summary>
    /// <param name="poseXDirection">
    /// Current pose-definition direction byte. Native treats exactly four as facing left;
    /// every other value uses the right-facing +8 echo velocity branch.
    /// </param>
    public void CancelRunningMomentum(byte poseXDirection)
    {
        if (HasRunningMomentum)
        {
            HasRunningMomentum = false;
            SpeedBoostCounter = 0;
            SpecialPaletteFrame = 0;
            SpecialPaletteTimer = 0;
            NormalSuitPaletteRestoreRequested = true;
        }

        // This block is intentionally outside the momentum test. Every native caller can
        // enter echo-departure mode even when `$0B3C` was already clear. Repeated calls while
        // `$0AAE` is negative must *not* reset the velocities or restart the two bodies.
        if ((SpeedEchoIndex & 0x8000) == 0)
        {
            SpeedEchoIndex = 0xffff;
            ushort velocity = (SamusFacingDirection)poseXDirection == SamusFacingDirection.Left
                ? unchecked((ushort)-8)
                : (ushort)8;
            FirstSpeedEchoXSpeed = velocity;
            SecondSpeedEchoXSpeed = velocity;
        }
    }

    /// <summary>
    /// Performs the complete horizontal-momentum teardown shared by collision, standing,
    /// posture, aerial, and Morph-Ball handlers. The native seam first cancels Speed Booster
    /// bookkeeping/echoes, then clears the two extra-run words, two base-speed words, and
    /// acceleration mode; keeping that ordering here prevents five copies from diverging.
    /// </summary>
    public void ClearHorizontalMomentum(SamusFacingDirection facingDirection)
    {
        CancelRunningMomentum((byte)facingDirection);
        ExtraRunSpeed = 0;
        ExtraRunSubspeed = 0;
        BaseSpeed = 0;
        BaseSubspeed = 0;
        AccelerationMode = 0;
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
        ushort samusXPosition,
        ushort samusYPosition)
    {
        if ((SpeedEchoIndex & 0x8000) == 0)
            return false;
        if (slot is < 0 or > 1)
            throw new ArgumentOutOfRangeException(nameof(slot));

        ushort xPosition = slot == 0
            ? FirstSpeedEchoXPosition
            : SecondSpeedEchoXPosition;
        if (xPosition == 0)
            return false;

        ushort yPosition = slot == 0
            ? FirstSpeedEchoYPosition
            : SecondSpeedEchoYPosition;
        if (yPosition != samusYPosition)
        {
            yPosition = unchecked((short)(yPosition - samusYPosition)) < 0
                ? unchecked((ushort)(yPosition + 2))
                : unchecked((ushort)(yPosition - 2));
        }

        ushort xSpeed = slot == 0
            ? FirstSpeedEchoXSpeed
            : SecondSpeedEchoXSpeed;
        xPosition = unchecked((ushort)(xPosition + xSpeed));
        bool crossedSamus = unchecked((short)xSpeed) < 0
            ? unchecked((short)(xPosition - samusXPosition)) < 0
            : unchecked((short)(xPosition - samusXPosition)) >= 0;
        if (crossedSamus)
            xPosition = 0;

        if (slot == 0)
        {
            FirstSpeedEchoXPosition = xPosition;
            FirstSpeedEchoYPosition = yPosition;
        }
        else
        {
            SecondSpeedEchoXPosition = xPosition;
            SecondSpeedEchoYPosition = yPosition;
        }

        return xPosition != 0;
    }

    /// <summary>
    /// Executes `$90:884E-$90:8854` after both departure slots have been considered.
    /// Once neither stored X position remains, the shared index returns to ordinary zero.
    /// </summary>
    public void FinishDepartingSpeedEchoFrame()
    {
        if ((SpeedEchoIndex & 0x8000) != 0 &&
            FirstSpeedEchoXPosition == 0 &&
            SecondSpeedEchoXPosition == 0)
        {
            SpeedEchoIndex = 0;
        }
    }

    /// <summary>
    /// Resolves the exact 12-byte entry selected by <c>Samus_DetermineSpeedTableEntryPtr_X</c>
    /// at <c>$90:9BD1</c>, assuming the already-modeled inside-block reaction chose the base.
    /// </summary>
    public int ResolveEntryAddress(SamusMovementType movementType)
    {
        ushort bankOffset = unchecked((ushort)(
            ActiveSpeedTableBaseAddress + SpeedTableEntry.ByteCount * (byte)movementType));
        return SamusMovementRomData.Banks.Movement | bankOffset;
    }

    /// <summary>
    /// Reads a speed entry directly from cartridge bank $90. Fields remain split into the
    /// same high/low words as the original table instead of becoming floating-point values.
    /// </summary>
    public SpeedTableEntry ReadEntry(ISnesAddressSpace bus, SamusMovementType movementType)
    {
        ArgumentNullException.ThrowIfNull(bus);
        int address = ResolveEntryAddress(movementType);
        return new SpeedTableEntry(
            ReadWord(bus, address + 0),
            ReadWord(bus, address + 2),
            ReadWord(bus, address + 4),
            ReadWord(bus, address + 6),
            ReadWord(bus, address + 8),
            ReadWord(bus, address + 10));
    }

    /// <summary>
    /// Ports <c>Samus_CalcBaseSpeed_X</c> at <c>$90:9A7E</c> and returns its unsigned
    /// 16.16 result. It mutates the modeled WRAM speed words exactly once.
    /// </summary>
    public uint CalculateBaseSpeed(ISnesAddressSpace bus, SamusMovementType movementType)
    {
        SpeedTableEntry entry = ReadEntry(bus, movementType);
        return CalculateBaseSpeed(entry);
    }

    /// <summary>
    /// Executes `$90:9A7E` against a literal bank-$90 table address. Bomb jumps use the
    /// standalone record at `$90:9F25` instead of the movement-type-indexed normal table.
    /// </summary>
    public uint CalculateBaseSpeedAtAddress(ISnesAddressSpace bus, int address)
    {
        ArgumentNullException.ThrowIfNull(bus);
        var entry = new SpeedTableEntry(
            ReadWord(bus, address + 0),
            ReadWord(bus, address + 2),
            ReadWord(bus, address + 4),
            ReadWord(bus, address + 6),
            ReadWord(bus, address + 8),
            ReadWord(bus, address + 10));
        return CalculateBaseSpeed(entry);
    }

    private uint CalculateBaseSpeed(SpeedTableEntry entry)
    {

        if (AccelerationMode != 0)
        {
            // With a multiplier, the 65816 routine multiplies the whole deceleration word
            // for the high result but only the HIGH BYTE of decel_sub for the low result.
            // Combining the products as ordinary 16.16 multiplication would be cleaner,
            // but would not be what the cartridge executes.
            ushort deltaSpeed;
            ushort deltaSubspeed;
            if (DecelerationMultiplier != 0)
            {
                int highProduct = DecelerationMultiplier * entry.Deceleration;
                int lowProduct = DecelerationMultiplier * (entry.DecelerationSubspeed >> 8);
                deltaSpeed = unchecked((ushort)(highProduct >> 8));
                deltaSubspeed = unchecked((ushort)lowProduct);
            }
            else
            {
                deltaSpeed = entry.Deceleration;
                deltaSubspeed = entry.DecelerationSubspeed;
            }

            SetBaseFixed(unchecked(BaseFixed - Compose(deltaSpeed, deltaSubspeed)));

            // The native BMI tests only the signed high word. Once deceleration crosses
            // below zero it clears both halves and returns to acceleration mode zero.
            if (unchecked((short)BaseSpeed) < 0)
            {
                BaseSpeed = 0;
                BaseSubspeed = 0;
                AccelerationMode = 0;
            }
        }
        else
        {
            SetBaseFixed(unchecked(BaseFixed + Compose(entry.Acceleration, entry.AccelerationSubspeed)));

            // $90:9A7E does not use a normal unsigned 32-bit >= comparison. Preserve its
            // signed word-subtraction quirk, including the fact that exact equality does
            // not count as greater and therefore is left untouched.
            if (IsGreaterThanQuirked(
                    BaseSpeed,
                    BaseSubspeed,
                    entry.MaximumSpeed,
                    entry.MaximumSubspeed))
            {
                BaseSpeed = entry.MaximumSpeed;
                BaseSubspeed = entry.MaximumSubspeed;
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
        ISnesAddressSpace bus,
        SamusMovementType movementType)
    {
        SpeedTableEntry entry = ReadEntry(bus, movementType);

        if ((AccelerationMode & 1) != 0)
        {
            // This is byte-for-byte the same asymmetric multiplier calculation used by
            // the deceleration-allowed routine. The distinction between the two native
            // entry points is the bit test above and their carry result, not the subtract.
            ushort deltaSpeed;
            ushort deltaSubspeed;
            if (DecelerationMultiplier != 0)
            {
                int highProduct = DecelerationMultiplier * entry.Deceleration;
                int lowProduct = DecelerationMultiplier * (entry.DecelerationSubspeed >> 8);
                deltaSpeed = unchecked((ushort)(highProduct >> 8));
                deltaSubspeed = unchecked((ushort)lowProduct);
            }
            else
            {
                deltaSpeed = entry.Deceleration;
                deltaSubspeed = entry.DecelerationSubspeed;
            }

            SetBaseFixed(unchecked(BaseFixed - Compose(deltaSpeed, deltaSubspeed)));
            if (unchecked((short)BaseSpeed) < 0)
            {
                BaseSpeed = 0;
                BaseSubspeed = 0;
                AccelerationMode = 0;
            }

            // Every path through $90:9B5E-$90:9BC3 exits with carry clear.
            return new AerialBaseSpeedResult(BaseFixed, ReachedMaximum: false);
        }

        SetBaseFixed(unchecked(BaseFixed + Compose(entry.Acceleration, entry.AccelerationSubspeed)));

        // CMP/BMI performs signed 16-bit comparisons. Exact equality in both halves is
        // deliberately *not* considered a cap: $90:9B5A returns carry clear in that case.
        bool exceedsMaximum = unchecked((short)(BaseSpeed - entry.MaximumSpeed)) > 0 ||
            (BaseSpeed == entry.MaximumSpeed &&
             unchecked((short)(BaseSubspeed - entry.MaximumSubspeed)) > 0);
        if (exceedsMaximum)
        {
            BaseSpeed = entry.MaximumSpeed;
            BaseSubspeed = entry.MaximumSubspeed;
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
        uint total = unchecked(baseSpeed + Compose(ExtraRunSpeed, ExtraRunSubspeed));
        int shift = Math.Min(SpeedDivisor, (ushort)4);
        total >>= shift;
        TotalSpeed = unchecked((ushort)(total >> 16));
        TotalSubspeed = unchecked((ushort)total);
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
    public uint BaseFixed => Compose(BaseSpeed, BaseSubspeed);

    private void SetBaseFixed(uint value)
    {
        BaseSpeed = unchecked((ushort)(value >> 16));
        BaseSubspeed = unchecked((ushort)value);
    }

    private static bool IsGreaterThanQuirked(
        ushort valueHigh,
        ushort valueLow,
        ushort comparisonHigh,
        ushort comparisonLow)
    {
        if (unchecked((short)(valueHigh - comparisonHigh)) < 0)
            return false;

        return valueHigh != comparisonHigh ||
               (unchecked((short)(valueLow - comparisonLow)) >= 0 && valueLow != comparisonLow);
    }

    private static int ClampDisplacement(int displacement)
    {
        short high = unchecked((short)(displacement >> 16));
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

        return unchecked((int)(((uint)(ushort)high << 16) | low));
    }

    private static uint Compose(ushort high, ushort low) => ((uint)high << 16) | low;

    private static ushort ReadWord(ISnesAddressSpace bus, int address) =>
        unchecked((ushort)(bus.ReadByte(address) | (bus.ReadByte(address + 1) << 8)));
}

/// <summary>One native 12-byte <c>SamusSpeedTableEntry</c> from cartridge bank $90.</summary>
public readonly record struct SpeedTableEntry(
    ushort Acceleration,
    ushort AccelerationSubspeed,
    ushort MaximumSpeed,
    ushort MaximumSubspeed,
    ushort Deceleration,
    ushort DecelerationSubspeed)
{
    /// <summary>All six table fields are little-endian 16-bit words.</summary>
    public const int ByteCount = 12;
}

/// <summary>Value and 65816 carry returned by <c>$90:9B1F</c>.</summary>
public readonly record struct AerialBaseSpeedResult(uint Speed, bool ReachedMaximum);
