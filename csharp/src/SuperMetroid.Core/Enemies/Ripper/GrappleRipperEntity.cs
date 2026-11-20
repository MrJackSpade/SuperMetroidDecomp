namespace SuperMetroid.Core.Enemies.Ripper;

/// <summary>The grapple Ripper (#2146), which Samus's grapple can hold.</summary>
/// <param name="species">The Ripper species owner.</param>
/// <param name="place">The place the grapple Ripper occupies.</param>
/// <param name="common">The shared participation.</param>
/// <param name="settings">What the enemy's population entry tells its species.</param>
internal sealed class GrappleRipperEntity(RipperBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common, GrappleRipperSpawnSettings settings)
    : RipperEntity(species, occupant, place, common)
{
    /// <summary>What the enemy's population entry tells its species, which it flies by (#2554).</summary>
    private GrappleRipperSpawnSettings Settings { get; } = settings;

    /// <summary>The enemy's state, which its initialization builds as the enemy is built (#2554).</summary>
    internal override RipperEnemyState State { get; } = RipperBehavior.InitializeGrappleRipper(place, settings);

    /// <inheritdoc/>
    public override void RunMainAi(IEnemyAiFrame frame) => Species.RunGrappleRipperMainAi(Place, State, Settings, frame);
}
