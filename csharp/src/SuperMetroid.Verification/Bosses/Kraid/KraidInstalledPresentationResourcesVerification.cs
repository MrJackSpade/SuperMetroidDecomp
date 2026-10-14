using System.Buffers.Binary;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Bosses.Kraid.KraidInstalledPresentationHeadClockVerification;
using static SuperMetroid.Verification.Rooms.TilemapJsonContractsVerification;
using SuperMetroid.Core.Bosses.Kraid;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Bosses.Kraid.Artwork;
using SuperMetroid.AssetExtraction.Enemies;

namespace SuperMetroid.Verification.Bosses.Kraid;

/// <summary>Checks Kraid live head tilemap DMA and required installed artwork files.</summary>
internal static class KraidInstalledPresentationResourcesVerification
{
    /// <summary>Confirms the low-bank alias reads live WRAM without altering body tilemap or source memory.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native VRAM words")]
    internal static void VerifyKraidLiveHeadAlias(EnemyTileArtworkCatalog art)
    {
        var fixture = new KraidHeadClockFixture(art);
        ushort[] bodyMap = fixture.State.BackgroundTilemapWords.ToArray();
        byte[] source = TilemapContractWords(KraidBackgroundRomData.HeadTilemapWords);
        const ushort livePointer = 0x1000;
        WriteKraidLowBankHeadTilemap(fixture.Memory, livePointer, source);
        byte[] ramBefore = fixture.Memory.WorkRam.ToArray(), expectedVram = fixture.Vram.Bytes.ToArray();
        var words = new ushort[source.Length / sizeof(ushort)];
        for (int word = 0; word < words.Length; word++)
            words[word] = BinaryPrimitives.ReadUInt16LittleEndian(source.AsSpan(word * 2));
        KraidHeadClockFixture.ExpectedUpload(expectedVram, words);
        fixture.Transfer(livePointer);
        AssertTrue(expectedVram.SequenceEqual(fixture.Vram.Bytes), "low-half head source reads actual live WRAM");
        AssertTrue(bodyMap.SequenceEqual(fixture.State.BackgroundTilemapWords), "live head alias leaves body map untouched");
        AssertTrue(ramBefore.SequenceEqual(fixture.Memory.WorkRam), "head DMA is read-only to source WRAM");
        AssertEqual(1, fixture.State.HeadTilemapUploadCount, "live alias retains one head upload");
        Console.WriteLine("PASS Kraid live head alias: exact WRAM-to-VRAM words, no body-map or source-RAM mutation.");
    }

    /// <summary>Seeds the native low-half WRAM mirror used as Kraid's head tilemap DMA source.</summary>
    [CpuBusBoundary("exact bus test: seeds Kraid's live head tilemap through the bank-$A7 low-half WRAM mirror")]
    private static void WriteKraidLowBankHeadTilemap(
        SuperMetroidAddressSpace memory, ushort pointer, ReadOnlySpan<byte> source)
    {
        for (int index = 0; index < source.Length; index++)
            memory.WriteByte(KraidBackgroundRomData.NativeBank | (pointer + index), source[index]);
    }

    /// <summary>Removes each required generated asset in turn and checks that validation and loading reject it.</summary>
    internal static void VerifyKraidRequiredFiles(string directory)
    {
        IEnumerable<string> files = new[] { KraidBackgroundArtworkFormat.UpperFileName,
            KraidBackgroundArtworkFormat.LowerFileName, KraidBackgroundArtworkFormat.RoomBackgroundFileName }
            .Concat(KraidHeadInstructionDefinitions.All.Where(command => command.Kind == KraidHeadInstructionKind.Frame)
                .Select(command => command.Tilemap).Distinct().Select(KraidBackgroundArtworkFormat.HeadFileName));
        int count = 0;
        foreach (string file in files)
        {
            string path = Path.Combine(directory, file);
            byte[] original = File.ReadAllBytes(path);
            // These are newly generated, fixture-owned assets. Restore each file
            // before the next independent missing-resource assertion.
            File.Delete(path);
            try
            {
                AssertThrows<FileNotFoundException>(() => EnemyTileArtworkFiles.ValidateStock(directory),
                    "missing Kraid file is not a valid stock installation: " + file);
                AssertThrows<FileNotFoundException>(() => EnemyTileArtworkFiles.Load(directory, null),
                    "missing Kraid file cannot silently load from ROM: " + file);
            }
            finally { File.WriteAllBytes(path, original); }
            count++;
        }
        AssertEqual(7, count, "complete Kraid body/head/backdrop file inventory");
        Console.WriteLine("PASS Kraid resources: all seven missing body/head/backdrop files rejected by validation and loading.");
    }
}
