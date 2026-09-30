using System.Buffers.Binary;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;

internal static partial class Program
{
    private static void VerifyKraidLiveHeadAlias(EnemyTileArtworkCatalog art)
    {
        var fixture = new KraidHeadClockFixture(art);
        ushort[] bodyMap = fixture.State.BackgroundTilemapWords.ToArray();
        byte[] source = TilemapContractWords(KraidBackgroundRomData.HeadTilemapWords);
        const ushort livePointer = 0x1000;
        for (int index = 0; index < source.Length; index++)
            fixture.Memory.WriteByte(KraidBackgroundRomData.NativeBank | (livePointer + index), source[index]);
        byte[] ramBefore = fixture.Memory.WorkRam.ToArray(), expectedVram = fixture.Vram.Bytes.ToArray();
        var words = new ushort[source.Length / sizeof(ushort)];
        for (int word = 0; word < words.Length; word++)
            words[word] = BinaryPrimitives.ReadUInt16LittleEndian(source.AsSpan(word * 2));
        KraidHeadClockFixture.ExpectedUpload(expectedVram, words);
        fixture.Transfer(livePointer);
        AssertTrue(expectedVram.AsSpan().SequenceEqual(fixture.Vram.Bytes), "low-half head source reads actual live WRAM");
        AssertTrue(bodyMap.AsSpan().SequenceEqual(fixture.State.BackgroundTilemapWords), "live head alias leaves body map untouched");
        AssertTrue(ramBefore.AsSpan().SequenceEqual(fixture.Memory.WorkRam), "head DMA is read-only to source WRAM");
        AssertEqual(1, fixture.State.HeadTilemapUploadCount, "live alias retains one head upload");
        Console.WriteLine("PASS Kraid live head alias: exact WRAM-to-VRAM words, no body-map or source-RAM mutation.");
    }

    private static void VerifyKraidRequiredFiles(string directory)
    {
        IEnumerable<string> files = new[] { KraidBackgroundArtworkFormat.UpperFileName,
            KraidBackgroundArtworkFormat.LowerFileName, KraidBackgroundArtworkFormat.RoomBackgroundFileName }
            .Concat(KraidHeadInstructionDefinitions.All.ToArray().Where(command => command.Kind == KraidHeadInstructionKind.Frame)
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
