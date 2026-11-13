namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// Definitions for the room object that clears the Crateria mainstreet escape
/// passage when the critters escaped. The room's setup spawns it during the escape.
/// </summary>
internal static class CrateriaMainstreetEscapePassageObjectDefinitions
{
    /// <summary>The column of the block the room object is spawned at.</summary>
    internal const int BlockX = 61;
    /// <summary>The row of the block the room object is spawned at.</summary>
    internal const int BlockY = 11;

    /// <summary>The passage's one draw.</summary>
    internal enum Draw
    {
        /// <summary>Clears the two passage blocks.</summary>
        ClearPair,
    }

    /// <summary>The draw that clears the two passage blocks.</summary>
    internal static RoomObjectFrameDraw<Draw> ClearPairDraw { get; } = RoomObjectDraw.Of(Draw.ClearPair);
}
