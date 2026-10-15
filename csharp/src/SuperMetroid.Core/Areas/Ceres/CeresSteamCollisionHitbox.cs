namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>Immutable steam rectangle and native touch/shot callback identities.</summary>
internal readonly record struct CeresSteamCollisionHitbox(
    short Left, short Top, short Right, short Bottom, ushort TouchAi, ushort ShotAi);
