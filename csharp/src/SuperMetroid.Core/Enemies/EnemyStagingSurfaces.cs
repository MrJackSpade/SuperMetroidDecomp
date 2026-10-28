using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies.Corpses;

namespace SuperMetroid.Core.Enemies;

/// <summary>
/// The enemies' shared staging state (#1406): the enemy BG2 staging image, which Mother Brain stages her body in and the
/// Shitroid cutscene clears, and the rotting heap's rot tables. Encounters that share a surface share its state, as on the
/// cartridge. The session's enemy binding owns one instance.
/// </summary>
public sealed class EnemyStagingSurfaces
{
    /// <summary>The cells of the enemy BG2 staging image.</summary>
    private const int Bg2CellCount = EnemyStagingDefinitions.Bg2ByteCount / sizeof(ushort);

    /// <summary>The enemy BG2 staging image's tiles, which Mother Brain's body and the extended-tilemap writer (<c>$A0:96CA</c>) fill.</summary>
    public BackgroundTile[] EnemyBg2Tilemap { get; } = new BackgroundTile[Bg2CellCount];

    /// <summary>The rot tables of the corpses rotting in the rotting heap.</summary>
    private readonly CorpseRotHeap rotHeap = new(EnemyStagingDefinitions.RottingHeapByteCount);

    /// <summary>Clears the whole enemy BG2 staging image to tile zero, as the Shitroid cutscene's initialization does.</summary>
    public void ClearEnemyBg2Tilemap() => Array.Clear(EnemyBg2Tilemap);

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
