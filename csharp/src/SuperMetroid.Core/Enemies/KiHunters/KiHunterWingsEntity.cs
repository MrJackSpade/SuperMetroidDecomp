namespace SuperMetroid.Core.Enemies.KiHunters;

/// <summary>
/// One Ki-Hunter's wings, of any colour (#2146): they follow their body until they are stripped, with the shared
/// reactions. Their definitions ignore Samus and her shots and withhold their frames' hitboxes, so no shot or bomb
/// reaches a reaction of the species'.
/// </summary>
/// <param name="species">The Ki-Hunter species owner.</param>
/// <param name="place">The place the wings occupy.</param>
/// <param name="common">The shared participation.</param>
internal sealed class KiHunterWingsEntity(KiHunterBehavior species, RoomEnemyPlace place, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(place, common), IEnemyInitialization, IEnemyMainAi, IEnemyTypedAnimation
{
    /// <inheritdoc/>
    public void Initialize() => species.InitializeKiHunterWings(Place);

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame) => species.RunKiHunterWingsMainAi(Place, frame);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => KiHunterBehavior.AdvanceKiHunterWingsTypedAnimation(Place);
}
