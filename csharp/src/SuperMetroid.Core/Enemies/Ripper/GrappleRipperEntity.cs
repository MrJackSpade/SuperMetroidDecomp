namespace SuperMetroid.Core.Enemies.Ripper;

/// <summary>The grapple Ripper (#2146), which Samus's grapple can hold.</summary>
/// <param name="species">The Ripper species owner.</param>
/// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
/// <param name="place">The place the grapple Ripper occupies.</param>
/// <param name="common">The shared participation.</param>
/// <param name="settings">What the Ripper's population entry tells its species.</param>
/// <param name="state">What the enemy's setup built, which its base holds (#2554).</param>
internal sealed partial class GrappleRipperEntity(RipperBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place,
    EnemyCommonParticipation common, GrappleRipperSpawnSettings settings, RipperEnemyState state)
    : RipperEntity(species, occupant, place, common, state)
{
    /// <summary>
    /// Builds the enemy, running its setup as the game builds it (#2554). A restore of the game's state builds it through the
    /// primary constructor from what it holds, so the setup never runs again.
    /// </summary>
    /// <param name="species">The Ripper species owner.</param>
    /// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
    /// <param name="place">The place the grapple Ripper occupies.</param>
    /// <param name="common">The shared participation.</param>
    /// <param name="settings">What the Ripper's population entry tells its species.</param>
    internal GrappleRipperEntity(RipperBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common,
        GrappleRipperSpawnSettings settings)
        : this(species, occupant, place, common, settings, RipperBehavior.InitializeGrappleRipper(settings, place))
    {
    }

    /// <summary>What the Ripper's population entry tells its species, which its factory checked as it built the enemy.</summary>
    internal GrappleRipperSpawnSettings Settings { get; } = settings;

    /// <inheritdoc/>
    public override void RunMainAi(IEnemyAiFrame frame) => Species.RunGrappleRipperMainAi(Settings, Place, State, frame);
}
