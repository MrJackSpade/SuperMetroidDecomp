using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Enemies.Corpses;

namespace SuperMetroid.Core.Enemies;

/// <summary>
/// The cartridge's two enemy staging surfaces as bytes (#1406), for checks whose reference routines stage native data in
/// them, alongside the typed staging state the game keeps.
/// </summary>
internal sealed class NativeEnemyStaging
{
    /// <summary>Group A, <c>$7E:2000-$2FFF</c>.</summary>
    internal readonly VramStagingBuffer Bg2 = new(EnemyStagingDefinitions.Bg2ByteCount);

    /// <summary>Group B, <c>$7E:9000-$97FF</c>.</summary>
    internal readonly VramStagingBuffer RottingHeap = new(EnemyStagingDefinitions.RottingHeapByteCount);

    /// <summary>The game's typed staging state, whose rot tables the reference routines rot.</summary>
    internal EnemyStagingSurfaces Surfaces { get; } = new();

    /// <summary>The 4bpp corpse tile surface the dead-monster and Dead Torizo rotting routines move pixel rows in.</summary>
    public StagingByteView CorpseTiles => Bg2.ByteView(0, Bg2.Length);

    /// <summary>Mother Brain's six-row corpse tile staging buffer, uploaded by <c>$A9:E1F4</c>.</summary>
    public StagingByteView MotherBrainCorpseTiles =>
        RottingHeap.ByteView(0, EnemyStagingDefinitionsConstants.MotherBrainCorpseTileByteCount);

    /// <summary>The Dead Torizo sand strips that <c>$A9:D69C</c> rewrites line by line.</summary>
    public StagingByteView DeadTorizoSand =>
        RottingHeap.ByteView(EnemyStagingDefinitions.DeadTorizoSandOffset, EnemyStagingDefinitions.DeadTorizoSandByteCount);

    /// <summary>Mother Brain's 48-entry rot table.</summary>
    public CorpseRotTable MotherBrainRotTable => Surfaces.MotherBrainRotTable;

    /// <summary>The dead-monster rot table of <paramref name="entryCount"/> entries <paramref name="firstByte"/> bytes into the rotting heap.</summary>
    public CorpseRotTable RotTable(int firstByte, int entryCount) => Surfaces.RotTable(firstByte, entryCount);
}
