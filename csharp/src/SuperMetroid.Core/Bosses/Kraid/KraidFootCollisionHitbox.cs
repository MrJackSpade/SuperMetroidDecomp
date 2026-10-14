namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>One rectangle's signed extents and touch/shot AI addresses.</summary>
internal readonly record struct KraidFootCollisionHitbox(
    short Left, short Top, short Right, short Bottom, ushort TouchAi, ushort ShotAi);
