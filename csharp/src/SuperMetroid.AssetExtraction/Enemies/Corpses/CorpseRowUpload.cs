namespace SuperMetroid.AssetExtraction.Enemies.Corpses;

/// <summary>
/// One cartridge descriptor of a corpse row upload (#1406): the bytes the cartridge uploads after a rot step and the
/// destination word naming where in the OBJ characters they land. The extraction decodes it to find the characters a
/// corpse's rows show in (#1531).
/// </summary>
/// <param name="SizeInBytes">The bytes the row uploads.</param>
/// <param name="DestinationWord">The descriptor's destination word.</param>
internal readonly record struct CorpseRowUpload(int SizeInBytes, int DestinationWord)
{
    /// <summary>The OBJ character table byte the row lands at.</summary>
    internal int FirstTableByte => (DestinationWord - CorpseSheetDefinitions.ObjCharacterTableWord) * sizeof(ushort);
}
