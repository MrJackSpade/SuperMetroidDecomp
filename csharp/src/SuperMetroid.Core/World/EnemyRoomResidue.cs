using System.Collections.Immutable;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.World;

/// <summary>
/// The enemy words the enemy load leaves in place when a room is replaced (#1329): each bank-$86 enemy projectile
/// slot's whole coordinates and velocity, which the projectile deletion (<c>$86:8016/8154</c>) does not clear and a
/// later family initializer may read, and the graphics set whose tiles and palettes stay loaded, which an empty
/// population draws under (#1392). The next room's enemy system starts from them.
/// </summary>
internal sealed class EnemyRoomResidue
{
    /// <summary>The residue of a session's first room: every word clear and no set loaded.</summary>
    internal static EnemyRoomResidue None { get; } = new(
        [.. Enumerable.Repeat(new ProjectileResidue(FixedPosition<RoomPixels>.Zero, default), EnemyProjectileEngine.RoomEnemyProjectileSlotCount)],
        RoomEnemyGraphicsSetDefinitions.SessionStart);

    /// <summary>Per-slot projectile residue in native slot order.</summary>
    private readonly ImmutableArray<ProjectileResidue> _projectiles;


    /// <summary>The graphics set the outgoing room drew under.</summary>
    private readonly RoomEnemyGraphicsSetDefinition _loadedSet;

    /// <summary>Holds one residue per enemy projectile slot and the loaded graphics set.</summary>
    private EnemyRoomResidue(ImmutableArray<ProjectileResidue> projectiles, RoomEnemyGraphicsSetDefinition loadedSet)
    {
        _projectiles = projectiles;
        _loadedSet = loadedSet;
    }

    /// <summary>Captures what the outgoing room's enemies leave behind.</summary>
    internal static EnemyRoomResidue Of(RoomEnemySystem previous)
    {
        Ensure.NotNull(previous);
        return new([.. previous.Projectiles.EnemyProjectiles.Select(slot => new ProjectileResidue(slot.Position, slot.Velocity))],
            previous.GraphicsSet.LoadedSet);
    }

    /// <summary>Writes the residue into a new room's enemy system before its population loads.</summary>
    internal void ApplyTo(RoomEnemySystem enemies)
    {
        IReadOnlyList<RoomEnemyProjectileSlot> slots = enemies.Projectiles.EnemyProjectiles;
        for (int index = 0; index < slots.Count; index++)
        {
            slots[index].Position = FixedPosition<RoomPixels>.FromWhole(_projectiles[index].Position.Whole);
            slots[index].Velocity = _projectiles[index].Velocity;
        }
        enemies.GraphicsSet.Inherit(_loadedSet);
    }

    /// <summary>One enemy projectile slot's surviving words: its whole coordinates and its velocity.</summary>
    /// <param name="Position">The slot's position; only its whole words survive.</param>
    /// <param name="Velocity">The slot's velocity words.</param>
    private readonly record struct ProjectileResidue(FixedPosition<RoomPixels> Position, EightEightVector<RoomPixels> Velocity);
}
