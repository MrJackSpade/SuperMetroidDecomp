namespace SuperMetroid.AssetExtraction.Enemies.Corpses;

/// <summary>Where the rotting corpses' row uploads land in the sprite characters (#1406, #1531).</summary>
internal static class CorpseSheetDefinitions
{
    /// <summary>
    /// The destination word the OBJ character table begins at in the corpses' row upload descriptors, whose rows land in
    /// that table: a row's table byte is its word's distance from this one, in bytes.
    /// </summary>
    internal const int ObjCharacterTableWord = 24576;
}
