namespace SuperMetroid.Core.Enemies.Evir;

/// <summary>
/// An Evir's spit (#2146): it waits in its body's mouth, flies at Samus and regenerates, with the shared reactions its
/// definition names. Its definition names its touch, shot and Power Bomb reactions and withholds its frames' shot
/// hitboxes, so none falls to the Evir's own.
/// </summary>
/// <param name="species">The Evir species owner.</param>
/// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
/// <param name="place">The place the spit occupies.</param>
/// <param name="common">The shared participation.</param>
/// <param name="state">What the enemy's setup built (#2554).</param>
internal sealed partial class EvirSpitEntity(EvirBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common,
    EvirSpitState state)
    : CommonParticipationEnemyEntity(occupant, place, common), IEnemyAi, IEnemyTypedAnimation
{
    /// <summary>
    /// Builds the enemy, running its setup as the game builds it (#2554). A restore of the game's state builds it through the
    /// primary constructor from what it holds, so the setup never runs again.
    /// </summary>
    /// <param name="species">The Evir species owner.</param>
    /// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
    /// <param name="place">The place the spit occupies.</param>
    /// <param name="common">The shared participation.</param>
    /// <param name="body">The place of the body the room lists before the spit, which offered them their creation (#2554).</param>
    internal EvirSpitEntity(EvirBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common,
        RoomEnemyPlace body)
        : this(species, occupant, place, common, species.InitializeEvirProjectile(place, body))
    {
    }

    /// <summary>The enemy's state, which its setup builds as the enemy is built (#2554).</summary>
    internal EvirSpitState State { get; } = state;

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame) => EvirBehavior.RunEvirProjectileMainAi(Place, State, frame);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => species.AdvanceEvirSpitTypedAnimation(Place, State);
}
