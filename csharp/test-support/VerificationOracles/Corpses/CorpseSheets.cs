using SuperMetroid.Core.Enemies;

namespace SuperMetroid.TestSupport.VerificationOracles;

/// <summary>
/// The installed sheets the character rot oracle stages the rotting corpses' characters from (#1406, #1412), which the
/// game no longer stages since it draws each corpse through its rows (#1531).
/// </summary>
internal static class CorpseSheets
{
    /// <summary>The dead Torizo's 192-tile sheet, used both for the initial corpse and falling sand.</summary>
    internal const EnemyDefinitionId DeadTorizoSheet = EnemyDefinitionId.CorpseTorizo;

    /// <summary>$1800 bytes, the full tile sheet named enemy-ed3f-tiles.png in the installation.</summary>
    internal const int DeadTorizoSheetByteCount = 6144;

    /// <summary>The sheet the dead Sidehopper, Zoomer, Ripper and Skree share: the dead Sidehopper's.</summary>
    internal const EnemyDefinitionId DeadMonsterSheet = EnemyDefinitionId.CorpseSidehopper;

    /// <summary>$0E00 bytes, the complete enemy-ed7f-tiles.png source sheet.</summary>
    internal const int DeadMonsterSheetByteCount = 3584;
}
