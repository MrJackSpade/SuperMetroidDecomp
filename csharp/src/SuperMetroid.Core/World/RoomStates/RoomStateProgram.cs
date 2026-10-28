namespace SuperMetroid.Core.World.RoomStates;

/// <summary>One room's state-selection program: its branches in native order, and the state none of them selects.</summary>
/// <param name="Default">The state selected when no branch's condition holds.</param>
/// <param name="Branches">The conditional branches, tested first to last.</param>
internal sealed record RoomStateProgram(RoomStateSettings Default, IReadOnlyList<RoomStateBranch> Branches);
