using static SuperMetroid.Core.Game.InstructionItem;

namespace SuperMetroid.Core.Game;

/// <summary>One compiled command or duration word at its native bank-$A9 address.</summary>
/// <param name="Address">The bank-$A9 address where this mechanics word appears in the compiled body animation streams.</param>
/// <param name="Word">The raw 16-bit command or frame-duration value stored at <paramref name="Address"/>.</param>
internal readonly record struct MotherBrainBodyInstructionMechanicsWord(
    ushort Address,
    ushort Word);
