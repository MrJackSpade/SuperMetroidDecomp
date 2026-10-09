using System.Reflection;

namespace SuperMetroid.Core.Game;

/// <summary>One compiled mechanics-owned instruction word at its native address.</summary>
/// <param name="Address">Bank-relative address where the mechanics word is read from the instruction program.</param>
/// <param name="Value">Duration, control opcode, or branch target compiled for that address.</param>
internal readonly record struct InstructionMechanicsWord(ushort Address, ushort Value);
