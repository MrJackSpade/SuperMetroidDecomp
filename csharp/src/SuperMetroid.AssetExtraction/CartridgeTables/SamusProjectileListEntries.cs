using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.AssetExtraction.CartridgeTables;

/// <summary>Reads the Samus projectile list layout from where the lists start (#1301, #1411).</summary>
internal static class SamusProjectileListEntries
{

    /// <summary>Reads the word just before every list and lays the lists out.</summary>
    internal static SamusProjectileListLayout Read(IImportCartridgeSource cartridge) => new(SamusProjectileListEntryDefinitions.Entries.ToDictionary(
        entry => entry.Key,
        entry => new SamusProjectileListPosition(entry.Value, cartridge.ReadCartridgeWord(
            SamusSourceDefinitions.ProjectileDataBank.Value | unchecked((ushort)(entry.Value - sizeof(ushort)))))));
}
