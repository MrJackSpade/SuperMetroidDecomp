namespace SuperMetroid.Core.Enemies.Boyon;

/// <summary>
/// One Boyon occupant: its initialization, bounce AI and typed animation, with the shared reactions. It owns the
/// occupant's state; the initializer writes every word and the program before any is read, so a re-initialized
/// occupant of the same slot never sees the previous one's values.
/// </summary>
/// <param name="species">The Boyon species owner, which publishes the bounce sound.</param>
/// <param name="slot">The slot the Boyon occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class BoyonEntity(BoyonBehavior species, RoomEnemySlot slot, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(slot, common), IEnemyInitialization, IEnemyMainAi, IEnemyTypedAnimation
{
    /// <inheritdoc/>
    public EnemyInitializationAiHandler? FindInitialization() => BoyonBehavior.InitializeBoyon;

    /// <inheritdoc/>
    public EnemyMainAiHandler? FindMain() => RunMain;

    /// <inheritdoc/>
    public Action<RoomEnemySlot, EnemyInstructionContext>? FindTypedAnimation() => species.AdvanceBoyonAnimation;

    /// <summary>Ports <c>MainAI_Boyon</c>.</summary>
    private void RunMain(RoomEnemySlot slot, IEnemyAiFrame frame) =>
        BoyonBehavior.RunBoyonMain(slot, slot.RequireSpeciesState<BoyonEnemyState>(), frame.Samus);
}
