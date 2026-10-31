using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;
using SuperMetroid.Core.Enemies.Corpses;

namespace SuperMetroid.TestSupport.VerificationImporters;

/// <summary>
/// The codec for a bank-$A9 corpse-rotting configuration record (#1472), for the checks that compare the corpse
/// definitions with the cartridge. It decodes the record into the typed values the game and the extraction keep, and the
/// rotting-heap bytes the cartridge places the corpse's rot schedule in, which only the check that no two coexisting
/// corpses share heap bytes reads.
/// </summary>
[EncodingOwner("corpse rotting configuration record")]
internal static class CorpseRottingConfigurationCodec
{
    /// <summary>The bank the records and the row layout tables they name lie in.</summary>
    private const int CorpseBank = 0xa90000;

    /// <summary>The bank-$7E address the rotting heap begins at.</summary>
    private const int RottingHeap = 0x9000;

    /// <summary>The heap bytes one row's (pixel row, delay) entry takes.</summary>
    private const int ScheduleEntryBytes = 4;

    /// <summary>The record word holding the rot schedule's bank-$7E address.</summary>
    private const int ScheduleWord = 0;

    /// <summary>The record word holding the row upload list's bank-$A9 address, or zero for none.</summary>
    private const int UploadListWord = 1;

    /// <summary>The record word holding the corpse's height in pixel rows.</summary>
    private const int RowCountWord = 4;

    /// <summary>The record word holding the bank-$A9 address of the corpse's character-row layout table.</summary>
    private const int RowLayoutWord = 6;

    /// <summary>The byte offset, in the row layout table, of the second character row's staged offset.</summary>
    private const int SecondRowOffsetByte = 2;

    /// <summary>The bytes the wrap is short of the second character row's staged offset: the planes-0/1 words of a character's six rows above its last two.</summary>
    private const int WrapCarryBack = 12;

    /// <summary>Decodes the configuration record at bank-$A9 <paramref name="recordAddress"/>.</summary>
    internal static CorpseRottingConfiguration Decode(ISnesAddressSpace rom, ushort recordAddress)
    {
        IImportCartridgeSource cartridge = CartridgeImportSource.Require(rom);
        int Word(int index) => cartridge.ReadCartridgeWord(CorpseBank | (recordAddress + index * 2));
        int uploadList = Word(UploadListWord);
        int rowLayout = Word(RowLayoutWord);
        int rowCount = Word(RowCountWord);
        int scheduleStart = Word(ScheduleWord) - RottingHeap;
        return new CorpseRottingConfiguration(
            Kind: uploadList == 0 ? null : CorpseNativeAddresses.TransferTableAt(checked((ushort)uploadList)),
            RowCount: rowCount,
            RowLayoutTable: checked((ushort)rowLayout),
            Wrap: cartridge.ReadCartridgeWord(CorpseBank | (rowLayout + SecondRowOffsetByte)) - WrapCarryBack,
            ScheduleHeapBytes: new CorpseScheduleHeapBytes(scheduleStart, scheduleStart + rowCount * ScheduleEntryBytes));
    }
}
