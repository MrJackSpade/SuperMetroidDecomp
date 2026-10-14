/// <summary>
/// One converted gameplay update from <c>tools/convert-smv-updates.py</c>. A null
/// <see cref="ExpectedRecord"/> marks a dispatch that jumped into CommonBootSection: the
/// next dispatch ran under the same controller read, so no native checkpoint completes it.
/// </summary>
internal readonly record struct ConvertedMovieUpdate(
    int Update, int SourceFrame, ushort Input, string Kind, string TimingClass,
    int? ExpectedRecord, int ExcludedNmiAfter, ushort? HardwareWaitLatch, int? DoorLoaderCompletedEnemySlots,
    int? MessageBoxStartFrame, int? MessageBoxEndFrame);
