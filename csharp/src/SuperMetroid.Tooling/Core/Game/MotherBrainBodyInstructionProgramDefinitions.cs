using static SuperMetroid.Core.Game.InstructionItem;

namespace SuperMetroid.Core.Game;

/// <summary>One compiled command or duration word at its native bank-$A9 address.</summary>
internal readonly record struct MotherBrainBodyInstructionMechanicsWord(
    ushort Address,
    ushort Word);
