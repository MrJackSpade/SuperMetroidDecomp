namespace SuperMetroid.Core.Enemies.Boyon;

/// <summary>
/// One Boyon occupant: its initialization, bounce AI and typed animation, with the shared reactions. It owns the
/// occupant's state; the initializer writes every value and the animation before any is read, so a re-initialized
/// occupant of the same place never sees the previous one's values.
/// </summary>
/// <param name="species">The Boyon species owner, which publishes the bounce sound.</param>
/// <param name="place">The place the Boyon occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class BoyonEntity(BoyonBehavior species, RoomEnemyPlace place, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(place, common), IEnemyInitialization, IEnemyMainAi, IEnemyTypedAnimation
{
    /// <inheritdoc/>
    public void Initialize() => BoyonBehavior.InitializeBoyon(Place);

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame) =>
        BoyonBehavior.RunBoyonMain(Place, Place.RequireSpeciesState<BoyonEnemyState>(), frame.Samus);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => species.AdvanceBoyonAnimation(Place, context);
}
