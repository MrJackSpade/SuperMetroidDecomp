namespace SuperMetroid.Core.Game;

/// <summary>One bank-$B2 Space Pirate collision frame and its components.</summary>
internal readonly record struct SpacePirateCollisionFrame(ushort Pointer, SpacePirateCollisionComponent[] Components);
