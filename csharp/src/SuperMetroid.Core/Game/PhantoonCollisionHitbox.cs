namespace SuperMetroid.Core.Game;

/// <summary>Engine-owned rectangle and touch/shot callbacks for Phantoon.</summary>
internal readonly record struct PhantoonCollisionHitbox(
    short Left, short Top, short Right, short Bottom, ushort TouchAi, ushort ShotAi);
