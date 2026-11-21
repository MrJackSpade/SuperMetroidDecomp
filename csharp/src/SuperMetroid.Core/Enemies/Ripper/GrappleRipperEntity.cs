namespace SuperMetroid.Core.Enemies.Ripper;

/// <summary>The grapple Ripper (#2146), which Samus's grapple can hold.</summary>
/// <param name="species">The Ripper species owner.</param>
/// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
/// <param name="place">The place the grapple Ripper occupies.</param>
/// <param name="common">The shared participation.</param>
/// <param name="settings">What the Ripper's population entry tells its species.</param>
internal sealed class GrappleRipperEntity(RipperBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common, GrappleRipperSpawnSettings settings)
    : RipperEntity(species, occupant, place, common, RipperBehavior.InitializeGrappleRipper(settings, place))
{
    /// <summary>What the Ripper's population entry tells its species, which its factory checked as it built the enemy.</summary>
    internal GrappleRipperSpawnSettings Settings { get; } = settings;

    /// <inheritdoc/>
    public override void RunMainAi(IEnemyAiFrame frame) => Species.RunGrappleRipperMainAi(Occupant, Settings, Place, State, frame);
}
