namespace SuperMetroid.Core.Enemies.KiHunters;

/// <summary>
/// One Ki-Hunter's wings, of any colour (#2146): they follow their body until they are stripped, with the shared
/// reactions. Their definitions ignore Samus and her shots and withhold their frames' hitboxes, so no shot or bomb
/// reaches a reaction of the species'.
/// </summary>
/// <param name="species">The Ki-Hunter species owner.</param>
/// <param name="place">The place the wings occupy.</param>
/// <param name="common">The shared participation.</param>
/// <param name="settings">What the enemy's population entry tells its species.</param>
internal sealed class KiHunterWingsEntity(KiHunterBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common, KiHunterWingsSpawnSettings settings)
    : CommonParticipationEnemyEntity(occupant, place, common), IEnemyAi, IEnemyTypedAnimation
{
    /// <summary>The enemy's state, which its initialization builds as the enemy is built (#2554).</summary>
    internal KiHunterWingState State { get; } = species.InitializeKiHunterWings(place, settings);

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame) => species.RunKiHunterWingsMainAi(Place, State, frame);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => KiHunterBehavior.AdvanceKiHunterWingsTypedAnimation(Place, State);
}
