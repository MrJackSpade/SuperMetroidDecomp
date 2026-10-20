namespace SuperMetroid.Core.Rooms.LevelData;

/// <summary>
/// Collision-relevant words for one entry in native <c>InteractiveEnemyIndices</c>.
/// </summary>
/// <param name="Slot">The enemy's slot number.</param>
/// <param name="Index">Native enemy-slot byte index, ordinarily slot number times $40; not this body's ordinal in the interactive list.</param>
/// <param name="Position">Whole-pixel room centre of the collision box, retained as wrapping 16-bit words; Y increases downward.</param>
/// <param name="Radius">Collision half-extents in pixels.</param>
/// <param name="Solid">Whether the body blocks Samus: a frozen enemy always does, otherwise its properties decide.</param>
public readonly record struct SolidEnemyCollisionBody(
    int Slot,
    int Index,
    Position16<RoomPixels> Position,
    Extent16<RoomPixels> Radius,
    bool Solid);
