using Engine.Actors;
using Engine.Shared;
using Godot;

namespace Engine.World.Objects;

/// <summary>
/// Something that produces enemies at its place by a rule: one every so often, up to so many alive at once, while its
/// condition holds.
/// </summary>
public partial class Spawner : RoomObject
{
    /// <summary>The scene of the enemy it produces. Its root is an <see cref="Enemy"/>.</summary>
    [Export]
    public PackedScene? EnemyScene { get; set; }

    /// <summary>How many updates pass between one enemy and the next.</summary>
    [Export]
    public int IntervalUpdates { get; set; } = 120;

    /// <summary>How many of its enemies may be alive at once.</summary>
    [Export]
    public int MaxAlive { get; set; } = 1;

    private readonly List<Enemy> _alive = [];
    private Countdown _wait = Countdown.Over;

    /// <summary>How many of its enemies are alive.</summary>
    public int AliveCount => _alive.Count;

    /// <inheritdoc/>
    protected override void OnStep()
    {
        _alive.RemoveAll(enemy => enemy.IsDead || Room.IsLeaving(enemy));
        _wait = _wait.Stepped();
        if (_wait.IsRunning || _alive.Count >= MaxAlive)
            return;
        Enemy enemy = CreateEnemy();
        enemy.Position = Position;
        Room.Spawn(enemy);
        _alive.Add(enemy);
        _wait = Countdown.Of(IntervalUpdates);
    }

    /// <summary>Makes one of its enemies.</summary>
    protected virtual Enemy CreateEnemy()
    {
        PackedScene scene = EnemyScene ?? throw new InvalidOperationException($"The spawner \"{Name}\" names no enemy scene.");
        Node root = scene.Instantiate();
        return root as Enemy ?? throw new InvalidOperationException($"The spawner \"{Name}\"'s scene has a {root.GetType().Name} at its root, not an Enemy.");
    }
}
