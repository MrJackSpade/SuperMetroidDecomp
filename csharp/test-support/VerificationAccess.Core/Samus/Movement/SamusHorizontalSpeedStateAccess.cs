using SuperMetroid.AssetExtraction.CartridgeTables;
namespace SuperMetroid.Core.Samus.Movement;

/// <summary>
/// The oracles' view of Samus's horizontal speeds as the cartridge stores them in bank $90 (#1734): three adjacent lists
/// of twelve-byte records, air then water then lava/acid, each indexed by the movement byte from the list's start, after
/// the standalone diagonal bomb jump and grapple release records. Core keys the speeds by medium and movement; only the
/// checks know this layout.
/// </summary>
internal static class SamusHorizontalSpeedStateAccess
{
    /// <summary><c>$90:9F49 kSamusSpeedTable_Normal_X</c>.</summary>
    private const ushort NormalSpeedTableWord = 0x9f49;

    /// <summary>The air list's records: it ends after the damage boost's, movement byte 25.</summary>
    private const int AirRecordCount = 26;

    /// <summary>The water and lava/acid lists' records, which add Draygon's hold and special movement.</summary>
    private const int LiquidRecordCount = 28;

    /// <summary>The bank-$90 address of each standalone record and the speeds Core gives it.</summary>
    private static readonly Dictionary<int, SamusHorizontalSpeeds> StandaloneRecords = new()
    {
        [0x909f25] = SamusHorizontalSpeedDefinitions.DiagonalBombJump,
        [0x909f31] = SamusHorizontalSpeedDefinitions.GrappleRelease,
        [0x909f3d] = SamusHorizontalSpeedDefinitions.GrappleRelease,
        [0x909f49] = SamusHorizontalSpeedDefinitions.GrappleRelease,
    };

    /// <summary>The bank-$90 word of the air list's first record, after the last standalone record.</summary>
    private static ushort AirListWord => NormalSpeedTableWord + SpeedTableEntryConstants.ByteCount;

    /// <summary>The bank-$90 word of <paramref name="medium"/>'s list.</summary>
    private static ushort ListWord(SamusLiquidMedium medium) => medium switch
    {
        SamusLiquidMedium.Air => AirListWord,
        SamusLiquidMedium.Water => (ushort)(AirListWord + SpeedTableEntryConstants.ByteCount * AirRecordCount),
        SamusLiquidMedium.LavaOrAcid => (ushort)(AirListWord + SpeedTableEntryConstants.ByteCount * (AirRecordCount + LiquidRecordCount)),
        _ => throw new ArgumentOutOfRangeException(nameof(medium), medium, "The cartridge has three speed lists."),
    };

    /// <summary>The medium whose list starts at <paramref name="word"/>.</summary>
    private static SamusLiquidMedium MediumAt(ushort word)
    {
        foreach (SamusLiquidMedium medium in Enum.GetValues<SamusLiquidMedium>())
            if (ListWord(medium) == word)
                return medium;
        throw new ArgumentOutOfRangeException(nameof(word), word, "The cartridge selects only the three movement-type lists.");
    }

    /// <summary>
    /// The bank-$90 address the cartridge reads <paramref name="movementType"/>'s speeds from in
    /// <paramref name="medium"/>: the list's start plus the movement byte's records, unbounded, so the air list's
    /// Draygon hold and special movement land in the water list.
    /// </summary>
    internal static int RecordAddress(SamusLiquidMedium medium, SamusMovementType movementType) =>
        SamusSourceDefinitions.MovementBank.Value |
            (ListWord(medium) + SpeedTableEntryConstants.ByteCount * SamusMovementTypeNativeWords.Types.Word(movementType));

    /// <summary>The native list words.</summary>
    extension(SamusMovementDefinitions.HorizontalMotion)
    {
        /// <summary><c>$90:9F49 kSamusSpeedTable_Normal_X</c>.</summary>
        internal static ushort NormalSpeedTable => NormalSpeedTableWord;

        /// <summary>The bank-$90 base of the ordinary-air list.</summary>
        internal static ushort NormalAirSpeedTable => ListWord(SamusLiquidMedium.Air);

        /// <summary>The bank-$90 base selected below water without Gravity Suit.</summary>
        internal static ushort WaterSpeedTable => ListWord(SamusLiquidMedium.Water);

        /// <summary>The bank-$90 base selected below lava/acid without Gravity Suit.</summary>
        internal static ushort LavaAcidSpeedTable => ListWord(SamusLiquidMedium.LavaOrAcid);
    }

    /// <summary>Address-based resolution of the standalone records.</summary>
    extension(SamusHorizontalSpeedDefinitions)
    {
        /// <summary>The bank-$90 address of every standalone record: the diagonal bomb jump's, then the three grapple releases'.</summary>
        internal static IEnumerable<int> StandaloneAddresses => StandaloneRecords.Keys;

        /// <summary>The speeds Core gives the standalone record at <paramref name="address"/>.</summary>
        internal static bool TryResolveStandalone(int address, out SamusHorizontalSpeeds speeds) =>
            StandaloneRecords.TryGetValue(address, out speeds);
    }

    /// <summary>The selected medium as its native list word, and speeds as native addresses.</summary>
    extension(SamusHorizontalSpeedState self)
    {
        /// <summary>The bank-$90 base word of the selected medium's list; setting it selects the medium whose list starts there.</summary>
        internal ushort ActiveSpeedTableBaseAddress
        {
            get => ListWord(self.SpeedMedium);
            set => self.SpeedMedium = MediumAt(value);
        }

        /// <summary>The bank-$90 address the cartridge reads <paramref name="movementType"/>'s speeds from in the selected medium.</summary>
        internal int ResolveEntryAddress(SamusMovementType movementType) => RecordAddress(self.SpeedMedium, movementType);

        /// <summary>Executes the base-speed calculation against the standalone record at <paramref name="address"/>.</summary>
        internal uint CalculateBaseSpeedAtAddress(int address) =>
            StandaloneRecords.TryGetValue(address, out SamusHorizontalSpeeds speeds)
                ? self.CalculateBaseSpeed(speeds).RawValue
                : throw new InvalidDataException($"Samus horizontal speed entry ${address:X6} is not an authored standalone record.");
    }
}
