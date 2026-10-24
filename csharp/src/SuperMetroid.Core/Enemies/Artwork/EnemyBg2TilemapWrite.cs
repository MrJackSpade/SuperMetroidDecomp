namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>One ordered visual BG2 write; native hitbox and timing data are absent.</summary>
/// <param name="FirstCell">
/// Row-major index of the first tilemap cell written in the enemy BG2 working tilemap
/// (<see cref="EnemyBg2FrameLayout.TilemapWidth"/> entries per row); the writer rejects a run that leaves the buffer.
/// </param>
/// <param name="Tiles">The tilemap entries written from that index onward, in order.</param>
internal readonly record struct EnemyBg2TilemapWrite(
    int FirstCell, ReadOnlyMemory<ushort> Tiles);
