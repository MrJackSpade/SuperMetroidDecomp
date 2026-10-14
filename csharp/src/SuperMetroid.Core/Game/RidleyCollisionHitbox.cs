namespace SuperMetroid.Core.Game;

/// <summary>Engine-owned rectangle and callbacks; never derived from editable OAM.</summary>
internal readonly record struct RidleyCollisionHitbox(
    short Left, short Top, short Right, short Bottom, ushort TouchAi, ushort ShotAi);
