namespace SuperMetroid.Core.Game;

/// <summary>One native Mother Brain rectangle with its touch and shot callbacks.</summary>
internal readonly record struct MotherBrainCollisionHitbox(
    short Left, short Top, short Right, short Bottom, ushort TouchAi, ushort ShotAi);
