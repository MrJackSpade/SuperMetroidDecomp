namespace SuperMetroid.Core.Enemies.SpacePirates;

/// <summary>One bank-$B2 Space Pirate collision frame and its hitboxes, placed from the Pirate's origin.</summary>
internal readonly record struct SpacePirateCollisionFrame(ushort Pointer, EnemyCollisionHitbox[] Hitboxes);
