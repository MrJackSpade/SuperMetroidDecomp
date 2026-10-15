namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>One ordered visual BG2 write; native hitbox and timing data are absent.</summary>
internal readonly record struct EnemyBg2TilemapWrite(
    ushort DestinationWord, ReadOnlyMemory<ushort> Tiles);
