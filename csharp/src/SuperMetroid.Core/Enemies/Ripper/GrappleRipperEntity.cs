namespace SuperMetroid.Core.Enemies.Ripper;

/// <summary>The grapple Ripper (#2146), which Samus's grapple can hold.</summary>
/// <param name="species">The Ripper species owner.</param>
/// <param name="place">The place the grapple Ripper occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class GrappleRipperEntity(RipperBehavior species, RoomEnemyPlace place, EnemyCommonParticipation common)
    : RipperEntity(species, place, common)
{
    /// <summary>The enemy's state, which its initialization builds as the enemy is built (#2554).</summary>
    internal override RipperEnemyState State { get; } = RipperBehavior.InitializeGrappleRipper(place);

    /// <inheritdoc/>
    public override void RunMainAi(IEnemyAiFrame frame) => Species.RunGrappleRipperMainAi(Place, State, frame);
}
