namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>One native physical hitbox and its independently selected callbacks.</summary>
internal readonly record struct CrocomireBodyCollisionHitbox(
    short Left, short Top, short Right, short Bottom, ushort TouchAi, ushort ShotAi);
