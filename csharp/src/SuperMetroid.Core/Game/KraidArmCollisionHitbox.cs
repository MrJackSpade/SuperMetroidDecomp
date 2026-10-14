namespace SuperMetroid.Core.Game;

/// <summary>Signed arm rectangle bounds and native callbacks for touch and shot intersections.</summary>
internal readonly record struct KraidArmCollisionHitbox(
    short Left, short Top, short Right, short Bottom, ushort TouchAi, ushort ShotAi);
