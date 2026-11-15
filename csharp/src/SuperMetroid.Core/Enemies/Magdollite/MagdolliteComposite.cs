namespace SuperMetroid.Core.Enemies.Magdollite;

/// <summary>A Magdollite's three retail-ordered slots.</summary>
/// <param name="Head">The head slot.</param>
/// <param name="Body">The body slot.</param>
/// <param name="Overlay">The overlay slot.</param>
internal readonly record struct MagdolliteComposite(RoomEnemySlot Head, RoomEnemySlot Body, RoomEnemySlot Overlay);
