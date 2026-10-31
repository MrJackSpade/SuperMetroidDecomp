using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Enemies;

/// <summary>
/// The cartridge's two enemy staging surfaces as bytes (#1406), for checks whose reference routines stage native data in
/// them.
/// </summary>
internal sealed class NativeEnemyStaging
{
    /// <summary>Group A, <c>$7E:2000-$2FFF</c>.</summary>
    internal readonly VramStagingBuffer Bg2 = new(EnemySourceDefinitions.EnemyBg2StagingBytes);

    /// <summary>Group B, <c>$7E:9000-$97FF</c>.</summary>
    internal readonly VramStagingBuffer RottingHeap = new(EnemySourceDefinitions.CorpseRottingHeapBytes);

    /// <summary>The 4bpp corpse tile surface the dead-monster and Dead Torizo rotting routines move pixel rows in.</summary>
    public StagingByteView CorpseTiles => Bg2.ByteView(0, Bg2.Length);

    /// <summary>Mother Brain's six-row corpse tile staging buffer, uploaded by <c>$A9:E1F4</c>.</summary>
    public StagingByteView MotherBrainCorpseTiles =>
        RottingHeap.ByteView(0, EnemyStagingDefinitionsConstants.MotherBrainCorpseTileByteCount);

    /// <summary>The Dead Torizo sand strips that <c>$A9:D69C</c> rewrites line by line.</summary>
    public StagingByteView DeadTorizoSand =>
        RottingHeap.ByteView(EnemySourceDefinitions.DeadTorizoSandOffset, EnemySourceDefinitions.DeadTorizoSandBytes);
}
