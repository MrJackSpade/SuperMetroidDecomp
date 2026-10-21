namespace SuperMetroid.Core.Progression;

/// <summary>A bit of a progression bitfield.</summary>
/// <param name="ByteIndex">The byte index.</param>
/// <param name="BitMask">The bit mask.</param>
public readonly record struct ProgressionBit(int ByteIndex, int BitMask);
