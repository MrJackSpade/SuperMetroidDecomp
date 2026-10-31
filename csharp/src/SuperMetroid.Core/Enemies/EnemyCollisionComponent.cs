namespace SuperMetroid.Core.Enemies;

/// <summary>
/// One physical component of a large enemy's frame, independent of its replaceable art: an offset from the body's
/// position and the rectangles that component tests, in order (#1481).
/// </summary>
/// <param name="Offset">The component's offset from the body's position.</param>
/// <param name="Hitboxes">The component's rectangles, tested in order; empty for a component without collision.</param>
internal readonly record struct EnemyCollisionComponent(Offset16<RoomPixels> Offset, IReadOnlyList<EnemyCollisionHitbox> Hitboxes);
