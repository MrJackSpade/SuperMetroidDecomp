using SuperMetroid.Core.Hardware;

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
    // kSamusSpeedTable_Normal_X begins at $90:9F49. BlockInsideReact_ShootableAir at
    // $94:97D0 writes base+12 ($9F55) to samus_x_speed_table_pointer. DetermineSpeedTable-
    // EntryPtr_X at $90:9BD1 then adds 12 * movement type. Running type 1 consequently
    // reads $90:9F61, not the superficially tempting $90:9F55 entry.
    public const ushort NormalSpeedTableAddress = 0x9f49;
    public const ushort NormalAirSpeedTableBaseAddress = NormalSpeedTableAddress + SpeedTableEntry.ByteCount;

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

    /// <summary>Contact-damage selector published when the counter reaches stage four.</summary>
    public ushort ContactDamageIndex { get; set; }

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
    /// Bank-$90 offset stored by the current inside-block reaction. Normal dry air selects
    /// <see cref="NormalAirSpeedTableBaseAddress"/>; water/lava and special blocks can
    /// replace it once those reactions are translated.
    /// </summary>
    public ushort ActiveSpeedTableBaseAddress { get; private set; } = NormalAirSpeedTableBaseAddress;

    /// <summary>
    /// Reproduces the ordinary-air assignment made by <c>$94:97D0</c> before movement.
    /// </summary>
    public void SelectNormalAirSpeedTable() =>
        ActiveSpeedTableBaseAddress = NormalAirSpeedTableBaseAddress;

    /// <summary>
    /// Ports the dry-air portion of <c>Handle_Samus_XExtraRunSpeed</c> at <c>$90:973E</c>.
    /// Both routes accelerate by hexadecimal <c>0.1000</c>. Ordinary Dash caps at
    /// <c>2.0000</c>; equipped Speed Booster caps at <c>7.0000</c> and initializes its
    /// counter through `$91:B61F`. Their shared momentum flag survives release and jumps.
    /// </summary>
    public void HandleExtraRunSpeed(
        byte movementType,
        ushort controllerInput,
        bool speedBoosterEquipped,
        ISnesAddressSpace? bus = null)
    {
        const ushort dashButton = 0x8000; // Retail default B/Dash binding.
        bool activelyDashing = movementType == 1 && (controllerInput & dashButton) != 0;
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
                SpeedBoostCounter = ReadWord(bus, 0x91b61f);
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
        byte movementType,
        ushort controllerInput,
        ushort animationFrameBuffer,
        ref ushort animationFrame,
        out ushort animationFrameTimer)
    {
        ArgumentNullException.ThrowIfNull(bus);
        animationFrameTimer = 0;
        const ushort dashButton = 0x8000;
        if (!HasRunningMomentum || movementType != 1 || (controllerInput & dashButton) == 0)
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
        ushort nextLowByte = ReadWord(bus, 0x91b61f + stage * 2);
        SpeedBoostCounter = unchecked((ushort)((SpeedBoostCounter & 0xff00) | nextLowByte));

        ushort delayList = ReadWord(bus, 0x91b5de + stage * 2);
        animationFrame = 0;
        animationFrameTimer = unchecked((ushort)(
            animationFrameBuffer + bus.ReadByte(0x910000 | delayList)));
        PublishBoostContactDamage();
        return true;
    }

    /// <summary>Reads one byte from the delay list selected by the counter's stage byte.</summary>
    public byte ReadSpeedBoosterAnimationByte(ISnesAddressSpace bus, ushort byteIndex)
    {
        ArgumentNullException.ThrowIfNull(bus);
        byte stage = unchecked((byte)(SpeedBoostCounter >> 8));
        ushort delayList = ReadWord(bus, 0x91b5de + stage * 2);
        int address = 0x910000 | unchecked((ushort)(delayList + byteIndex));
        return bus.ReadByte(address);
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
    /// Clears the locomotion-owned subset of <c>CancelSpeedBoost</c> at <c>$91:DE53</c>.
    /// Palette restoration and echo-projectile departure remain separate rendering work.
    /// </summary>
    public void CancelRunningMomentum()
    {
        if (HasRunningMomentum)
        {
            HasRunningMomentum = false;
            SpeedBoostCounter = 0;
            SpecialPaletteFrame = 0;
            SpecialPaletteTimer = 0;
        }
    }

    /// <summary>
    /// Resolves the exact 12-byte entry selected by <c>Samus_DetermineSpeedTableEntryPtr_X</c>
    /// at <c>$90:9BD1</c>, assuming the already-modeled inside-block reaction chose the base.
    /// </summary>
    public int ResolveEntryAddress(byte movementType)
    {
        ushort bankOffset = unchecked((ushort)(
            ActiveSpeedTableBaseAddress + SpeedTableEntry.ByteCount * movementType));
        return 0x900000 | bankOffset;
    }

    /// <summary>
    /// Reads a speed entry directly from cartridge bank $90. Fields remain split into the
    /// same high/low words as the original table instead of becoming floating-point values.
    /// </summary>
    public SpeedTableEntry ReadEntry(ISnesAddressSpace bus, byte movementType)
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
    public uint CalculateBaseSpeed(ISnesAddressSpace bus, byte movementType)
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
        byte movementType)
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
