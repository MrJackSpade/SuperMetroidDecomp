namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>One native Mother Brain rectangle with its touch and shot callbacks.</summary>
internal readonly record struct MotherBrainCollisionHitbox(
    short Left, short Top, short Right, short Bottom, ushort TouchAi, ushort ShotAi);
