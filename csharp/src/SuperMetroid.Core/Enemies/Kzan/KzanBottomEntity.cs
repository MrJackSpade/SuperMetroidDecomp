using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Enemies.Kzan;

/// <summary>
/// A Kzan's bottom (#2146), which follows its top and has no animation of its own: its own AI, with the shared reactions.
/// </summary>
/// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
/// <param name="place">The place the bottom occupies.</param>
/// <param name="common">The shared participation.</param>
/// <param name="top">What the enemy's setup built (#2554).</param>
[RoomLifetime]
internal sealed partial class KzanBottomEntity(RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common, RoomEnemyPlace top)
    : CommonParticipationEnemyEntity(occupant, place, common), IEnemyInitialization, IEnemyAi
{
    /// <summary>
    /// Builds the enemy, running its setup as the game builds it (#2554). A restore of the game's state builds it through the
    /// primary constructor from what it holds, so the setup never runs again.
    /// </summary>
    /// <param name="species">The Kzan species owner.</param>
    /// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
    /// <param name="place">The place the bottom occupies.</param>
    /// <param name="common">The shared participation.</param>
    internal KzanBottomEntity(KzanBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common)
        : this(occupant, place, common, species.TopOf(place))
    {
    }

    /// <summary>The place of the top the bottom follows, which its room lists directly before it, found as it is built.</summary>
    private RoomEnemyPlace Top { get; } = top;

    /// <inheritdoc/>
    public void Initialize() => KzanBehavior.FollowKzanTop(Place, Top);

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame) => KzanBehavior.FollowKzanTop(Place, Top);
}
