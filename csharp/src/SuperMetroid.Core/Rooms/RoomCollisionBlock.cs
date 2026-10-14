namespace SuperMetroid.Core.Rooms;

/// <summary>One indexed pair from native <c>level_data</c> and parallel <c>BTS</c>.</summary>
/// <param name="Index">Row-major room-block index, not the native level-data byte offset; synthetic prefilled-solid boundary results use minus one.</param>
/// <param name="LevelWord">Complete native BG1 word: collision type in bits 12..15, parent flips in bits 10..11, and visual block selector in bits 0..9.</param>
/// <param name="Behavior">Parallel native BTS byte interpreted according to the collision type, rather than a universal terrain identifier.</param>
public readonly record struct RoomCollisionBlock(int Index, ushort LevelWord, byte Behavior)
{
    /// <summary>
    /// Lossless typed interpretation of <see cref="LevelWord"/>. The original property is
    /// retained because ROM fixtures and diagnostic output intentionally expose raw words.
    /// </summary>
    public RoomLevelWord PackedWord => new(LevelWord);

    /// <summary>Typed bank-$94 collision-dispatch nibble for this block.</summary>
    public RoomCollisionType CollisionType => PackedWord.CollisionType;

    /// <summary>
    /// Context-aware, lossless view of the raw BTS byte. The collision type determines
    /// which of its named interpretations is valid.
    /// </summary>
    public RoomBlockBehavior Bts => new(Behavior);
}
