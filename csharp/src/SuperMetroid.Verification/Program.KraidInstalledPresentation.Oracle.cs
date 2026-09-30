using System.Buffers.Binary;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

internal static partial class Program
{
    /// <summary>
    /// Import-only expected image for $A7:AAC6/AB19. This deliberately does not
    /// invoke the production constructor or give its cartridge source to gameplay.
    /// </summary>
    private static KraidEnemyState ReadImportedKraidWorkingMap(CartridgeImportAddressSpace source)
    {
        byte[] upper = RomDataReader.Decompress(source, KraidBackgroundRomData.UpperTilemap,
            KraidBackgroundRomData.DecompressedTilemapBytes);
        byte[] lower = RomDataReader.Decompress(source, KraidBackgroundRomData.LowerTilemap,
            KraidBackgroundRomData.DecompressedTilemapBytes);
        var result = new KraidEnemyState { BackgroundTilemapsPrepared = true, OwnsBg2Tilemap = true };
        for (int word = 0; word < result.BackgroundTilemapWords.Length; word++)
        {
            // Native copies only the first $600 lower bytes up to $2800. The
            // untouched $2E00-$2FFF tail still contains its decompressed lower data.
            int selected = word < KraidBackgroundRomData.VisiblePageWords ? word :
                word < KraidBackgroundRomData.PreservedLowerTailFirstWord ? word - KraidBackgroundRomData.WorkingLowerHalfWord : word;
            byte[] bytes = word < KraidBackgroundRomData.VisiblePageWords ? upper : lower;
            ushort value = BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(selected * 2));
            result.BackgroundTilemapWords[word] = word >= KraidBackgroundRomData.BlankRowWorkingWord
                ? KraidBackgroundRomData.BlankTile : word >= KraidBackgroundRomData.PreservedLowerTailFirstWord
                ? value : (ushort)(value & ~KraidBackgroundRomData.PriorityBit);
        }
        return result;
    }

    private static (KraidEnemyState State, SnesVram Vram) ReadImportedKraidHeadFrame(
        CartridgeImportAddressSpace source, ushort pointer)
    {
        byte[] words = RomDataReader.ReadFixedBank(source, KraidBackgroundRomData.NativeBank | pointer,
            KraidBackgroundRomData.HeadTilemapWords * sizeof(ushort));
        var state = new KraidEnemyState { HeadTilemapUploadCount = 1 };
        // $A7:AF5D queues the head source directly to VRAM. It does not touch
        // the body map that later rise/growth/sink requests upload from WRAM.
        var vram = new SnesVram();
        vram.LoadBytes(KraidBackgroundRomData.LiveBg2TilemapWord * 2, words);
        return (state, vram);
    }

    /// <summary>Checks all 91 authored control words before the RAM-only clock fixture runs.</summary>
    private static void VerifyKraidInstalledControlOracle(CartridgeImportAddressSpace source)
    {
        ushort Word(int address) => BinaryPrimitives.ReadUInt16LittleEndian(RomDataReader.ReadFixedBank(source, address, 2));
        foreach (KraidHeadInstructionDefinition command in KraidHeadInstructionDefinitions.All)
        {
            int address = KraidBackgroundRomData.NativeBank | command.Pointer;
            if (command.Kind == KraidHeadInstructionKind.Frame)
            {
                AssertEqual(Word(address), command.Duration, "compiled Kraid duration matches pinned cartridge");
                AssertEqual(Word(address + 2), command.Tilemap, "compiled Kraid physical frame matches pinned cartridge");
                AssertEqual(Word(address + 4), command.VulnerableHitbox, "compiled Kraid outer mouth matches pinned cartridge");
                AssertEqual(Word(address + 6), command.InvulnerableHitbox, "compiled Kraid inner mouth matches pinned cartridge");
            }
            else if (command.Kind == KraidHeadInstructionKind.Terminate)
                AssertEqual(ushort.MaxValue, Word(address), "compiled Kraid native termination");
            else
            {
                // The two authored JSR targets load these sound immediates. Keep
                // their native addresses in this import oracle, not production logic.
                int callback = command.Kind == KraidHeadInstructionKind.RoarSound ? 0xaf94 : 0xaf9f;
                AssertEqual((ushort)callback, Word(address), "compiled Kraid native sound callback");
                AssertEqual(Word(KraidBackgroundRomData.NativeBank | (callback + 2)), command.SoundId,
                    "compiled Kraid native sound immediate");
            }
        }
    }
}
