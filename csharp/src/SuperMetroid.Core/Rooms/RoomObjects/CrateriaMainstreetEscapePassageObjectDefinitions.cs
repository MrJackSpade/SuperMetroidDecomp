using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// Definitions for the room object that clears the Crateria mainstreet escape
/// passage when the critters escaped. The room's setup spawns it during the escape.
/// </summary>
internal static partial class CrateriaMainstreetEscapePassageObjectDefinitions
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
    internal static RoomObjectFrameDraw<Draw> ClearPairDraw { get; } = CrateriaMainstreetEscapePassageDraws.Family.Of(Draw.ClearPair);

    /// <summary>The opened passage (#1862): the object's block and the one right of it, both air.</summary>
    internal static RoomBlockShape OpenPassage { get; } = new([new(new(0, 0), RoomCollisionType.Air), new(new(1, 0), RoomCollisionType.Air)]);
}
