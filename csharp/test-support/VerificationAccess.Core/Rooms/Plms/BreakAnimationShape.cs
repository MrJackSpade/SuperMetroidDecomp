namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>A break animation's first draw frame and its stride.</summary>
/// <param name="First">The first.</param>
/// <param name="Stride">The stride.</param>
internal readonly record struct BreakAnimationShape(ushort First, int Stride);
