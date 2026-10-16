using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Bosses.Kraid.KraidInstalledPresentationHeadClockVerification;
using static SuperMetroid.Verification.Rooms.TilemapJsonContractsVerification;
using SuperMetroid.Core.Bosses.Kraid;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Bosses.Kraid.Artwork;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core;

namespace SuperMetroid.Verification.Bosses.Kraid;

/// <summary>Checks Kraid live head tilemap DMA and required installed artwork files.</summary>
internal static class KraidInstalledPresentationResourcesVerification
{
    /// <summary>
    /// Confirms a low-half head-tilemap pointer is rejected without touching VRAM or the body map: the live-WRAM
    /// alias is unreachable from authored head records and its path was deleted (#1318 stage 2).
    /// </summary>
    [PpuLayoutBoundary("exact hardware test: checks native VRAM words")]
    internal static void VerifyKraidLiveHeadAlias(EnemyTileArtworkCatalog art)
    {
        var fixture = new KraidHeadClockFixture(art);
        ushort[] bodyMap = fixture.State.BackgroundTilemapWords.ToArray();
        byte[] vramBefore = fixture.Vram.Bytes.ToArray();
        const ushort lowHalfPointer = 0x1000;
        AssertThrows<InvalidDataException>(() => fixture.Transfer(lowHalfPointer), "low-half head pointer is rejected");
        AssertTrue(vramBefore.SequenceEqual(fixture.Vram.Bytes), "a rejected head pointer uploads nothing");
        AssertTrue(bodyMap.SequenceEqual(fixture.State.BackgroundTilemapWords), "a rejected head pointer leaves the body map untouched");
        AssertEqual(0, fixture.State.HeadTilemapUploadCount, "a rejected head pointer counts no upload");
        Console.WriteLine("PASS Kraid low-half head pointer: rejected with no VRAM, body-map or upload change.");
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
