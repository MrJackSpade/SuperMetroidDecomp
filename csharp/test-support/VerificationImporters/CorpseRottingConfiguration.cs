using SuperMetroid.AssetExtraction.Enemies.Corpses;

namespace SuperMetroid.TestSupport.VerificationImporters;

/// <summary>A decoded bank-$A9 corpse-rotting configuration record (#1472).</summary>
/// <param name="UploadList">The row upload list the record names; none for the dead Torizo, which uploads by halves.</param>
/// <param name="RowCount">The corpse's height in pixel rows, each of which its rot schedule drops.</param>
/// <param name="ScheduleHeapBytes">The rotting-heap bytes the cartridge keeps the corpse's rot schedule in.</param>
internal sealed record CorpseRottingConfiguration(
    CorpseRowUploadList? UploadList,
    int RowCount,
    CorpseScheduleHeapBytes ScheduleHeapBytes);
