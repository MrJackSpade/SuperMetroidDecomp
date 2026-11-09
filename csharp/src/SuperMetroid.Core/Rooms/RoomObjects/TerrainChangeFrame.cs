namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// One frame of a timed terrain change (<see cref="TerrainChange"/>): what it draws at the change's block and how many
/// updates it stays, with the move of the change's origin and the sound that come with it.
/// </summary>
/// <param name="Draw">What the frame draws at the change's block.</param>
/// <param name="Updates">The updates the frame stays before the next frame draws, or before the change ends.</param>
/// <param name="MoveBefore">The blocks the change's origin moves before the frame draws; zero for none.</param>
/// <param name="SoundBefore">The sound requested as the frame draws; null for none.</param>
/// <param name="SoundPriority">How crowded the sound's queue may be for the frame's request to join it.</param>
internal readonly record struct TerrainChangeFrame(
    RoomObjectDraw Draw,
    int Updates,
    Offset16<RoomBlocks> MoveBefore = default,
    SoundEffectId? SoundBefore = null,
    SoundRequestPriority SoundPriority = SoundRequestPriority.Normal);
