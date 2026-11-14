namespace SuperMetroid.Core.Enemies.Boyon;

/// <summary>
/// One Boyon occupant: its initialization, bounce AI and typed animation, with the shared reactions. It owns the
/// occupant's state; the initializer writes every value and the animation before any is read, so a re-initialized
/// occupant of the same slot never sees the previous one's values.
/// </summary>
/// <param name="species">The Boyon species owner, which publishes the bounce sound.</param>
/// <param name="slot">The slot the Boyon occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class BoyonEntity(BoyonBehavior species, RoomEnemySlot slot, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(slot, common), IEnemyInitialization, IEnemyMainAi, IEnemyTypedAnimation
{
    /// <inheritdoc/>
    public void Initialize() => BoyonBehavior.InitializeBoyon(Slot);

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame) =>
        BoyonBehavior.RunBoyonMain(Slot, Slot.RequireSpeciesState<BoyonEnemyState>(), frame.Samus);

    /// <inheritdoc/>
    public Action<RoomEnemySlot, EnemyAnimationCueContext>? FindTypedAnimation() => species.AdvanceBoyonAnimation;
}
