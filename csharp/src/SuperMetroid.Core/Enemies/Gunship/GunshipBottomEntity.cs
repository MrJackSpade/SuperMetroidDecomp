namespace SuperMetroid.Core.Enemies.Gunship;

/// <summary>
/// The gunship's bottom hull with its entrance pad (#2146): set up beside the top hull, then moved and animated with the
/// ship, with the shared reactions.
/// </summary>
/// <param name="species">The Gunship species owner.</param>
/// <param name="place">The place the bottom hull occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class GunshipBottomEntity(GunshipBehavior species, RoomEnemyPlace place, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(place, common), IEnemySurroundingsInitialization, IEnemyAi, IEnemyTypedAnimation
{
    /// <inheritdoc/>
    public void Initialize(EnemyPopulationSurroundings surroundings) => species.InitializeGunshipBottom(Place, surroundings.Samus);

    /// <summary>Takes no action of its own; the top hull moves the ship.</summary>
    /// <param name="frame">The enemy frame.</param>
    public void RunMainAi(IEnemyAiFrame frame)
    {
    }

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => GunshipBehavior.AdvanceGunshipAnimation(Place);
}
