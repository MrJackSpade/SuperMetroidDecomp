using SuperMetroid.Core.Rooms;

/// <summary>Verification access to <see cref="RoomLevelWord"/> members production does not use.</summary>
internal static class RoomLevelWordAccess
{
    /// <summary>Changes a level word's visual flip bits while preserving its other packed fields.</summary>
    extension(RoomLevelWord self)
    {
        /// <summary>Replaces only the parent-block visual transforms.</summary>
        internal RoomLevelWord WithVisualFlipFlags(LevelBlockFlipFlags visualFlipFlags)
        {
            RoomLevelWord.ValidateVisualFlipFlags((LevelBlockFlipFlags)(visualFlipFlags));
            return new RoomLevelWord(unchecked((ushort)(
                (self.Raw & ~RoomLevelWord.VisualFlipMask) | (ushort)visualFlipFlags)));
        }
    }
}
