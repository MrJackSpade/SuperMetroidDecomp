namespace SuperMetroid.Core.Samus.Movement;

/// <summary>
/// The oracles' view of Samus's horizontal speed tables as bank-$90 addresses: the three movement-type tables are
/// adjacent, twelve-byte records from the normal-air table, and the standalone records precede them.
/// </summary>
internal static class SamusHorizontalSpeedStateAccess
{
    /// <summary><c>$90:9F49 kSamusSpeedTable_Normal_X</c>.</summary>
    private const ushort NormalSpeedTableWord = 0x9f49;

    /// <summary>The bank-$90 address of each standalone record.</summary>
    private static readonly Dictionary<SamusStandaloneSpeedRecord, int> StandaloneAddresses = new()
    {
        [SamusStandaloneSpeedRecord.DiagonalBombJump] = 0x909f25,
        [SamusStandaloneSpeedRecord.GrappleReleaseAir] = 0x909f31,
        [SamusStandaloneSpeedRecord.GrappleReleaseWater] = 0x909f3d,
        [SamusStandaloneSpeedRecord.GrappleReleaseLavaAcid] = 0x909f49,
    };

    /// <summary>The bank-$90 word of row zero, the first ordinary-air movement-type record after the standalone entry.</summary>
    private static ushort FirstRowWord => NormalSpeedTableWord + SpeedTableEntryConstants.ByteCount;

    /// <summary>The bank-$90 word of <paramref name="row"/>.</summary>
    private static ushort RowWord(int row) => unchecked((ushort)(FirstRowWord + SpeedTableEntryConstants.ByteCount * row));

    /// <summary>The medium whose table starts at <paramref name="word"/>.</summary>
    private static SamusLiquidMedium MediumAt(ushort word)
    {
        foreach (SamusLiquidMedium medium in Enum.GetValues<SamusLiquidMedium>())
            if (RowWord(SamusHorizontalMotionDefinitions.FirstRow(medium)) == word)
                return medium;
        throw new ArgumentOutOfRangeException(nameof(word), word, "The cartridge selects only the three movement-type tables.");
    }

    /// <summary>The native table words.</summary>
    extension(SamusMovementRomData.HorizontalMotion)
    {
        /// <summary><c>$90:9F49 kSamusSpeedTable_Normal_X</c>.</summary>
        internal static ushort NormalSpeedTable => NormalSpeedTableWord;

        /// <summary>The bank-$90 base of the ordinary-air table.</summary>
        internal static ushort NormalAirSpeedTable => RowWord(SamusHorizontalMotionDefinitions.FirstRow(SamusLiquidMedium.Air));

        /// <summary>The bank-$90 base selected below water without Gravity Suit.</summary>
        internal static ushort WaterSpeedTable => RowWord(SamusHorizontalMotionDefinitions.FirstRow(SamusLiquidMedium.Water));

        /// <summary>The bank-$90 base selected below lava/acid without Gravity Suit.</summary>
        internal static ushort LavaAcidSpeedTable => RowWord(SamusHorizontalMotionDefinitions.FirstRow(SamusLiquidMedium.LavaOrAcid));
    }

    /// <summary>Address-based resolution of authored records.</summary>
    extension(SamusHorizontalMotionDefinitions)
    {
        /// <summary>The bank-$90 address of <paramref name="record"/>.</summary>
        internal static int StandaloneAddressOf(SamusStandaloneSpeedRecord record) => StandaloneAddresses[record];

        /// <summary>Resolves a movement-type record by its bank-$90 address.</summary>
        internal static bool TryResolveIndexed(int address, out SpeedTableEntry entry)
        {
            int offset = address - (SamusMovementRomData.Banks.Movement | FirstRowWord);
            if (offset < 0 || offset % SpeedTableEntryConstants.ByteCount != 0)
            {
                entry = default;
                return false;
            }
            return SamusHorizontalMotionDefinitions.TryResolveRow(offset / SpeedTableEntryConstants.ByteCount, out entry);
        }

        /// <summary>Resolves a standalone record by its bank-$90 address.</summary>
        internal static bool TryResolveStandalone(int address, out SpeedTableEntry entry)
        {
            foreach ((SamusStandaloneSpeedRecord record, int recordAddress) in StandaloneAddresses)
            {
                if (recordAddress == address)
                {
                    entry = SamusHorizontalMotionDefinitions.Standalone(record);
                    return true;
                }
            }
            entry = default;
            return false;
        }
    }

    /// <summary>Address-based speed reads.</summary>
    extension(SamusHorizontalSpeedState)
    {
        /// <summary>The authored record at <paramref name="address"/>; any other address is not authored physics data.</summary>
        internal static SpeedTableEntry ReadEntryAtAddress(int address) =>
            SamusHorizontalMotionDefinitions.TryResolveIndexed(address, out SpeedTableEntry entry) ||
                SamusHorizontalMotionDefinitions.TryResolveStandalone(address, out entry)
                ? entry
                : throw new InvalidDataException($"Samus horizontal speed entry ${address:X6} is not an authored mechanics record.");
    }

    /// <summary>The selected table as its native base word, and entries as native addresses.</summary>
    extension(SamusHorizontalSpeedState self)
    {
        /// <summary>The bank-$90 base word of the selected table; setting it selects the table that starts there.</summary>
        internal ushort ActiveSpeedTableBaseAddress
        {
            get => RowWord(SamusHorizontalMotionDefinitions.FirstRow(self.ActiveSpeedTable));
            set => self.ActiveSpeedTable = MediumAt(value);
        }

        /// <summary>The bank-$90 address of the entry <paramref name="movementType"/> selects.</summary>
        internal int ResolveEntryAddress(SamusMovementType movementType) =>
            SamusMovementRomData.Banks.Movement | RowWord(self.ResolveEntryRow(movementType));

        /// <summary>Executes the base-speed calculation against the standalone record at <paramref name="address"/>.</summary>
        internal uint CalculateBaseSpeedAtAddress(int address)
        {
            foreach ((SamusStandaloneSpeedRecord record, int recordAddress) in StandaloneAddresses)
                if (recordAddress == address)
                    return self.CalculateBaseSpeed(record);
            throw new InvalidDataException($"Samus horizontal speed entry ${address:X6} is not an authored standalone record.");
        }
    }
}
