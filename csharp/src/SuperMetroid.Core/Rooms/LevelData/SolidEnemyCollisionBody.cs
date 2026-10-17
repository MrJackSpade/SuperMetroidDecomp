namespace SuperMetroid.Core.Rooms.LevelData;

/// <summary>
/// Collision-relevant words for one entry in native <c>InteractiveEnemyIndices</c>.
/// </summary>
/// <param name="Index">Native enemy-slot byte index, ordinarily slot number times $40; not this body's ordinal in the interactive list.</param>
/// <param name="Position">Whole-pixel room centre of the collision box, retained as wrapping 16-bit words; Y increases downward.</param>
/// <param name="Radius">Collision half-extents in pixels.</param>
/// <param name="Frozen">Whether the enemy is frozen, which makes the body solid regardless of its properties.</param>
/// <param name="Properties">Raw enemy property word; bit $8000 makes an unfrozen body solid to Samus. Other property bits are not filtered by this probe.</param>
public readonly record struct SolidEnemyCollisionBody(
    int Index,
    Position16<RoomPixels> Position,
    Extent16<RoomPixels> Radius,
    bool Frozen,
    SuperMetroid.Core.Enemies.EnemyPropertyWord Properties);
