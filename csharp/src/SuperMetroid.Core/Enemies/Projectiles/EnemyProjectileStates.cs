namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>
/// What one projectile behaviour keeps on each projectile bound to it (#1470, #2571), one entry per enemy-projectile place.
/// The behaviour creates its table and lives as long as the room's species composition that builds it; its spawner gives
/// each projectile its entry as it sets the projectile up, and the behaviour reads the entry back as its own type. An entry
/// lasts as long as the projectile in its place: the next projectile the behaviour is bound to there is given its own.
/// </summary>
/// <typeparam name="TState">What the behaviour keeps on each of its projectiles.</typeparam>
internal sealed partial class EnemyProjectileStates<TState> where TState : EnemyProjectileState
{
    /// <summary>The entry of the projectile in each place, given when the behaviour's spawner set that projectile up.</summary>
    private readonly TState[] _places = new TState[EnemyProjectilePlaces.Capacity];

    /// <summary>Gives <paramref name="projectile"/>, which the behaviour is bound to, <paramref name="state"/> for its life.</summary>
    /// <param name="projectile">A projectile the behaviour's spawner is setting up.</param>
    /// <param name="state">What the behaviour keeps on it.</param>
    internal void Start(EnemyProjectile projectile, TState state) =>
        _places[Ensure.NotNull(projectile).Place] = Ensure.NotNull(state);

    /// <summary>What the behaviour keeps on <paramref name="projectile"/>, given when its spawner set it up.</summary>
    /// <param name="projectile">A projectile the behaviour is bound to.</param>
    internal TState Of(EnemyProjectile projectile) => _places[Ensure.NotNull(projectile).Place];
}
