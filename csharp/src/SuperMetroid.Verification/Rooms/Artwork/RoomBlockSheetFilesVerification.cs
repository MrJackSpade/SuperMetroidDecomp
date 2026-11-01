using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.TestSupport.RepositoryInstallation;
using SuperMetroid.Verification.Harness;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;

namespace SuperMetroid.Verification.Rooms.Artwork;

/// <summary>
/// Confirms the room block sheets compose to the installed files byte for byte: their block, tile word and BG3 sheets
/// and manifest, recomposed from the installed content they are composed from.
/// </summary>
internal static class RoomBlockSheetFilesVerification
{
    /// <summary>Recomposes the room block sheets into a scratch directory and compares each file with the installed one.</summary>
    internal static void VerifyRoomBlockSheetFiles()
    {
        DirectoryInfo scratch = Directory.CreateTempSubdirectory("sm-room-block-sheets-");
        try
        {
            string content = RepositoryInstallation.Installation.ContentDirectory;
            string installed = Path.Combine(content, GameInstallationLayout.RoomBlockSheetDirectoryName);
            string composed = Path.Combine(scratch.FullName, GameInstallationLayout.RoomBlockSheetDirectoryName);
            RoomBlockSheetFiles.Extract(content, composed, SupportedCartridge.Sha256);
            string[] expected = [.. Directory.GetFiles(installed).Select(Path.GetFileName).OfType<string>().Order(StringComparer.Ordinal)];
            string[] actual = [.. Directory.GetFiles(composed).Select(Path.GetFileName).OfType<string>().Order(StringComparer.Ordinal)];
            AssertTrue(expected.SequenceEqual(actual), "the recomposed room block sheets are the installed files");
            foreach (string name in expected)
            {
                AssertTrue(File.ReadAllBytes(Path.Combine(installed, name)).AsSpan().SequenceEqual(File.ReadAllBytes(Path.Combine(composed, name))),
                    $"room block sheet file {name} recomposes byte for byte");
            }
            Console.WriteLine($"Room block sheets: {expected.Length} files recompose byte for byte.");
        }
        finally
        {
            scratch.Delete(recursive: true);
        }
    }

    [VerificationCommand("--room-block-sheet-files")]
    internal static int RoomBlockSheetFilesCommand()
    {
        Suite(nameof(VerifyRoomBlockSheetFiles), VerifyRoomBlockSheetFiles);
        return 0;
    }
}
