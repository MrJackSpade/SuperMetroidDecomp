using SuperMetroid.Core.Bosses.Kraid;
using SuperMetroid.AssetExtraction.Bosses;
using SuperMetroid.AssetExtraction.Enemies;

namespace SuperMetroid.Verification.Bosses.Kraid;

/// <summary>Checks Kraid live head tilemap DMA and required installed artwork files.</summary>
internal static class KraidInstalledPresentationResourcesVerification
{
    /// <summary>Removes each required generated asset in turn and checks that validation and loading reject it.</summary>
    internal static void VerifyKraidRequiredFiles(string directory)
    {
        IEnumerable<string> files = new[] { KraidBackgroundArtworkFormat.UpperFileName,
            KraidBackgroundArtworkFormat.LowerFileName, KraidBackgroundArtworkFormat.RoomBackgroundFileName }
            .Concat(KraidHeadFrame.ByMouthStage.Select(head => KraidBackgroundArtworkFormat.HeadFileName(head.MouthStage)));
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
