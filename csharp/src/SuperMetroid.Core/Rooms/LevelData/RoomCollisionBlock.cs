namespace SuperMetroid.Core.Rooms.LevelData;

/// <summary>One indexed pair from native <c>level_data</c> and parallel <c>BTS</c>.</summary>
/// <param name="Index">Row-major room-block index, not the native level-data byte offset; synthetic prefilled-solid boundary results use minus one.</param>
/// <param name="PackedWord">Complete native BG1 word: collision type, parent flips and visual block selector.</param>
/// <param name="Bts">
/// Parallel native BTS byte. The collision type determines which of its named interpretations is valid; it is not a
/// universal terrain identifier.
/// </param>
public readonly record struct RoomCollisionBlock(int Index, RoomLevelWord PackedWord, RoomBlockBehavior Bts)
{
    /// <summary>Typed bank-$94 collision-dispatch nibble for this block.</summary>
    public RoomCollisionType CollisionType => PackedWord.CollisionType;
}
