namespace SuperMetroid.Core.World.RoomStates;

/// <summary>One branch of a room's state-selection program: the state it offers when its condition holds.</summary>
/// <param name="Condition">The test the branch makes.</param>
/// <param name="State">The state selected when the test holds and no earlier branch matched.</param>
internal readonly record struct RoomStateBranch(RoomStateCondition Condition, RoomStateSettings State);
