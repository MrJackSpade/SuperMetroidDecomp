using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Frontend.FileSelect;
namespace SuperMetroid.Verification.Frontend.FileSelect;
using SuperMetroid.Verification.Harness;

/// <summary>Checks the file-select map entry's fade length, transfer boundaries, and opening window.</summary>
internal static class FileSelectMapEntryVerification
{
    /// <summary>
    /// Verifies the entry's fade lasts sixteen gradual steps, the setup's own call having taken the first, before the
    /// exact twenty-eight-update reveal rectangle. The fade's colours are never shown (#1512), so only its length is checked.
    /// </summary>
    internal static void VerifyFileSelectMapEntry()
    {
        var entry = new FileSelectMapEntry();
        AssertEqual(FileSelectMapEntryPhase.PaletteFade, entry.Phase, "entry starts in its forced-black palette fade");
        for (int tick = 1; tick <= 16; tick++)
        {
            entry.Step();
            AssertEqual(FileSelectMapEntryPhase.PaletteFade, entry.Phase, $"gradual step {tick} keeps the entry fading");
        }
        entry.Step();
        AssertEqual(FileSelectMapEntryPhase.LoadForeground, entry.Phase, "seventeenth call completes gradual palette fade");
        entry.Step();
        AssertEqual(FileSelectMapEntryPhase.LoadBackground, entry.Phase, "foreground transfer has its own entry boundary");
        entry.Step();
        AssertEqual(FileSelectMapEntryPhase.SetupWindow, entry.Phase, "background transfer has its own entry boundary");
        entry.Step();
        AssertEqual(FileSelectMapEntryPhase.Revealing, entry.Phase, "window setup precedes first expansion update");
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
