using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>
/// The typed programs Ridley's afterburn centres start at spawn (#1435): Ridley's and Mother Brain's explosions initialize a
/// centre from its definition, then start the program its kind names, horizontal or vertical.
/// </summary>
internal static class RidleyProjectileStarts
{
    /// <summary>Each kind's initial program.</summary>
    private static readonly Dictionary<RoomEnemyProjectileKind, Func<EnemyProjectileAnimation>> InitialPrograms = new()
    {
        [RoomEnemyProjectileKind.CeresRidleyHorizontalAfterburnCenter] = () => new RidleyFireballAnimation(RidleyFireballProgram.HorizontalCenter),
        [RoomEnemyProjectileKind.CeresRidleyVerticalAfterburnCenter] = () => new RidleyFireballAnimation(RidleyFireballProgram.VerticalCenter),
    };

    /// <summary>Initializes <paramref name="projectile"/> from its definition and starts the program its kind names.</summary>
    /// <exception cref="KeyNotFoundException">The kind is not an afterburn centre.</exception>
    internal static void Initialize(RoomEnemyProjectileSlot projectile, RoomEnemyProjectileKind kind, SpriteGraphics graphics)
    {
        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(projectile, kind, graphics);
        projectile.StartAnimation(InitialPrograms[kind]());
    }
}
