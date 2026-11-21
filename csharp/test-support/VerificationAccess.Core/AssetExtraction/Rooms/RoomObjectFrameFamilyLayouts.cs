namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>
/// A room object's frames looked up by frame (#2604): the geometry each listed frame's picture covers, read from the
/// family's own list of frames (<see cref="RoomObjectFrameFamily{TFrame}.Frames"/>), which the import writes in order.
/// </summary>
internal static class RoomObjectFrameFamilyLayouts
{
    /// <summary>The frames of a family, by frame.</summary>
    extension<TFrame>(RoomObjectFrameFamily<TFrame> family)
        where TFrame : notnull
    {
        /// <summary>Every listed frame's geometry, looked up by frame.</summary>
        internal IReadOnlyDictionary<TFrame, RoomObjectFrameLayout> Layouts =>
            family.Frames.ToDictionary(frame => frame.Frame, frame => frame.Layout);
    }
}
