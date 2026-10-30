namespace SuperMetroid.Core.Rooms.LevelData;

/// <summary>
/// What Samus's solid-enemy collision reads of one interactive enemy.
/// </summary>
/// <param name="Slot">The enemy's slot ordinal, which a collision records; not this body's position in the interactive list.</param>
/// <param name="Position">Whole-pixel room centre of the collision box, retained as wrapping 16-bit words; Y increases downward.</param>
/// <param name="Radius">Collision half-extents in pixels.</param>
/// <param name="Solid">Whether the body blocks Samus: a frozen enemy always does, otherwise its properties decide.</param>
public readonly record struct SolidEnemyCollisionBody(
    int Slot,
    Position16<RoomPixels> Position,
    Extent16<RoomPixels> Radius,
    bool Solid);
