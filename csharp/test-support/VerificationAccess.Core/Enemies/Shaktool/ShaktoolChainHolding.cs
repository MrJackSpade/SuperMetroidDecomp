using SuperMetroid.Core.World;

namespace SuperMetroid.Core.Enemies.Shaktool;

/// <summary>
/// A Shaktool chain a check or the snapshot import holds (#2554). Production's leading saw starts its chain as it is placed
/// and every segment's entity keeps it to itself; a check that steers a segment's orbit, or an import that fills the
/// segments' recorded state, builds the chain over the room's first seven places itself, as the leading saw's start builds
/// it, and builds each placed segment's own entity in that chain. The segments then run their own setup, AI and animation
/// on the chain the holder holds.
/// </summary>
internal static class ShaktoolChainHolding
{
    /// <summary>Builds a held chain over the room's Shaktool segments.</summary>
    /// <param name="enemies">The room's enemies.</param>
    extension(RoomEnemySystem enemies)
    {
        /// <summary>
        /// Starts the chain the room's first place leads, moving as the leading saw's placement says, its seven segments in
        /// the first seven places in chain order (<see cref="ShaktoolSegmentDefinitions.Chain"/>), and builds the entity of
        /// every Shaktool segment those places hold in it, in place order, as the room's population load builds each one
        /// after the saw before it.
        /// </summary>
        /// <returns>The chain, which holds every segment's state.</returns>
        /// <exception cref="InvalidOperationException">The room's first place holds no Shaktool.</exception>
        internal ShaktoolChain HoldShaktoolChain()
        {
            IReadOnlyList<RoomEnemyPlace> places = Ensure.NotNull(enemies).Places.InUpdateOrder;
            if (!places[0].Holds(EnemyDefinitionId.Shaktool))
                throw new InvalidOperationException("A Shaktool chain starts at the room's first place, which holds no Shaktool.");
            ShaktoolChainMotion motion = places[0].HeldOccupant().SpawnSettings<ShaktoolSpawnSettings>().Motion;
            ShaktoolSegmentDefinition[] segments = ShaktoolSegmentDefinitions.Segments;
            var chain = new ShaktoolChain(
                [.. segments.Select((segment, listed) => new ShaktoolChainSegment(places[listed], segment, new ShaktoolSegmentState()))], motion);
            ShaktoolBehavior species = enemies.ShaktoolBehavior;
            foreach (RoomEnemyPlace place in places.Take(segments.Length).Where(place => place.Holds(EnemyDefinitionId.Shaktool)))
                _ = place.Build(place.HeldOccupant(), new SegmentBuild(species, place.HeldOccupant().SpawnSettings<ShaktoolSpawnSettings>(), chain), enemies.SpeciesComposition.Registry.Common);
            return chain;
        }
    }

    /// <summary>Builds one segment's entity in a held chain.</summary>
    /// <param name="species">The room's Shaktool species owner.</param>
    /// <param name="settings">What the segment's population entry tells its species.</param>
    /// <param name="chain">The held chain the segment is in.</param>
    private sealed class SegmentBuild(ShaktoolBehavior species, ShaktoolSpawnSettings settings, ShaktoolChain chain) : IEnemyBuild
    {
        /// <inheritdoc/>
        public EnemyOccupant Create(RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common) =>
            EnemyOccupant.Running(new ShaktoolEntity(species, occupant, place, common, settings, chain));
    }
}
