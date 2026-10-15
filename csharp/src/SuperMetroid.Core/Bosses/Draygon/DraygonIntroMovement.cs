namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>
/// One decoded Evir movement record from Draygon's fixed opening dance stream.
/// The delete sentinel is kept distinct from the otherwise valid zero-delta record.
/// </summary>
/// <param name="Delta">The signed per-frame room-pixel step, each axis one decoded nibble.</param>
/// <param name="DeletesSprite">True for the delete sentinel.</param>
internal readonly record struct DraygonIntroMovement(
    Offset16<RoomPixels> Delta,
    bool DeletesSprite);
