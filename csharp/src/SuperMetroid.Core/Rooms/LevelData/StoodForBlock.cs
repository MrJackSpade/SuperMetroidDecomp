namespace SuperMetroid.Core.Rooms.LevelData;

/// <summary>
/// The whole block a cell stands for (<see cref="Compatibility.ContinuedBlocks"/>), with its kind as a whole block (#2554):
/// a read that reaches it meets no continuing cell, so its readers answer only the kinds a whole block can be
/// (<see cref="IWholeBlockReader{TAnswer}"/>).
/// </summary>
/// <param name="Block">The block reached, where a reaction to it takes place.</param>
/// <param name="Kind">How the block collides, with the reaction a reacting kind carries.</param>
internal readonly record struct StoodForBlock(RoomCollisionBlock Block, RoomBlockKind.Whole Kind);
