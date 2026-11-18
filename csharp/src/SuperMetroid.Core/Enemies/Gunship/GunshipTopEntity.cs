namespace SuperMetroid.Core.Enemies.Gunship;

/// <summary>The gunship's top hull (#2146), which moves the whole ship: its own AI, with the shared reactions.</summary>
/// <param name="species">The Gunship species owner.</param>
/// <param name="place">The place the top hull occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class GunshipTopEntity(GunshipBehavior species, RoomEnemyPlace place, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(place, common), IEnemySurroundingsInitialization, IEnemyAi, IEnemyTypedAnimation
{
    /// <inheritdoc/>
    public void Initialize(EnemyPopulationSurroundings surroundings) => species.InitializeGunshipTop(Place, surroundings.Samus);

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame) => species.RunGunshipMainAi(Place, frame);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => GunshipBehavior.AdvanceGunshipAnimation(Place);
}
