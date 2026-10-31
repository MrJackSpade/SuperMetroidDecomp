using SuperMetroid.Core.Enemies.Corpses;

namespace SuperMetroid.TestSupport.VerificationImporters;

/// <summary>A decoded bank-$A9 corpse-rotting configuration record (#1472).</summary>
/// <param name="Kind">The kind of corpse whose row upload list the record names; none for the dead Torizo, which uploads by halves.</param>
/// <param name="RowCount">The corpse's height in pixel rows, each of which its rot schedule drops.</param>
/// <param name="RowLayoutTable">The bank-$A9 address of the character-row layout table the record names.</param>
/// <param name="Wrap">The bytes a row's fall out of a character's last two rows skips into the next character row.</param>
/// <param name="ScheduleHeapBytes">The rotting-heap bytes the cartridge keeps the corpse's rot schedule in.</param>
internal sealed record CorpseRottingConfiguration(
    RottingCorpseKind? Kind,
    int RowCount,
    ushort RowLayoutTable,
    int Wrap,
    CorpseScheduleHeapBytes ScheduleHeapBytes);
