using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>One of Ridley's side-facing body frames' collision (#1937, #2565): the way it faces, its legs and its head.</summary>
/// <param name="FacesRight">Whether the frame faces right, mirroring the left-facing component origins.</param>
/// <param name="Legs">The frame's leg rectangles.</param>
/// <param name="Head">The frame's head rectangles.</param>
internal sealed record RidleySideBodyCollision(bool FacesRight, EnemyHitboxRectangle[] Legs, EnemyHitboxRectangle[] Head);
