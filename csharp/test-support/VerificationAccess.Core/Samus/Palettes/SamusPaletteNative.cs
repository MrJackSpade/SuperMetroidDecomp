using SuperMetroid.AssetExtraction.Samus;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Samus.Artwork;

namespace SuperMetroid.Core.Samus.Palettes;

/// <summary>
/// Verification's native view of Samus's suit, cycle and charge palettes: the bank-$9B address of each, as the
/// cartridge's bank-$91 lists store them.
/// </summary>
internal static class SamusPaletteNative
{
    /// <summary>Byte distance between one suit's cycle palettes and the next suit's.</summary>
    private const int SuitStride = 0x200;
    /// <summary>Byte distance between adjacent sixteen-color shades.</summary>
    private const int ShadeStride = 0x20;

    /// <summary>Every cycle palette the catalog installs.</summary>
    internal static IEnumerable<SamusFullBodyCyclePalette> All =>
        from family in Enum.GetValues<SamusFullBodyCycleFamily>()
        from suit in SamusSuitDefinitions.ListOrder
        from shade in Enumerable.Range(0, SamusFullBodyCycleColorFormat.ShadesPerSuit)
        select new SamusFullBodyCyclePalette(family, suit, shade);

    /// <summary>The bank-$9B address of <paramref name="palette"/>.</summary>
    internal static ushort Address(SamusFullBodyCyclePalette palette) => checked((ushort)(palette.Family switch
    {
        SamusFullBodyCycleFamily.SpeedBooster => 0x9b20,
        SamusFullBodyCycleFamily.StoredShine => 0x9ba0,
        SamusFullBodyCycleFamily.ActiveShinespark => 0x9c20,
        SamusFullBodyCycleFamily.ScrewAttack => 0x9ca0,
        _ => throw new ArgumentOutOfRangeException(nameof(palette)),
    } + palette.Suit.ListPlace() * SuitStride + palette.Shade * ShadeStride));

    /// <summary>Whether a cycle palette is stored at bank-$9B <paramref name="address"/>.</summary>
    internal static bool IsCyclePalette(int address) => All.Any(palette => Address(palette) == address);

    /// <summary>The cycle palette stored at bank-$9B <paramref name="address"/>.</summary>
    /// <exception cref="ArgumentOutOfRangeException">No cycle palette is stored there.</exception>
    internal static SamusFullBodyCyclePalette Palette(int address) =>
        All.Where(palette => Address(palette) == address).Select(palette => (SamusFullBodyCyclePalette?)palette).SingleOrDefault() ??
            throw new ArgumentOutOfRangeException(nameof(address), $"Uncatalogued full-body palette pointer ${address:X4}.");

    /// <summary>
    /// The bank-$9B address a bounded suit and phase select in a family's lists, mirroring later phases back toward the
    /// first shade for ping-pong cycles; false for offsets outside the compiled lists.
    /// </summary>
    private static bool TryCycle(ushort suitByteOffset, ushort phaseByteOffset, ushort lastPhaseByteOffset,
        SamusFullBodyCycleFamily family, PaletteCycleShape shape, out ushort pointer)
    {
        if (suitByteOffset > 4 || (suitByteOffset & 1) != 0 ||
            phaseByteOffset > lastPhaseByteOffset || (phaseByteOffset & 1) != 0)
        {
            pointer = 0;
            return false;
        }
        int phase = phaseByteOffset / 2;
        pointer = Address(new(family, SuitAtTableByteOffset(suitByteOffset),
            shape == PaletteCycleShape.PingPong ? Math.Min(phase, 6 - phase) : phase));
        return true;
    }

    /// <summary>The native suit-table byte offset of <paramref name="suit"/>: two bytes per suit, in list order.</summary>
    internal static ushort SuitTableByteOffset(SamusSuit suit) => checked((ushort)(suit.ListPlace() * sizeof(ushort)));

    /// <summary>The suit an even native suit-table byte offset selects.</summary>
    internal static SamusSuit SuitAtTableByteOffset(int byteOffset) => (byteOffset & 1) == 0
        ? SamusSuits.AtListPlace(byteOffset / sizeof(ushort))
        : throw new ArgumentOutOfRangeException(nameof(byteOffset), byteOffset, "An odd suit-table offset selects no suit.");

    /// <summary>The cycle step a native list byte offset selects; odd offsets straddle two entries.</summary>
    internal static int StepAtByteOffset(int byteOffset) => (byteOffset & 1) == 0
        ? byteOffset / sizeof(ushort)
        : throw new InvalidDataException($"List read at odd byte {byteOffset} straddles two entries and selects no cycle palette.");

    /// <summary>The bank-$9B address of a found cycle palette, or zero when none was found.</summary>
    private static bool Pointer(bool found, SamusFullBodyCyclePalette palette, out ushort pointer)
    {
        pointer = found ? Address(palette) : (ushort)0;
        return found;
    }

    /// <summary>
    /// The suit and cycle step a native suit-table byte offset and list byte offset name, or false when either is odd or
    /// the suit offset is past the table.
    /// </summary>
    private static bool TryNativeSuitAndStep(ushort suitByteOffset, ushort phaseByteOffset, out SamusSuit suit, out int step)
    {
        bool valid = suitByteOffset <= 4 && (suitByteOffset & 1) == 0 && (phaseByteOffset & 1) == 0;
        suit = valid ? SuitAtTableByteOffset(suitByteOffset) : default;
        step = phaseByteOffset / sizeof(ushort);
        return valid;
    }

    /// <summary>The normal suit palette the native suit table selects, by its bank-$9B address.</summary>
    extension(SamusPaletteDefinitions.Common)
    {
        /// <summary>$91:D727: the Power, Varia or Gravity palette pointer at an even suit-table byte offset.</summary>
        internal static ushort NormalSuitPalettePointer(ushort byteOffset) => byteOffset switch
        {
            0 => 0x9400,
            2 => 0x9520,
            4 => 0x9800,
            _ => throw new ArgumentOutOfRangeException(nameof(byteOffset)),
        };
    }

    /// <summary>The cycle lists by bank-$9B palette address.</summary>
    extension(SamusPaletteDefinitions.FullBodyCycles)
    {
        /// <summary>$91:D998: the special Super-palette flash's cycle palette pointer.</summary>
        internal static ushort SpeedBoostPalettePointer(ushort byteOffset) =>
            Address(SamusFullBodyCyclePalettes.SpeedBoostPalette(SuitAtTableByteOffset(byteOffset)));

        /// <summary>The Screw Attack palette pointer for a bounded suit and phase.</summary>
        internal static bool TryScrewAttackPalettePointer(ushort suitByteOffset, ushort phaseByteOffset, out ushort pointer) =>
            TryCycle(suitByteOffset, phaseByteOffset, 10, SamusFullBodyCycleFamily.ScrewAttack, PaletteCycleShape.PingPong,
                out pointer);

        /// <summary>The active Speed Booster palette pointer for a bounded suit and phase.</summary>
        internal static bool TryActiveSpeedBoosterPalettePointer(ushort suitByteOffset, ushort phaseByteOffset, out ushort pointer) =>
            TryCycle(suitByteOffset, phaseByteOffset, 6, SamusFullBodyCycleFamily.SpeedBooster, PaletteCycleShape.Linear,
                out pointer);

        /// <summary>The stored-shine palette pointer for a bounded suit and phase.</summary>
        internal static bool TryStoredShinePalettePointer(ushort suitByteOffset, ushort phaseByteOffset, out ushort pointer)
        {
            SamusFullBodyCyclePalette palette = default;
            bool found = TryNativeSuitAndStep(suitByteOffset, phaseByteOffset, out SamusSuit suit, out int step) &&
                SamusFullBodyCyclePalettes.TryStoredShinePalette(suit, step, out palette);
            return Pointer(found, palette, out pointer);
        }

        /// <summary>The active-shinespark palette pointer for a bounded suit and phase.</summary>
        internal static bool TryActiveShinesparkPalettePointer(ushort suitByteOffset, ushort phaseByteOffset, out ushort pointer)
        {
            SamusFullBodyCyclePalette palette = default;
            bool found = TryNativeSuitAndStep(suitByteOffset, phaseByteOffset, out SamusSuit suit, out int step) &&
                SamusFullBodyCyclePalettes.TryActiveShinesparkPalette(suit, step, out palette);
            return Pointer(found, palette, out pointer);
        }

        /// <summary>The Screw Attack list word read anywhere in its compiled allocation.</summary>
        internal static ushort ReadScrewAttackPalettePointer(ushort suitByteOffset, ushort phaseByteOffset) =>
            Address(SamusFullBodyCyclePalettes.ReadScrewAttackPalette(SuitAtTableByteOffset(suitByteOffset),
                StepAtByteOffset(phaseByteOffset)));

        /// <summary>The active Speed Booster list word read anywhere in its compiled allocation, or an overrun's operand.</summary>
        internal static ushort ReadActiveSpeedBoosterPalettePointer(ushort suitByteOffset, ushort phaseByteOffset)
        {
            SpeedBoosterPaletteSource source = SamusFullBodyCyclePalettes.ReadActiveSpeedBoosterPalette(
                SuitAtTableByteOffset(suitByteOffset), StepAtByteOffset(phaseByteOffset));
            return source.Overrun switch
            {
                SpeedBoosterPaletteOverrun.Expansion => 0x68ad,
                SpeedBoosterPaletteOverrun.GrappleCode => 0xc90a,
                null => Address(source.Palette ?? throw new InvalidOperationException("A source without an overrun names a palette.")),
                _ => throw new ArgumentOutOfRangeException(nameof(suitByteOffset)),
            };
        }

        /// <summary>The bank-$9B source of the ten Hyper Beam frames $91:D99E selects, descending from $9B:A360.</summary>
        internal static int HyperBeamPaletteSource(int frame) => (uint)frame < SamusPaletteDefinitions.FullBodyCycles.HyperBeamPaletteCount
            ? 0x9ba360 - frame * ShadeStride
            : throw new ArgumentOutOfRangeException(nameof(frame));
    }

    /// <summary>The cycle catalog's rows by bank-$9B palette address.</summary>
    extension(SamusFullBodyCycleColorFormat)
    {
        /// <summary>The bank-$9B address of a family's shade for a suit.</summary>
        internal static ushort Pointer(SamusFullBodyCycleFamily family, int suit, int shade) =>
            (uint)suit < SamusFullBodyCycleColorFormat.SuitCount && (uint)shade < SamusFullBodyCycleColorFormat.ShadesPerSuit
                ? Address(new(family, SamusSuits.AtListPlace(suit), shade))
                : throw new ArgumentOutOfRangeException(nameof(suit), "Suit and shade must be catalogued.");

        /// <summary>The artwork row of the cycle palette at bank-$9B <paramref name="pointer"/>.</summary>
        internal static int PaletteIndex(ushort pointer) => SamusFullBodyCycleColorFormat.PaletteIndex(Palette(pointer));
    }

    /// <summary>The import pipeline's decoded cycle colors by bank-$9B palette address.</summary>
    extension(InstalledSamusFullBodyCycleColors colors)
    {
        /// <summary>One color of the cycle palette at bank-$9B <paramref name="pointer"/>.</summary>
        internal RgbColor Resolve(ushort pointer, int colorIndex) => colors.Resolve(Palette(pointer), colorIndex);
    }

    /// <summary>Samus's cycle colors by bank-$9B palette address.</summary>
    extension(SamusFullBodyCycleColours colors)
    {
        /// <summary>One color of the cycle palette at bank-$9B <paramref name="pointer"/>.</summary>
        internal RgbColor Resolve(ushort pointer, int colorIndex) => colors.Resolve(Palette(pointer), colorIndex);

        /// <summary>Copies the cycle palette at bank-$9B <paramref name="pointer"/> to Samus's suit colors.</summary>
        internal void Apply(SamusColours colours, ushort pointer) => colors.Apply(colours, Palette(pointer));
    }

    /// <summary>The charge and Hyper-shot lists by bank-$9B palette address.</summary>
    extension(SamusChargePaletteDefinitions)
    {
        /// <summary>$91:D7D5: the charged-beam palette pointer for a suit and phase, shade sequence 0/1/2/3/2/1.</summary>
        internal static bool TryChargedBeam(ushort suitOffset, ushort phaseOffset, out ushort pointer)
        {
            if (!IsNativeChargePhase(suitOffset, phaseOffset))
            {
                pointer = 0;
                return false;
            }
            int phase = phaseOffset / sizeof(ushort);
            pointer = (ushort)(0x9820 + suitOffset / sizeof(ushort) * 0x100 + Math.Min(phase, 6 - phase) * ShadeStride);
            return true;
        }

        /// <summary>$91:D7FF: the pseudo-screw palette pointer, the active-shinespark shade three then the normal suit.</summary>
        internal static bool TryPseudoScrew(ushort suitOffset, ushort phaseOffset, out ushort pointer)
        {
            if (!IsNativeChargePhase(suitOffset, phaseOffset))
            {
                pointer = 0;
                return false;
            }
            pointer = phaseOffset / sizeof(ushort) < 3
                ? Address(new(SamusFullBodyCycleFamily.ActiveShinespark, SuitAtTableByteOffset(suitOffset), 3))
                : SamusPaletteDefinitions.Common.NormalSuitPalettePointer(suitOffset);
            return true;
        }

        /// <summary>Whether a native suit offset and charge byte offset name one of the six steps of a suit's list.</summary>
        private static bool IsNativeChargePhase(ushort suitOffset, ushort phaseOffset) =>
            suitOffset <= 4 && (suitOffset & 1) == 0 && (phaseOffset & 1) == 0 &&
            SamusChargePalettes.IsCompiledChargeStep(phaseOffset / sizeof(ushort));

        /// <summary>$91:D829: the Hyper-shot glow palette pointer, frames nine down to zero.</summary>
        internal static bool TryHyperShot(ushort tableOffset, out ushort pointer)
        {
            if (!SamusChargePalettes.IsCompiledHyperShot(tableOffset))
            {
                pointer = 0;
                return false;
            }
            int frame = (SamusChargePaletteDefinitions.LastHyperTableByteOffset - tableOffset) / sizeof(ushort);
            pointer = (ushort)(0xa240 + frame * ShadeStride);
            return true;
        }
    }
}
