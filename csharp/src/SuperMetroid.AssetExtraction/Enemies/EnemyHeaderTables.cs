using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;
using SuperMetroid.Core.Enemies;
using static SuperMetroid.AssetExtraction.Enemies.EnemyHeaderTableDefinitions;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>Decodes the bank-$B4 item-drop and vulnerability tables an enemy header's words select.</summary>
[EncodingOwner("cartridge enemy drop and vulnerability tables")]
internal static class EnemyHeaderTables
{
    /// <summary>The item-drop chances a header word selects, or null for a zero word, which drops nothing.</summary>
    /// <exception cref="InvalidDataException">The word is not one of the authored tables.</exception>
    internal static EnemyDropChances? DropChances(IImportCartridgeSource cartridge, int word)
    {
        Ensure.NotNull(cartridge);
        if (word == 0)
            return null;
        int table = TableAddress(word, FirstDropChances, DropChancesSize, DropChancesCount, "item-drop");
        int Chance(int column) => cartridge.ReadCartridgeByte(table + column);
        return new EnemyDropChances(Chance(0), Chance(1), Chance(2), Chance(3), Chance(4), Chance(5));
    }

    /// <summary>
    /// The vulnerabilities a header word selects, or null for a zero word, with which the enemy takes the standard
    /// table's.
    /// </summary>
    /// <exception cref="InvalidDataException">The word is not one of the authored tables, or a byte is not a response.</exception>
    internal static EnemyVulnerabilities? Vulnerability(IImportCartridgeSource cartridge, int word)
    {
        Ensure.NotNull(cartridge);
        return word == 0 ? null : VulnerabilityAt(cartridge, word);
    }

    /// <summary>The vulnerability table at <paramref name="word"/>, which may be the standard table's own word.</summary>
    /// <exception cref="InvalidDataException">The word is not one of the authored tables, or a byte is not a response.</exception>
    internal static EnemyVulnerabilities VulnerabilityAt(IImportCartridgeSource cartridge, int word)
    {
        Ensure.NotNull(cartridge);
        int table = TableAddress(word, FirstVulnerability, VulnerabilitySize, VulnerabilityCount, "vulnerability");
        EnemyWeaponVulnerability Beam(int field) => BeamResponse(cartridge, table + field);
        EnemyWeaponVulnerability Other(int field) => WeaponResponse(cartridge, table + field);
        EnemyBeamFamilyVulnerability Family(int first) => new(Beam(first), Beam(first + 1), Beam(first + 2), Beam(first + 3));
        return new EnemyVulnerabilities(
            Power: Family(0),
            Spazer: Family(4),
            Plasma: Family(8),
            ChargedBeam: Other(19),
            Ordnance: new EnemyOrdnanceVulnerability(Other(12), Other(13), Other(14), Other(15)),
            Contact: new EnemyContactVulnerability(Other(16), Other(17), Other(18), Other(20)));
    }

    /// <summary>One beam-combination byte: the freeze marker, or a multiplier.</summary>
    private static EnemyWeaponVulnerability BeamResponse(IImportCartridgeSource cartridge, int address)
    {
        int entry = cartridge.ReadCartridgeByte(address);
        return entry == FreezeEntry ? EnemyWeaponVulnerability.Freezes : Multiplied(entry, address);
    }

    /// <summary>One byte of a weapon that is not a beam combination, which never freezes.</summary>
    private static EnemyWeaponVulnerability WeaponResponse(IImportCartridgeSource cartridge, int address)
    {
        int entry = cartridge.ReadCartridgeByte(address);
        return entry == FreezeEntry
            ? throw new InvalidDataException($"Vulnerability byte at ${address:X6} freezes outside a beam entry.")
            : Multiplied(entry, address);
    }

    /// <summary>
    /// A multiplier byte with its lethal-freeze bit. A multiplier outside the low nibble would make the charged beam's
    /// nibble differ from the other weapons' seven bits, and no authored byte has one.
    /// </summary>
    private static EnemyWeaponVulnerability Multiplied(int entry, int address)
    {
        int multiplier = entry & ~PreventsLethalFreezeBit;
        if ((multiplier & ~MultiplierBits) != 0)
            throw new InvalidDataException($"Vulnerability byte ${entry:X2} at ${address:X6} has a multiplier past the low nibble.");
        return new EnemyWeaponVulnerability(multiplier, (entry & PreventsLethalFreezeBit) != 0
            ? EnemyIceResponse.DiesOnLethalHit
            : EnemyIceResponse.FreezesOnLethalHit);
    }

    /// <summary>The long address of the table at <paramref name="word"/> among <paramref name="count"/> tables.</summary>
    private static int TableAddress(int word, int first, int size, int count, string kind)
    {
        int offset = word - first;
        if (offset < 0 || offset % size != 0 || offset / size >= count)
            throw new InvalidDataException($"Enemy {kind} word ${word:X4} is not one of the {count} aligned tables.");
        return TableBank | word;
    }
}
