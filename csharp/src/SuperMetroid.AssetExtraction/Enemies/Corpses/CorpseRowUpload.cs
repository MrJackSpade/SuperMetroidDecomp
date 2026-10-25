using SuperMetroid.Core.Hardware;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.AssetExtraction.Enemies.Corpses;

/// <summary>
/// One cartridge descriptor of a corpse row upload (#1406): the bytes the cartridge uploads after a rot step, the
/// work-memory bank word and offset it reads them from, and the destination word naming where in the OBJ characters they
/// land. The extraction decodes it to picture each rot step a corpse's frames show; the game selects the step instead.
/// </summary>
/// <param name="SizeInBytes">The bytes the row uploads.</param>
/// <param name="SourceBankWord">The descriptor's bank word, whose high byte is the work-memory bank.</param>
/// <param name="SourceOffset">The work-memory offset the row is read from.</param>
/// <param name="DestinationWord">The descriptor's destination word.</param>
internal readonly record struct CorpseRowUpload(int SizeInBytes, int SourceBankWord, int SourceOffset, int DestinationWord)
{
    /// <summary>The work-memory address the row is read from: the bank word's bank byte with the offset.</summary>
    internal int SourceAddress => (int)new SnesAddress(SnesLittleEndian.HighByte(SourceBankWord), checked((ushort)SourceOffset));

    /// <summary>The OBJ character table byte the row lands at.</summary>
    internal int FirstTableByte => (DestinationWord - CorpseSheetDefinitions.ObjCharacterTableWord) * sizeof(ushort);
}
