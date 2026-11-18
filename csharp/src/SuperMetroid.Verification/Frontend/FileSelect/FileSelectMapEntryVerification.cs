using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Frontend.FileSelect;
namespace SuperMetroid.Verification.Frontend.FileSelect;
using SuperMetroid.Verification.Harness;

/// <summary>Checks the file-select map entry's black hold and opening window.</summary>
internal static class FileSelectMapEntryVerification
{
    /// <summary>
    /// Verifies the entry holds the screen black for twenty updates (#2415), the game's seventeen-update fade of colours
    /// that are never shown (#1512) and its three setup updates, before the exact twenty-eight-update reveal rectangle.
    /// </summary>
    internal static void VerifyFileSelectMapEntry()
    {
        var entry = new FileSelectMapEntry();
        AssertEqual(FileSelectMapEntryPhase.BlackHold, entry.Phase, "entry starts on its black hold");
        for (int tick = 1; tick < 20; tick++)
        {
            entry.Step();
            AssertEqual(FileSelectMapEntryPhase.BlackHold, entry.Phase, $"update {tick} keeps the screen black");
        }
        entry.Step();
        AssertEqual(FileSelectMapEntryPhase.Revealing, entry.Phase, "the twentieth update ends the black hold");
        for (int tick = 0; tick <= 27; tick++)
        {
            int left = 127 - 4 * tick, right = 129 + 4 * tick;
            int top = 111 - 4 * tick, bottom = 113 + 4 * tick;
            AssertEqual(left, entry.Left, "entry reveal window left edge");
            AssertEqual(right, entry.Right, "entry reveal window right edge (inclusive)");
            AssertEqual(top, entry.Top, "entry reveal window top edge");
            AssertEqual(bottom, entry.Bottom, "entry reveal window bottom edge (exclusive)");
            AssertTrue(!entry.IsComplete, "entry window remains active before signed margin underflow");
            entry.Step();
        }
        AssertTrue(entry.IsComplete, "twenty-eighth reveal update disables the window");
    }

    /// <summary>Runs the focused file-select map entry transition check.</summary>
    [VerificationCommand("--file-select-map-entry")]
    internal static int FileSelectMapEntryCommand()
    {
        Suite(nameof(VerifyFileSelectMapEntry), () => VerifyFileSelectMapEntry());
        return 0;
    }
}
