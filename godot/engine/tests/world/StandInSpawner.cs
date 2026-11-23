using Engine.Actors;
using Engine.Tests.Actors;
using Engine.World.Objects;

namespace Engine.Tests.World;

/// <summary>A spawner for the engine's own checks: it produces stand-in enemies without needing a scene.</summary>
public partial class StandInSpawner : Spawner
{
    /// <inheritdoc/>
    protected override Enemy CreateEnemy() => new StandInEnemy { Name = "Spawned" };
}
