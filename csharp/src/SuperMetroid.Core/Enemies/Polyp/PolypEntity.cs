namespace SuperMetroid.Core.Enemies.Polyp;

/// <summary>One Polyp occupant: its own AI, with the shared reactions.</summary>
/// <param name="species">The Polyp species owner.</param>
/// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
/// <param name="place">The place the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class PolypEntity(PolypBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(occupant, place, common), IEnemyAi, IEnemyTypedAnimation
{
    /// <summary>The enemy's state, which its setup builds as the enemy is built (#2554).</summary>
    private readonly PolypEnemyState state = species.InitializePolyp(place);

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame) => species.RunPolypMainAi(Occupant, Place, state, frame);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => PolypBehavior.AdvancePolypTypedAnimation(Place, state);
}
