namespace SuperMetroid.Core.Enemies.Magdollite;

/// <summary>A Magdollite's three retail-ordered places.</summary>
/// <param name="Head">The head place.</param>
/// <param name="Body">The body place.</param>
/// <param name="Overlay">The overlay place.</param>
internal readonly record struct MagdolliteComposite(RoomEnemyPlace Head, RoomEnemyPlace Body, RoomEnemyPlace Overlay);
