namespace SuperMetroid.Core.Enemies.LavaJumpers;

/// <summary>
/// A jumping Squeept (#2164): it leaps out of the lava and falls back, the cosmetic follower its room lists right after it
/// (<see cref="NorfairLavaJumperFollowerEntity"/>) showing its second half while it rises.
/// </summary>
/// <param name="species">The Squeept species owner.</param>
/// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
/// <param name="place">The place the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed partial class SqueeptEntity(SqueeptBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(occupant, place, common), IEnemyAi, IEnemyTypedAnimation
{
    /// <summary>The enemy's state, which its setup builds as the enemy is built (#2554).</summary>
    internal SqueeptState State { get; } = SqueeptBehavior.InitializeSqueept(place);

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame) => species.RunSqueeptMainAi(Place, State);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => SqueeptBehavior.AdvanceSqueeptTypedAnimation(Place, State);
}
