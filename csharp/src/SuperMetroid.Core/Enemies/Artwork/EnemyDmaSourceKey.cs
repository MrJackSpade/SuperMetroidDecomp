namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>The source and length of an enemy tile DMA transfer.</summary>
/// <param name="Source">The source.</param>
/// <param name="ByteCount">The byte count.</param>
public readonly record struct EnemyDmaSourceKey(int Source, int ByteCount);
