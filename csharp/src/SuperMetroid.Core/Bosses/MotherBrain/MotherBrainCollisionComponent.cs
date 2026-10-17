namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>One Mother Brain frame component: its offset from the enemy and the rectangles it holds.</summary>
/// <param name="Offset">The component's offset from the enemy position.</param>
/// <param name="Hitboxes">The component's rectangles, tested in order; empty for a component without collision.</param>
internal readonly record struct MotherBrainCollisionComponent(Offset16<RoomPixels> Offset, IReadOnlyList<MotherBrainCollisionHitbox> Hitboxes);
