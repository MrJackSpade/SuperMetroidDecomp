using SuperMetroid.Core.Bosses.MotherBrain;

namespace SuperMetroid.Verification.Bosses.MotherBrain;

/// <summary>One of Mother Brain's room mutations and the program it is expected to run.</summary>
/// <param name="Mutation">The mutation spawned.</param>
/// <param name="Program">The program.</param>
internal readonly record struct Reachable(MotherBrainRoomMutation Mutation, MotherBrainFakeDeathProgram Program);
