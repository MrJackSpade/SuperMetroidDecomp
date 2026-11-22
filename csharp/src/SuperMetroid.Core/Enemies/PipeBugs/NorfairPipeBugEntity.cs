namespace SuperMetroid.Core.Enemies.PipeBugs;

/// <summary>
/// A Gamet (#2146), the Norfair Pipe Bug: it rises with its formation, staggers and flies at Samus. Its formation's leader
/// (<see cref="NorfairPipeBugLeaderEntity"/>) also waits for its formation and for Samus; its followers
/// (<see cref="NorfairPipeBugFollowerEntity"/>) wait for their leader to launch them.
/// </summary>
/// <param name="species">The Pipe Bugs species owner.</param>
/// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
/// <param name="place">The place the Pipe Bug occupies.</param>
/// <param name="common">The shared participation.</param>
internal abstract partial class NorfairPipeBugEntity(PipeBugsBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(occupant, place, common), IEnemyAi, IEnemyTypedAnimation
{
    /// <summary>The enemy's state, which its setup builds as the enemy is built (#2554).</summary>
    internal NorfairPipeBugState State { get; } = species.InitializeNorfairPipeBug(place);

    /// <summary>The Pipe Bugs species owner.</summary>
    protected PipeBugsBehavior Species => species;

    /// <inheritdoc/>
    public abstract void RunMainAi(IEnemyAiFrame frame);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => PipeBugsBehavior.AdvanceNorfairPipeBugTypedAnimation(Place, State);
}
