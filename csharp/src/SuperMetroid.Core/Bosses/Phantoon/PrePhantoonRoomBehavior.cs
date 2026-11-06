using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Rooms.Backgrounds;

namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>
/// The pre-Phantoon room's own rules: its setup holds an enemy-projectile place
/// (<see cref="PrePhantoonRoomHeldProjectilePlace"/>), and its background-scroll rule keeps the background at no vertical
/// offset while that place is held, which undoes the offset the door into the room installed.
/// </summary>
[RoomLifetime]
internal sealed class PrePhantoonRoomBehavior
{
    /// <summary>Checks that the room is loaded before setup takes its projectile place.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Shared pool in which the setup holds its place.</summary>
    private readonly EnemyProjectileEngine _projectiles;

    /// <summary>
    /// The projectile the room's latest setup holds its place with, or null before any setup. It stops holding the place
    /// when its program deletes it or a room load clears the pool, after which another projectile may take the place.
    /// </summary>
    private RoomEnemyProjectileSlot? _placeHolder;

    /// <summary>Connects the room's rules to its loaded-room guard and projectile pool.</summary>
    internal PrePhantoonRoomBehavior(EnemyRoomBinding room, EnemyProjectileEngine projectiles)
    {
        _room = room;
        _projectiles = projectiles;
    }

    /// <summary>Whether the room's setup still holds its projectile place.</summary>
    private bool PlaceHeld => _placeHolder is { IsActive: true, Kind: RoomEnemyProjectileKind.PrePhantoonRoom };

    /// <summary>
    /// Runs the pre-Phantoon room's setup: holds its projectile place with a blank, harmless projectile, and clears the
    /// background's vertical offset at once.
    /// </summary>
    public void SpawnPrePhantoonRoomProjectile(BackgroundScrollState backgroundScroll)
    {
        Ensure.NotNull(backgroundScroll);
        _room.EnsureLoaded();
        RoomEnemyProjectileSlot projectile = _projectiles.AllocateEnemyProjectile() ?? throw new InvalidOperationException(
            "The pre-Phantoon room projectile found no free slot in a freshly cleared pool.");
        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(projectile, RoomEnemyProjectileKind.PrePhantoonRoom, graphics: default);
        projectile.StartAnimation(new PrePhantoonRoomAnimation());
        _placeHolder = projectile;
        backgroundScroll.BackgroundOffset = backgroundScroll.BackgroundOffset.WithY(0);
    }

    /// <summary>
    /// The room's background-scroll rule, applied at the start of every enemy-projectile update: while the room's setup
    /// holds its projectile place, the background has no vertical offset. Updates that skip the projectile update (X-ray
    /// suspension, Samus's death) skip the rule as well.
    /// </summary>
    /// <param name="backgroundScroll">The runtime's background scroll state, or null for a projectile update run without one.</param>
    /// <exception cref="InvalidOperationException">The place is held and the update has no background scroll state.</exception>
    internal void ApplyBackgroundScrollRule(BackgroundScrollState? backgroundScroll)
    {
        if (!PlaceHeld)
            return;
        BackgroundScrollState scroll = backgroundScroll ?? throw new InvalidOperationException(
            "The pre-Phantoon room's background rule requires the runtime's background scroll state.");
        scroll.BackgroundOffset = scroll.BackgroundOffset.WithY(0);
    }

    /// <summary>Registers the holding projectile's deletion cue.</summary>
    internal static void AddPrePhantoonRoomProjectiles(EnemyProjectileHandlerTable table)
    {
        table.AddCue<PrePhantoonRoomCue>((context, cue) =>
        {
            RoomEnemyProjectileSlot projectile = context.Projectile;
            switch (cue)
            {
                case PrePhantoonRoomCue.Delete:
                    projectile.Clear();
                    return;
                default:
                    throw new InvalidOperationException($"Undefined {nameof(PrePhantoonRoomCue)} {cue}.");
            }
        });
    }
}
