using SuperMetroid.AssetExtraction.CartridgeTables;
using System.Buffers.Binary;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Bosses.Kraid;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Verification.Bosses.Kraid;

/// <summary>Import-only Kraid BG2 and head-program reference data, kept
/// separate from the gameplay constructors and installed presentation path.</summary>
internal static class KraidInstalledPresentationOracleVerification
{
    /// <summary>
    /// Import-only expected image for $A7:AAC6/AB19. This deliberately does not
    /// invoke the production constructor or give its cartridge source to gameplay.
    /// </summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle decompressing the Kraid upper and lower BG2 tilemaps")]
    internal static ushort[] ReadImportedKraidWorkingMap(CartridgeImportAddressSpace source)
    {
        byte[] upper = RomDataReader.Decompress(source, KraidSourceDefinitions.UpperBodyTilemap,
            KraidBackgroundDefinitions.SourceTilemapBytes);
        byte[] lower = RomDataReader.Decompress(source, KraidSourceDefinitions.LowerBodyTilemap,
            KraidBackgroundDefinitions.SourceTilemapBytes);
        var result = new ushort[KraidBackgroundDefinitions.WorkingMapCells];
        for (int word = 0; word < result.Length; word++)
        {
            // Native copies only the first $600 lower bytes up to $2800. The
            // untouched $2E00-$2FFF tail still contains its decompressed lower data.
            int selected = word < KraidBackgroundDefinitions.PageCells ? word :
                word < KraidBackgroundDefinitions.PreservedLowerTailFirstWord ? word - KraidBackgroundDefinitions.LowerPageFirstCell : word;
            byte[] bytes = word < KraidBackgroundDefinitions.PageCells ? upper : lower;
            ushort value = BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(selected * 2));
            result[word] = word >= KraidBackgroundDefinitions.BlankRowFirstCell
                ? KraidBackgroundDefinitions.BlankTile : word >= KraidBackgroundDefinitions.PreservedLowerTailFirstWord
                ? value : (ushort)(value & ~SnesBgTilemapWord.PriorityMask);
        }
        return result;
    }

    /// <summary>
    /// The BG2 tilemap one native head transfer shows over blank pages (#1352): $A7:AF5D sends the head source straight
    /// to the display without changing the body map that later rise/growth/sink requests upload from WRAM.
    /// </summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for one Kraid head tilemap frame")]
    internal static ReadImportedKraidHeadFrameResult ReadImportedKraidHeadFrame(
        CartridgeImportAddressSpace source, ushort pointer)
    {
        byte[] bytes = RomDataReader.ReadFixedBank(source, KraidSourceDefinitions.CodeBank | pointer,
            KraidBackgroundDefinitions.HeadImageCells * sizeof(ushort));
        var head = new ushort[KraidBackgroundDefinitions.HeadImageCells];
        for (int word = 0; word < head.Length; word++)
            head[word] = BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(word * 2));
        var state = new KraidEnemyState();
        return new ReadImportedKraidHeadFrameResult(state, KraidBodyFixtures.Tilemap(Blank(), head, Blank()));

        static ushort[] Blank() => Enumerable.Repeat(KraidBackgroundDefinitions.BlankTile, KraidBackgroundDefinitions.PageCells).ToArray();
    }

    /// <summary>Checks every typed head step and cursor read against the authored control words before the RAM-only clock fixture runs.</summary>
    internal static void VerifyKraidInstalledControlOracle(CartridgeImportAddressSpace source) =>
        KraidHeadCommandMappingVerification.VerifyKraidHeadCommandMapping(source);
}
