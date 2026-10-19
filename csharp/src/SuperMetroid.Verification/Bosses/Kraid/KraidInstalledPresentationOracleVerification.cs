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
        byte[] upper = RomDataReader.Decompress(source, KraidBackgroundRomData.UpperTilemap,
            KraidBackgroundRomData.DecompressedTilemapBytes);
        byte[] lower = RomDataReader.Decompress(source, KraidBackgroundRomData.LowerTilemap,
            KraidBackgroundRomData.DecompressedTilemapBytes);
        var result = new ushort[KraidBackgroundRomData.WorkingTilemapWords];
        for (int word = 0; word < result.Length; word++)
        {
            // Native copies only the first $600 lower bytes up to $2800. The
            // untouched $2E00-$2FFF tail still contains its decompressed lower data.
            int selected = word < KraidBackgroundRomData.VisiblePageWords ? word :
                word < KraidBackgroundRomData.PreservedLowerTailFirstWord ? word - KraidBackgroundRomData.WorkingLowerHalfWord : word;
            byte[] bytes = word < KraidBackgroundRomData.VisiblePageWords ? upper : lower;
            ushort value = BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(selected * 2));
            result[word] = word >= KraidBackgroundRomData.BlankRowWorkingWord
                ? KraidBackgroundRomData.BlankTile : word >= KraidBackgroundRomData.PreservedLowerTailFirstWord
                ? value : (ushort)(value & ~KraidBackgroundRomData.PriorityBit);
        }
        return result;
    }

    /// <summary>
    /// The BG2 tilemap one native head transfer shows over blank pages (#1352): $A7:AF5D sends the head source straight
    /// to the display without changing the body map that later rise/growth/sink requests upload from WRAM.
    /// </summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for one Kraid head tilemap frame")]
    internal static (KraidEnemyState State, ushort[] Tilemap) ReadImportedKraidHeadFrame(
        CartridgeImportAddressSpace source, ushort pointer)
    {
        byte[] bytes = RomDataReader.ReadFixedBank(source, KraidBackgroundRomData.NativeBank | pointer,
            KraidBackgroundRomData.HeadTilemapWords * sizeof(ushort));
        var head = new ushort[KraidBackgroundRomData.HeadTilemapWords];
        for (int word = 0; word < head.Length; word++)
            head[word] = BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(word * 2));
        var state = new KraidEnemyState { HeadTilemapUploadCount = 1 };
        return (state, KraidBodyFixtures.Tilemap(Blank(), head, Blank()));

        static ushort[] Blank() => Enumerable.Repeat(KraidBackgroundRomData.BlankTile, KraidBackgroundRomData.VisiblePageWords).ToArray();
    }

    /// <summary>Checks every typed head step and cursor read against the authored control words before the RAM-only clock fixture runs.</summary>
    internal static void VerifyKraidInstalledControlOracle(CartridgeImportAddressSpace source) =>
        KraidHeadCommandMappingVerification.VerifyKraidHeadCommandMapping(source);
}
