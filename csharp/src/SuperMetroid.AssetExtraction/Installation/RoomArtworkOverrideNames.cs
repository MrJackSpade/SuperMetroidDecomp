using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.Core;

namespace SuperMetroid.AssetExtraction.Installation;

/// <summary>
/// Renames room artwork overrides from the names the installation used before #1363 and #1414, which carried the
/// cartridge source address, to the names of their character sheets, block tables, palettes, background tilemaps and
/// sky pages.
/// </summary>
internal static class RoomArtworkOverrideNames
{
    /// <summary>Moves each override still under its old name to its new one; a player who holds both must choose.</summary>
    /// <exception cref="IOException">An override exists under both its old and its new name.</exception>
    internal static void Migrate(GameInstallation installation)
    {
        Ensure.NotNull(installation);
        foreach (KeyValuePair<string, string> name in RoomTilesetFileNames.LegacyCharacterNames())
            Move(installation.RoomCharacterOverrideDirectory, name);
        foreach (KeyValuePair<string, string> name in RoomTilesetFileNames.LegacyBlockNames())
            Move(installation.RoomMetatileOverrideDirectory, name);
        foreach (KeyValuePair<string, string> name in RoomTilesetFileNames.LegacyPaletteNames())
            Move(installation.RoomPaletteOverrideDirectory, name);
        foreach (KeyValuePair<string, string> name in RoomBackgroundTilemapArtworkFiles.LegacyNames())
            Move(installation.RoomBackgroundTilemapOverrideDirectory, name);
        foreach (KeyValuePair<string, string> name in RoomSkyTilemapArtworkFiles.LegacyNames())
            Move(installation.RoomBackgroundTilemapOverrideDirectory, name);
    }

    /// <summary>Renames one override from its old name to its new one.</summary>
    private static void Move(string directory, KeyValuePair<string, string> name)
    {
        string legacy = Path.Combine(directory, name.Key);
        if (!File.Exists(legacy))
            return;
        string current = Path.Combine(directory, name.Value);
        if (File.Exists(current))
            throw new IOException($"Room artwork override {name.Value} also exists under its old name {name.Key}; keep one of them.");
        File.Move(legacy, current);
    }
}
