using SuperMetroid.Core.Rooms;

/// <summary>Verification access to <see cref="RoomCollisionBlock"/> members production does not use.</summary>
internal static class RoomCollisionBlockAccess
{
    /// <summary>Extracts the visual tile index from a collision block's packed room word.</summary>
    extension(RoomCollisionBlock self)
    {
        /// <summary>Low ten bits selecting the visual 16×16 block definition.</summary>
        internal ushort VisualBlockIndex => self.PackedWord.VisualBlockIndex;
    }
}
