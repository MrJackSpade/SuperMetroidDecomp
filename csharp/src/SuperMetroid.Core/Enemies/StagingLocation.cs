namespace SuperMetroid.Core.Enemies;

/// <summary>A byte offset within a VRAM staging buffer.</summary>
/// <param name="Buffer">The buffer.</param>
/// <param name="Offset">The offset.</param>
public readonly record struct StagingLocation(VramStagingBuffer Buffer, int Offset);
