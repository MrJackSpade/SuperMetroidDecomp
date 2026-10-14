namespace SuperMetroid.Core.Game;

/// <summary>One resolved Hyper Beam palette frame and its fixed control metadata.</summary>
/// <param name="Index">Zero-based frame number.</param>
/// <param name="Duration">Number of handler calls for which the frame remains active.</param>
/// <param name="NextInstructionPointer">Bank-$8D pointer reached after the done command.</param>
internal readonly record struct HyperBeamPaletteFxFrame(
    int Index,
    ushort Duration,
    ushort NextInstructionPointer);
