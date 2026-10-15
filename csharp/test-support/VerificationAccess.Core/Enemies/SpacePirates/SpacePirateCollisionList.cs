namespace SuperMetroid.Core.Enemies.SpacePirates;

/// <summary>One bank-$B2 Space Pirate hitbox list and its rectangles.</summary>
internal readonly record struct SpacePirateCollisionList(ushort Pointer, SpacePirateCollisionHitbox[] Rectangles);
