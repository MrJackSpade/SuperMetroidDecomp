namespace SuperMetroid.Core.Enemies.Gunship;

/// <summary>
/// The gunship's bottom hull with its entrance pad (#2146): set up beside the top hull, then moved and animated with the
/// ship, with the shared reactions.
/// </summary>
/// <param name="species">The Gunship species owner.</param>
/// <param name="place">The place the bottom hull occupies.</param>
/// <param name="common">The shared participation.</param>
/// <param name="setup">How the enemy comes to be set up, whose surroundings its initialization reads.</param>
/// <param name="settings">What the enemy's population entry tells its species.</param>
internal sealed class GunshipBottomEntity(GunshipBehavior species, RoomEnemyPlace place, EnemyCommonParticipation common, EnemySetup setup, GunshipSpawnSettings settings)
    : CommonParticipationEnemyEntity(place, common), IEnemyAi, IEnemyTypedAnimation
{
    /// <summary>The enemy's state, which its initialization builds as the enemy is built (#2554).</summary>
    internal GunshipPieceState State { get; } = species.InitializeGunshipBottom(place, setup.SurroundingsOf(place).Samus, settings);

    /// <summary>Takes no action of its own; the top hull moves the ship.</summary>
    /// <param name="frame">The enemy frame.</param>
    public void RunMainAi(IEnemyAiFrame frame)
    {
    }

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => GunshipBehavior.AdvanceGunshipAnimation(Place, State);
}
