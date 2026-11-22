using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.AssetExtraction.CartridgeTables;

/// <summary>Reads the Samus projectile list layout from where the lists start (#1301, #1411).</summary>
internal static partial class SamusProjectileListEntries
{
    /// <summary>Reads the word just before every list and the timed records each stores, and lays the lists out.</summary>
    internal static SamusProjectileListLayout Read(IImportCartridgeSource cartridge) => new(SamusProjectileListEntryDefinitions.Entries.ToDictionary(
        entry => entry.Key,
        entry => new SamusProjectileListLayout.ListPosition(entry.Value, Word(cartridge, entry.Value - sizeof(ushort)), RecordCount(cartridge, entry.Value))));

    /// <summary>
    /// The timed records the list at <paramref name="entry"/> stores: records, each opened by its duration, which
    /// <see cref="OpensTimedRecord"/> tells from an instruction, until a goto or delete instruction, or until the next list's entry where the list runs on into it.
    /// </summary>
    private static int RecordCount(IImportCartridgeSource cartridge, int entry)
    {
        var entries = SamusProjectileListEntryDefinitions.Entries.Values.ToHashSet();
        int records = 0;
        for (int place = entry; (place == entry || !entries.Contains(checked((ushort)place))) && OpensTimedRecord(Word(cartridge, place));
             place += SamusProjectileListLayout.TimedRecordByteCount)
            records++;
        return records;
    }

    /// <summary>Whether <paramref name="word"/> opens a timed record, a duration, rather than a list's goto or delete instruction.</summary>
    private static bool OpensTimedRecord(int word) => !SamusProjectileListEntryDefinitions.EndInstructions.Contains(word);

    /// <summary>The bank-$93 word at <paramref name="address"/>.</summary>
    private static ushort Word(IImportCartridgeSource cartridge, int address) =>
        cartridge.ReadCartridgeWord(SamusSourceDefinitions.ProjectileDataBank.Value | unchecked((ushort)address));
}
