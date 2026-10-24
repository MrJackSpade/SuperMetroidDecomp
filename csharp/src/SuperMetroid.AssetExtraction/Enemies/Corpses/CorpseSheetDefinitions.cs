namespace SuperMetroid.AssetExtraction.Enemies.Corpses;

/// <summary>The cartridge identities of the installed sheets the rotting corpses stage their characters from (#1406).</summary>
internal static class CorpseSheetDefinitions
{
    /// <summary>$B7:A800, the dead Torizo's 192-tile source sheet, used both for the initial corpse and falling sand.</summary>
    internal const int DeadTorizoSheetAddress = 0xb7a800;

    /// <summary>$1800 bytes, the full tile sheet named enemy-ed3f-tiles.png in the installation.</summary>
    internal const int DeadTorizoSheetByteCount = 0x1800;

    /// <summary>$B7:C000, the sheet shared by the dead Sidehopper, Zoomer, Ripper and Skree.</summary>
    internal const int DeadMonsterSheetAddress = 0xb7c000;

    /// <summary>$0E00 bytes, the complete enemy-ed7f-tiles.png source sheet.</summary>
    internal const int DeadMonsterSheetByteCount = 0x0e00;

    /// <summary>
    /// The destination word the OBJ character table begins at in the corpses' row upload descriptors, whose rows land in
    /// that table: a row's table byte is its word's distance from this one, in bytes.
    /// </summary>
    internal const int ObjCharacterTableWord = 0x6000;
}
