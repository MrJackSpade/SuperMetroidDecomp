namespace SuperMetroid.Core.Enemies.Magdollite;

/// <summary>
/// One Magdollite's three parts' states (#2554), which its head makes as it is placed: the head keeps its own, and its body
/// and overlay, placed after it, take theirs.
/// </summary>
/// <param name="Head">The head's state.</param>
/// <param name="Body">The rising body's state.</param>
/// <param name="Overlay">The tracking overlay's state.</param>
internal sealed record MagdolliteStates(MagdolliteHeadState Head, MagdolliteBodyState Body, MagdolliteOverlayState Overlay);
