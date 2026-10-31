using System.Collections.Immutable;

namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// What a room leaves for the room that replaces it (#1476). Each room builds its own shots and enemy projectiles, but
/// a few of their values outlive the room and change what the next room's do:
/// <list type="bullet">
/// <item>
/// The sub-pixel part of each of Samus's shot places' horizontal positions. A beam fired to the left first checks its
/// muzzle a fraction of a pixel to its left; when the place's leftover fraction is below that step the beam starts one
/// pixel further left, which moves its whole flight and the frame it reaches an enemy.
/// </item>
/// <item>
/// Each enemy projectile place's whole position and velocity. An enemy projectile that does not set all of them itself
/// when it spawns starts from what the place held, whether a projectile of this room or of the room before left it.
/// </item>
/// </list>
/// Nothing else of the outgoing room's shots or enemy projectiles reaches the next room.
/// </summary>
/// <remarks>
/// Consulted at the room load's teardown step, where the outgoing room's shots and enemy projectiles leave their values
/// here, and when the next room builds its shots and its enemies, which start from them before the room's population
/// loads or any shot is fired. The session's first room finds nothing left and starts everything at rest.
/// Evidence: the original game's room load resets its shots without clearing their sub-pixel positions, and deletes enemy
/// projectiles without clearing their whole positions or velocities. The 100% movie diverges in enemy damage at update
/// 23200 without the shots' fractions (ef4b79943a), and the 13%, 100%, lowc and Ridley movies diverge in enemy
/// projectiles without the enemy projectiles' values (db3aa8b3bb). A shot's vertical sub-pixel position and its Spazer
/// special-attack phase also survived the original reset, but every shot that reads either writes it first, so neither
/// is handed over.
/// </remarks>
[CompatibilityService("a room's shots and enemy projectiles start from what the room before left: the shots' horizontal sub-pixel positions and the enemy projectiles' positions and velocities")]
public sealed class RoomHandOver
{
    /// <summary>The sub-pixel part of each shot place's horizontal position, in place order; empty until a room leaves them.</summary>
    private ImmutableArray<int> _shotHorizontalFractions = [];

    /// <summary>What each enemy projectile place held, in place order; empty until a room leaves them.</summary>
    private ImmutableArray<EnemyProjectileHandOver> _enemyProjectiles = [];

    /// <summary>Records what the outgoing room's shot places leave: each one's horizontal sub-pixel position.</summary>
    /// <param name="horizontalFractions">Each place's fraction, in place order, 0 through 65535.</param>
    public void LeaveShots(IEnumerable<int> horizontalFractions) =>
        _shotHorizontalFractions = [.. Ensure.NotNull(horizontalFractions).Select(fraction =>
            Ensure.BetweenInclusive(0, ushort.MaxValue, fraction))];

    /// <summary>
    /// The horizontal sub-pixel position shot place <paramref name="place"/> starts the room with: what the room before
    /// left, or zero before any room has left one.
    /// </summary>
    public int ShotHorizontalFraction(int place) =>
        _shotHorizontalFractions.IsEmpty ? 0 : _shotHorizontalFractions[place];

    /// <summary>Records what the outgoing room's enemy projectile places leave.</summary>
    /// <param name="places">Each place's position and velocity, in place order.</param>
    public void LeaveEnemyProjectiles(IEnumerable<EnemyProjectileHandOver> places) =>
        _enemyProjectiles = [.. Ensure.NotNull(places)];

    /// <summary>
    /// What enemy projectile place <paramref name="place"/> starts the room with: what the room before left, or rest at
    /// the origin before any room has left one.
    /// </summary>
    public EnemyProjectileHandOver EnemyProjectile(int place) =>
        _enemyProjectiles.IsEmpty ? default : _enemyProjectiles[place];
}
