using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Verification.Bosses.MotherBrain;

/// <summary>The header and program of one reachable.</summary>
/// <param name="Header">The header.</param>
/// <param name="Program">The program.</param>
internal readonly record struct Reachable(PlmHeaderId Header, MotherBrainFakeDeathProgram Program);
