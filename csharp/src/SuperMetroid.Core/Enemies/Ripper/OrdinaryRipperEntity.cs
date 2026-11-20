namespace SuperMetroid.Core.Enemies.Ripper;

/// <summary>The ordinary Ripper (#2146).</summary>
/// <param name="species">The Ripper species owner.</param>
/// <param name="place">The place the Ripper occupies.</param>
/// <param name="common">The shared participation.</param>
/// <param name="settings">What the enemy's population entry tells its species.</param>
internal sealed class OrdinaryRipperEntity(RipperBehavior species, RoomEnemyPlace place, EnemyCommonParticipation common, RipperSpawnSettings settings)
    : RipperEntity(species, place, common)
{
    /// <summary>What the enemy's population entry tells its species, which it flies by (#2554).</summary>
    private RipperSpawnSettings Settings { get; } = settings;

    /// <summary>The enemy's state, which its initialization builds as the enemy is built (#2554).</summary>
    internal override RipperEnemyState State { get; } = RipperBehavior.InitializeRipper(settings);

    /// <inheritdoc/>
    public override void RunMainAi(IEnemyAiFrame frame) => Species.RunRipperMainAi(Place, State, Settings, frame);
}
