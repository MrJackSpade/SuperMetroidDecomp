namespace SuperMetroid.Core.Enemies;

/// <summary>
/// The size of the corpse-rotting heap, and where in it Mother Brain's rot table and Dead Torizo's sand strips sit. Where
/// the cartridge keeps the heap in work memory is the import's knowledge (#1412).
/// </summary>
[EncodingOwner("corpse heap sizes")]
internal static class EnemyStagingDefinitions
{
    /// <summary>The bytes of the corpse-rotting heap.</summary>
    internal const int RottingHeapByteCount = 0x800;

    /// <summary>Where Mother Brain's rot table starts in the heap.</summary>
    internal const int MotherBrainRotTableOffset = 0x700;

    /// <summary>Entries in Mother Brain's rot table.</summary>
    internal const int MotherBrainRotTableEntryCount = 48;

    /// <summary>Where Dead Torizo's sand strips start in the heap.</summary>
    internal const int DeadTorizoSandOffset = 0x500;

    /// <summary>The bytes Dead Torizo's sand strips span.</summary>
    internal const int DeadTorizoSandByteCount = 0x240;
}
