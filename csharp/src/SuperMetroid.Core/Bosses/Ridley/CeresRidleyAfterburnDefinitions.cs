namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>
/// Launch velocities of the directional afterburns a Ridley fireball's impact centre spawns
/// (<c>$86:966C</c>/<c>$86:967A</c> horizontal, <c>$86:9688</c>/<c>$86:9696</c> vertical): each
/// moves fourteen pixels per frame along one axis and keeps it for its continuation chain.
/// </summary>
internal static class CeresRidleyAfterburnDefinitions
{
    /// <summary>The signed 8.8 speed of every directional afterburn, fourteen pixels per frame.</summary>
    private const int Speed = 3584;

    /// <summary><c>EnemyProjectile_RidleyHorizontalAfterburn_Right</c>'s velocity.</summary>
    internal static EightEightVector<RoomPixels> Right => new(Speed, 0);

    /// <summary><c>EnemyProjectile_RidleyHorizontalAfterburn_Left</c>'s velocity.</summary>
    internal static EightEightVector<RoomPixels> Left => new(-Speed, 0);

    /// <summary><c>EnemyProjectile_RidleyVerticalAfterburn_Up</c>'s velocity.</summary>
    internal static EightEightVector<RoomPixels> Up => new(0, -Speed);

    /// <summary><c>EnemyProjectile_RidleyVerticalAfterburn_Down</c>'s velocity.</summary>
    internal static EightEightVector<RoomPixels> Down => new(0, Speed);
}
