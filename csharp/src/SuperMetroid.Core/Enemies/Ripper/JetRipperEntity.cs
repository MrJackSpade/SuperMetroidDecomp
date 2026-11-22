namespace SuperMetroid.Core.Enemies.Ripper;

/// <summary>The jet Ripper (#2146).</summary>
/// <param name="species">The Ripper species owner.</param>
/// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
/// <param name="place">The place the jet Ripper occupies.</param>
/// <param name="common">The shared participation.</param>
/// <param name="settings">What the Ripper's population entry tells its species.</param>
/// <param name="state">What the enemy's setup built, which its base holds (#2554).</param>
internal sealed partial class JetRipperEntity(RipperBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place,
    EnemyCommonParticipation common, RipperSpawnSettings settings, RipperEnemyState state)
    : RipperEntity(species, occupant, place, common, state)
{
    /// <summary>
    /// Builds the enemy, running its setup as the game builds it (#2554). A restore of the game's state builds it through the
    /// primary constructor from what it holds, so the setup never runs again.
    /// </summary>
    /// <param name="species">The Ripper species owner.</param>
    /// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
    /// <param name="place">The place the jet Ripper occupies.</param>
    /// <param name="common">The shared participation.</param>
    /// <param name="settings">What the Ripper's population entry tells its species.</param>
    internal JetRipperEntity(RipperBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common,
        RipperSpawnSettings settings)
        : this(species, occupant, place, common, settings, RipperBehavior.InitializeJetRipper(settings, place))
    {
    }

    /// <summary>What the Ripper's population entry tells its species, which its factory checked as it built the enemy.</summary>
    internal RipperSpawnSettings Settings { get; } = settings;

    /// <inheritdoc/>
    public override void RunMainAi(IEnemyAiFrame frame) => Species.RunJetRipperMainAi(Settings, Place, State, frame);
}
