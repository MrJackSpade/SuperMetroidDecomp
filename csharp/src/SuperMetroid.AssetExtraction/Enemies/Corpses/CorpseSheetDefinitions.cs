using SuperMetroid.Core.Enemies;

namespace SuperMetroid.AssetExtraction.Enemies.Corpses;

/// <summary>The installed sheets the rotting corpses stage their characters from (#1406, #1412).</summary>
internal static class CorpseSheetDefinitions
{
    /// <summary>The dead Torizo's 192-tile sheet, used both for the initial corpse and falling sand.</summary>
    internal const EnemyDefinitionId DeadTorizoSheet = EnemyDefinitionId.CorpseTorizo;

    /// <summary>$1800 bytes, the full tile sheet named enemy-ed3f-tiles.png in the installation.</summary>
    internal const int DeadTorizoSheetByteCount = 6144;

    /// <summary>The sheet the dead Sidehopper, Zoomer, Ripper and Skree share: the dead Sidehopper's.</summary>
    internal const EnemyDefinitionId DeadMonsterSheet = EnemyDefinitionId.CorpseSidehopper;

    /// <summary>$0E00 bytes, the complete enemy-ed7f-tiles.png source sheet.</summary>
    internal const int DeadMonsterSheetByteCount = 3584;

    /// <summary>
    /// The destination word the OBJ character table begins at in the corpses' row upload descriptors, whose rows land in
    /// that table: a row's table byte is its word's distance from this one, in bytes.
    /// </summary>
    internal const int ObjCharacterTableWord = 24576;
}
