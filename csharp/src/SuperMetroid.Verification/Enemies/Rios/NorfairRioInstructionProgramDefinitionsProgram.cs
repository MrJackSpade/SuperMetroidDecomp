using SuperMetroid.Core.Enemies.Rios;

namespace SuperMetroid.Verification.Enemies.Rios;

/// <summary>The entry, calls, offset, signal and name of one norfair rio instruction program definitions program.</summary>
/// <param name="Entry">The entry.</param>
/// <param name="Calls">The calls.</param>
/// <param name="Offset">The offset.</param>
/// <param name="Signal">The signal.</param>
/// <param name="Name">The name.</param>
internal readonly record struct NorfairRioInstructionProgramDefinitionsProgram(NorfairRioProgram Entry, int Calls, short Offset, bool Signal, string Name);
