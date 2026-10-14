using System.Reflection;

namespace SuperMetroid.Core.Game;

/// <summary>One compiled mechanics-owned instruction word at its native address.</summary>
internal readonly record struct InstructionMechanicsWord(ushort Address, ushort Value);
