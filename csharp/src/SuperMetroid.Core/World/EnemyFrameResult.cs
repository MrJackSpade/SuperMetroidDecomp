using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Elevator;
using SuperMetroid.Core.Enemies.Gunship;

namespace SuperMetroid.Core.World;

/// <summary>
/// What one enemy pass, or a room's enemy setup, did for the runtime to act on after it (#2562): the events its enemies
/// published, in the order they published them. The runtime acts on each at the point the game performs it; see
/// <see cref="EnemyFramePhase"/> and <see cref="EnemyRoomEffects"/>.
/// </summary>
/// <param name="Events">The pass's events, in publication order.</param>
public readonly record struct EnemyFrameResult(IReadOnlyList<EnemyPassEvent> Events)
{
    /// <summary>Whether the pass published an event of kind <typeparamref name="TEvent"/>.</summary>
    public bool Has<TEvent>() where TEvent : EnemyPassEvent => Events.OfType<TEvent>().Any();

    /// <summary>The pass's events of kind <typeparamref name="TEvent"/>, in publication order.</summary>
    public IEnumerable<TEvent> All<TEvent>() where TEvent : EnemyPassEvent => Events.OfType<TEvent>();

    /// <summary>Whether the gunship took step <paramref name="step"/> in the pass.</summary>
    public bool Reports(GunshipFrameEvent step) => All<GunshipEvent>().Any(gunship => gunship.Kind == step);

    /// <summary>Whether the elevator's ride took step <paramref name="step"/> in the pass.</summary>
    public bool Reports(ElevatorFrameEvent step) => All<ElevatorEvent>().Any(elevator => elevator.Kind == step);
}
