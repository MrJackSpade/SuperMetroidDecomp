using SuperMetroid.Core.Enemies.Corpses;

namespace SuperMetroid.Core.Enemies;

/// <summary>
/// The enemies' shared staging state (#1406): the rotting heap's rot tables. Encounters that share it share its state, as
/// on the cartridge. The session's enemy binding owns one instance. Mother Brain stages her body on her own canvas (#1466).
/// </summary>
public sealed class EnemyStagingSurfaces
{
    /// <summary>The rot tables of the corpses rotting in the rotting heap.</summary>
    private readonly CorpseRotHeap rotHeap = new(EnemyStagingDefinitions.RottingHeapByteCount);

    /// <summary>Mother Brain's 48-entry rot table.</summary>
    public CorpseRotTable MotherBrainRotTable =>
        rotHeap.Table(EnemyStagingDefinitions.MotherBrainRotTableOffset, EnemyStagingDefinitions.MotherBrainRotTableEntryCount);

    /// <summary>
    /// The dead-monster rot table of <paramref name="entryCount"/> entries <paramref name="firstByte"/> bytes into the
    /// rotting heap, which it must lie inside.
    /// </summary>
    /// <exception cref="InvalidDataException">The table leaves the rotting heap.</exception>
    public CorpseRotTable RotTable(int firstByte, int entryCount) => rotHeap.Table(firstByte, entryCount);
}
