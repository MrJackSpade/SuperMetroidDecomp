using SuperMetroid.Core;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.World.RoomStates;

namespace SuperMetroid.AssetExtraction.Enemies.Artwork;

/// <summary>
/// The room data under which a common spawner's code runs (#1513): a state placing one of its PLMs, running one of its
/// room mains or setups, or drawing one of its enemies (any enemy, for code every enemy runs). A set's shared sheet holds a
/// spawner's objects exactly when one of the set's states, or the enemies drawn under it, satisfy it.
/// </summary>
internal sealed record CommonSpawnerPresence
{
    /// <summary>The PLMs whose placement runs the spawner.</summary>
    public IReadOnlyList<RoomObjectKind> RoomObjects { get; init; } = [];

    /// <summary>The room mains that run the spawner.</summary>
    public IReadOnlyList<RoomMainBehavior> Mains { get; init; } = [];

    /// <summary>The room setups that run the spawner.</summary>
    public IReadOnlyList<RoomSetupBehavior> Setups { get; init; } = [];

    /// <summary>The enemies whose presence runs the spawner.</summary>
    public IReadOnlyList<EnemyDefinitionId> Enemies { get; init; } = [];

    /// <summary>Whether any enemy runs the spawner: code every enemy's death, drop or collision runs.</summary>
    public bool AnyEnemy { get; init; }

    /// <summary>The shades the spawner's objects show on the shared sheet besides the set's and its enemies'.</summary>
    public IReadOnlyList<CommonSpawnerShades> Shades { get; init; } = [];

    /// <summary>Whether the spawner runs in any of <paramref name="states"/> or for any of <paramref name="drawn"/>.</summary>
    internal bool RunsIn(IReadOnlyList<RoomStateSettings> states, IReadOnlyCollection<EnemyDefinitionId> drawn)
    {
        Ensure.NotNull(states);
        Ensure.NotNull(drawn);
        if ((AnyEnemy && drawn.Count > 0) || Enemies.Any(drawn.Contains))
            return true;
        return states.Any(state => Mains.Contains(state.MainBehavior) || Setups.Contains(state.SetupBehavior)
            || state.ObjectPopulation.Placements.Span.Any(placement => RoomObjects.Contains(placement.Kind)));
    }
}
