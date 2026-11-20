namespace SuperMetroid.Core.Enemies;

/// <summary>
/// The entity a species creates for a newly installed occupant, with the setup it runs once its place is filled (#2571).
/// The factory names the setup through the entity's own type, so the lifecycle runs the setup each occupant has without
/// asking the entity what it is, and an enemy with no setup takes no part in setup.
/// </summary>
internal sealed class EnemyOccupant
{
    /// <summary>An occupant held by <paramref name="entity"/> that runs <paramref name="setup"/>, or no setup.</summary>
    /// <param name="entity">The entity the place binds.</param>
    /// <param name="setup">The setup the occupant runs, or null when it has none.</param>
    private EnemyOccupant(IEnemyEntity entity, EnemySetup? setup)
    {
        Entity = entity;
        Setup = setup;
    }

    /// <summary>The entity the place binds.</summary>
    internal IEnemyEntity Entity { get; }

    /// <summary>The setup the occupant runs once its place is filled, or null when it has none.</summary>
    internal EnemySetup? Setup { get; }

    /// <summary>An occupant whose setup is <paramref name="entity"/>'s own initialization.</summary>
    /// <param name="entity">The entity, which sets itself up.</param>
    internal static EnemyOccupant Of(IEnemyInitialization entity) =>
        new(Ensure.NotNull(entity), new EnemySetup.Own(entity));

    /// <summary>An occupant whose setup is <paramref name="entity"/>'s initialization from a population load's surroundings.</summary>
    /// <param name="entity">The entity, which sets itself up from its surroundings.</param>
    internal static EnemyOccupant Of(IEnemySurroundingsInitialization entity) =>
        new(Ensure.NotNull(entity), new EnemySetup.FromSurroundings(entity));

    /// <summary>An occupant with no setup: placing it is all it does before it runs.</summary>
    /// <param name="entity">The entity, which keeps no state its placement does not already give it.</param>
    internal static EnemyOccupant WithoutSetup(IEnemyEntity entity) => new(Ensure.NotNull(entity), setup: null);
}
