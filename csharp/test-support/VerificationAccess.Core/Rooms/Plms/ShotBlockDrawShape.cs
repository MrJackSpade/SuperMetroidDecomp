namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>A shot block's first draw frame, its stride and the draw that restores it.</summary>
/// <param name="First">The first.</param>
/// <param name="Stride">The stride.</param>
/// <param name="Restore">The restore.</param>
internal readonly record struct ShotBlockDrawShape(ushort First, int Stride, ushort Restore);
