namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// Definitions for the room object the setup of the escape room before the old Tourian escape shaft places: once Samus
/// passes its trigger point it starts the room's acid rising toward the target the room's effect names (#1800).
/// </summary>
internal static class EscapeAcidRiseObjectDefinitions
{
    /// <summary>The column of the block the room object is spawned at.</summary>
    internal const int BlockX = 16;
    /// <summary>The row of the block the room object is spawned at.</summary>
    internal const int BlockY = 16;

    /// <summary>The point Samus passes, right of X 240 and below Y 1344, to start the acid rising.</summary>
    internal static SamusTriggerPoint TriggerPoint { get; } = new(new Position16<RoomPixels>(240, 1344));

    /// <summary>The acid's rising speed once triggered: 104/256 of a pixel upward per update.</summary>
    internal static SignedEightEight RiseSpeed { get; } = SignedEightEight.FromTwoHundredFiftySixthsOfPixel(-104);

    /// <summary>The updates the acid waits, shaking the room, before it starts to rise.</summary>
    internal const int RiseDelay = 16;
}
