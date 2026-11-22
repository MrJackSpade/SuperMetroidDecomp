namespace SuperMetroid.Core.Enemies.KiHunters;

/// <summary>
/// One Ki-Hunter's wings, of any colour (#2146): they follow their body until they are stripped, with the shared
/// reactions. Their definitions ignore Samus and her shots and withhold their frames' hitboxes, so no shot or bomb
/// reaches a reaction of the species'.
/// </summary>
/// <param name="species">The Ki-Hunter species owner.</param>
/// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
/// <param name="place">The place the wings occupy.</param>
/// <param name="common">The shared participation.</param>
/// <param name="settings">What the enemy's population entry tells its species.</param>
/// <param name="partner">The body the room lists right before the wings, which offered them their creation (#2554).</param>
/// <param name="state">What the enemy's setup built (#2554).</param>
internal sealed partial class KiHunterWingsEntity(KiHunterBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place,
    EnemyCommonParticipation common, KiHunterWingsSpawnSettings settings, KiHunterWingsBody partner, KiHunterWingState state)
    : CommonParticipationEnemyEntity(occupant, place, common), IEnemyAi, IEnemyTypedAnimation
{
    /// <summary>
    /// Builds the enemy, running its setup as the game builds it (#2554). A restore of the game's state builds it through the
    /// primary constructor from what it holds, so the setup never runs again.
    /// </summary>
    /// <param name="species">The Ki-Hunter species owner.</param>
    /// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
    /// <param name="place">The place the wings occupy.</param>
    /// <param name="common">The shared participation.</param>
    /// <param name="settings">What the enemy's population entry tells its species.</param>
    /// <param name="partner">The body the room lists right before the wings, which offered them their creation (#2554).</param>
    internal KiHunterWingsEntity(KiHunterBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common,
        KiHunterWingsSpawnSettings settings, KiHunterWingsBody partner)
        : this(species, occupant, place, common, settings, partner, KiHunterBehavior.InitializeKiHunterWings(place, partner))
    {
    }

    /// <summary>What the enemy's population entry tells its species, which its factory checked as it built the enemy.</summary>
    internal KiHunterWingsSpawnSettings Settings { get; } = settings;

    /// <summary>The enemy's state, which its setup builds as the enemy is built (#2554).</summary>
    internal KiHunterWingState State { get; } = state;

    /// <summary>The body the wings follow until they are stripped.</summary>
    private KiHunterWingsBody Partner { get; } = partner;

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame) => species.RunKiHunterWingsMainAi(Place, State, Partner, frame);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => KiHunterBehavior.AdvanceKiHunterWingsTypedAnimation(Place, State);
}
