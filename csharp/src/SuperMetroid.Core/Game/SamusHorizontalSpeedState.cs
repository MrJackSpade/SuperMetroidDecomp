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

    /// <summary>Whole part of <c>samus_x_base_speed</c> at WRAM <c>$0B42</c>.</summary>
    public ushort BaseSpeed { get; set; }

    /// <summary>Fractional part of <c>samus_x_base_subspeed</c> at WRAM <c>$0B40</c>.</summary>
    public ushort BaseSubspeed { get; set; }

    /// <summary>Whole part of the run-button/speed-booster addition at WRAM <c>$0B3E</c>.</summary>
    public ushort ExtraRunSpeed { get; set; }

    /// <summary>Fractional part of the run-button/speed-booster addition at WRAM <c>$0B3C</c>.</summary>
    public ushort ExtraRunSubspeed { get; set; }

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
