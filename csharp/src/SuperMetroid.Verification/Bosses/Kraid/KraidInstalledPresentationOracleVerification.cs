using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.CartridgeTables;
using System.Buffers.Binary;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Bosses.Kraid;
using SuperMetroid.AssetExtraction.Bosses;
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
    internal static ushort[] ReadImportedKraidWorkingMap(CartridgeImportAddressSpace source)
    {
        byte[] upper = RomDataReader.Decompress(source, KraidSourceDefinitions.UpperBodyTilemap,
            KraidWorkingMapDefinitions.SourceTilemapBytes);
        byte[] lower = RomDataReader.Decompress(source, KraidSourceDefinitions.LowerBodyTilemap,
            KraidWorkingMapDefinitions.SourceTilemapBytes);
        var result = new ushort[KraidWorkingMapDefinitions.WorkingMapCells];
        for (int word = 0; word < result.Length; word++)
        {
            // Native copies only the first $600 lower bytes up to $2800. The
            // untouched $2E00-$2FFF tail still contains its decompressed lower data.
            int selected = word < KraidWorkingMapDefinitions.PageCells ? word :
                word < KraidWorkingMapDefinitions.PreservedLowerTailFirstWord ? word - KraidWorkingMapDefinitions.LowerPageFirstCell : word;
            byte[] bytes = word < KraidWorkingMapDefinitions.PageCells ? upper : lower;
            ushort value = BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(selected * 2));
            result[word] = word >= KraidWorkingMapDefinitions.BlankRowFirstCell
                ? KraidWorkingMapDefinitions.BlankTile.PackedWord : word >= KraidWorkingMapDefinitions.PreservedLowerTailFirstWord
                ? value : (ushort)(value & ~SnesBgTilemapWord.PriorityMask);
        }
        return result;
    }

    /// <summary>
    /// The BG2 tilemap one native head transfer shows over blank pages (#1352): $A7:AF5D sends the head source straight
    /// to the display without changing the body map that later rise/growth/sink requests upload from WRAM.
    /// </summary>
    internal static ReadImportedKraidHeadFrameResult ReadImportedKraidHeadFrame(
        CartridgeImportAddressSpace source, ushort pointer)
    {
        byte[] bytes = RomDataReader.ReadFixedBank(source, KraidSourceDefinitions.CodeBank | pointer,
            KraidWorkingMapDefinitions.HeadImageCells * sizeof(ushort));
        var head = new ushort[KraidWorkingMapDefinitions.HeadImageCells];
        for (int word = 0; word < head.Length; word++)
            head[word] = BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(word * 2));
        var state = new KraidEnemyState();
        return new ReadImportedKraidHeadFrameResult(state, KraidBodyFixtures.Tilemap(Blank(), head, Blank()));

        static ushort[] Blank() => Enumerable.Repeat(KraidWorkingMapDefinitions.BlankTile.PackedWord, KraidWorkingMapDefinitions.PageCells).ToArray();
    }

    /// <summary>Checks every typed head step and cursor read against the authored control words before the RAM-only clock fixture runs.</summary>
    internal static void VerifyKraidInstalledControlOracle(CartridgeImportAddressSpace source) =>
        KraidHeadCommandMappingVerification.VerifyKraidHeadCommandMapping(source);
}
