namespace SuperMetroid.Core.Rooms.LevelData;

/// <summary>
/// What one reader of a room's cells makes of each kind of block it can meet, the cells that continue another block among
/// them (#2554): a reader that reads cells as they lie, without first reaching the block a continuing cell stands for
/// (<see cref="Compatibility.ContinuedBlocks"/>). The block's kind calls the one member for its own case
/// (<see cref="RoomBlockKind.Read{TAnswer}(IRoomBlockReader{TAnswer})"/>).
/// </summary>
/// <typeparam name="TAnswer">What the reader makes of a block.</typeparam>
public interface IRoomBlockReader<out TAnswer> : IWholeBlockReader<TAnswer>
{
    /// <summary>What the reader makes of a cell continuing a block along its row.</summary>
    TAnswer HorizontalExtension();

    /// <summary>What the reader makes of a cell continuing a block along its column.</summary>
    TAnswer VerticalExtension();
}
