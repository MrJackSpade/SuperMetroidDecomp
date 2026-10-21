using SuperMetroid.Core.Enemies.Rios;

namespace SuperMetroid.Verification.Enemies.Rios;

/// <summary>The entry, calls, visible, signal and name of one lower norfair rio instruction program definitions program.</summary>
/// <param name="Entry">The entry.</param>
/// <param name="Calls">The calls.</param>
/// <param name="Visible">The visible.</param>
/// <param name="Signal">The signal.</param>
/// <param name="Name">The name.</param>
internal readonly record struct LowerNorfairRioInstructionProgramDefinitionsProgram(LowerNorfairRioProgram Entry, int Calls, bool Visible, bool Signal, string Name);
