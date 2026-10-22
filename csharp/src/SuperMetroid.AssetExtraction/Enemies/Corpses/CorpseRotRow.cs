namespace SuperMetroid.AssetExtraction.Enemies.Corpses;

/// <summary>A row of characters a rotting corpse uploads.</summary>
/// <param name="Index">The row's place among the corpse's rows.</param>
/// <param name="SourceAddress">The native work-memory address the cartridge uploads it from.</param>
/// <param name="ByteCount">Its bytes.</param>
/// <param name="FirstByte">Its first byte in the OBJ character table.</param>
internal sealed record CorpseRotRow(int Index, int SourceAddress, int ByteCount, int FirstByte);
