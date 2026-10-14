namespace SuperMetroid.Core.Game;

/// <summary>One native Draygon rectangle with its touch and shot callbacks.</summary>
internal readonly record struct DraygonCollisionHitbox(
    short Left, short Top, short Right, short Bottom, ushort TouchAi, ushort ShotAi);
