namespace SuperMetroid.Core.Rooms.LevelData;

/// <summary>
/// What Samus's solid-enemy collision reads of one interactive enemy.
/// </summary>
/// <param name="Enemy">The enemy itself, which a collision names; not this body's position in the interactive list.</param>
/// <param name="Position">Whole-pixel room centre of the collision box, wrapping past 65535 to 0; Y increases downward.</param>
/// <param name="Radius">Collision half-extents in pixels.</param>
/// <param name="Solid">Whether the body blocks Samus: a frozen enemy always does, otherwise its properties decide.</param>
public readonly record struct SolidEnemyCollisionBody(
    ISolidEnemy Enemy,
    Position16<RoomPixels> Position,
    Extent16<RoomPixels> Radius,
    bool Solid);
