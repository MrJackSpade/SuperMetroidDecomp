namespace SuperMetroid.Core.Enemies;

/// <summary>
/// The entity a species creates for a newly installed occupant, with the steps it runs (#2571): the setup it runs once its
/// place is filled, its own or one from a population load's surroundings, and its main AI. The factory names them through
/// the entity's own type, so the lifecycle runs the steps each occupant has without asking the entity what it is, and an
/// enemy with none takes no part in setup or main AI.
/// </summary>
internal sealed partial class EnemyOccupant
{
    /// <summary>An occupant held by <paramref name="entity"/> with the steps it runs.</summary>
    /// <param name="entity">The entity the place binds.</param>
    /// <param name="initialization">The setup the occupant runs wherever it is placed, or null when it has none.</param>
    /// <param name="surroundingsInitialization">The setup a population load runs from its surroundings, or null when it has none.</param>
    /// <param name="mainAi">The occupant's main AI, or null when it has none.</param>
    private EnemyOccupant(
        IEnemyEntity entity,
        IEnemyInitialization? initialization,
        IEnemySurroundingsInitialization? surroundingsInitialization,
        IEnemyAi? mainAi)
    {
        Entity = entity;
        Initialization = initialization;
        SurroundingsInitialization = surroundingsInitialization;
        MainAi = mainAi;
    }

    /// <summary>The entity the place binds.</summary>
    internal IEnemyEntity Entity { get; }

    /// <summary>
    /// The setup the occupant runs once its place is filled, by a population load or by a spawn on another enemy's behalf,
    /// or null when it has none.
    /// </summary>
    internal IEnemyInitialization? Initialization { get; }

    /// <summary>
    /// The setup the population load that places the occupant runs from its surroundings, the room that has them, or null
    /// when the occupant has none.
    /// </summary>
    internal IEnemySurroundingsInitialization? SurroundingsInitialization { get; }

    /// <summary>The main AI the enemy pass runs for the occupant, or null when it has none.</summary>
    internal IEnemyAi? MainAi { get; }

    /// <summary>An occupant whose setup is <paramref name="entity"/>'s own initialization, and which runs its main AI.</summary>
    /// <typeparam name="TEntity">The entity's type.</typeparam>
    /// <param name="entity">The entity, which sets itself up and runs its main AI.</param>
    internal static EnemyOccupant Of<TEntity>(TEntity entity) where TEntity : class, IEnemyInitialization, IEnemyAi =>
        new(Ensure.NotNull(entity), entity, surroundingsInitialization: null, entity);

    /// <summary>
    /// An occupant whose setup is <paramref name="entity"/>'s own initialization and which has no main AI of its own: a part
    /// another part moves, or one that only animates.
    /// </summary>
    /// <param name="entity">The entity, which sets itself up.</param>
    internal static EnemyOccupant WithoutMainAi(IEnemyInitialization entity) =>
        new(Ensure.NotNull(entity), entity, surroundingsInitialization: null, mainAi: null);

    /// <summary>
    /// An occupant whose entity's construction ran its setup, building the state it holds from then on (#2554), and which
    /// runs its main AI.
    /// </summary>
    /// <typeparam name="TEntity">The entity's type.</typeparam>
    /// <param name="entity">The entity, set up as it was built, which runs its main AI.</param>
    internal static EnemyOccupant Running<TEntity>(TEntity entity) where TEntity : class, IEnemyEntity, IEnemyAi =>
        new(Ensure.NotNull(entity), initialization: null, surroundingsInitialization: null, entity);

    /// <summary>An occupant with no setup and no main AI: placing it is all it does.</summary>
    /// <param name="entity">The entity, which keeps no state its placement does not already give it.</param>
    internal static EnemyOccupant PlacedOnly(IEnemyEntity entity) =>
        new(Ensure.NotNull(entity), initialization: null, surroundingsInitialization: null, mainAi: null);
}
