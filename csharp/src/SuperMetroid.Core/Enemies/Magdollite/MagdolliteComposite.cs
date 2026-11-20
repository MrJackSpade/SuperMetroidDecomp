namespace SuperMetroid.Core.Enemies.Magdollite;

/// <summary>A Magdollite's three places, listed head, body, overlay.</summary>
/// <param name="Head">The head place.</param>
/// <param name="Body">The body place.</param>
/// <param name="Overlay">The overlay place.</param>
/// <param name="States">The three parts' states, which the head made as it was placed (#2554).</param>
internal readonly record struct MagdolliteComposite(RoomEnemyPlace Head, RoomEnemyPlace Body, RoomEnemyPlace Overlay, MagdolliteStates States);
