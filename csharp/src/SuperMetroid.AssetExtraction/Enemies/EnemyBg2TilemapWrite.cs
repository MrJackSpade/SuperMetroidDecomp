using SuperMetroid.Core.Assets;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>One ordered visual BG2 write; native hitbox and timing data are absent.</summary>
/// <param name="FirstCell">
/// Row-major index of the first tilemap cell written in the enemy BG2 working tilemap
/// (<see cref="EnemyBg2FrameLayout.TilemapWidth"/> entries per row); the writer rejects a run that leaves the buffer.
/// </param>
/// <param name="Tiles">The tiles placed from that cell onward, in order.</param>
internal readonly record struct EnemyBg2TilemapWrite(
    int FirstCell, ReadOnlyMemory<BackgroundTile> Tiles);
