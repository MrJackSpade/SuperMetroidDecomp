namespace SuperMetroid.Core.Enemies.Ripper;

/// <summary>The grapple Ripper (#2146), which Samus's grapple can hold.</summary>
/// <param name="species">The Ripper species owner.</param>
/// <param name="place">The place the grapple Ripper occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class GrappleRipperEntity(RipperBehavior species, RoomEnemyPlace place, EnemyCommonParticipation common)
    : RipperEntity(species, place, common)
{
    /// <inheritdoc/>
    public override void Initialize() => RipperBehavior.InitializeGrappleRipper(Place);

    /// <inheritdoc/>
    public override void RunMainAi(IEnemyAiFrame frame) => Species.RunGrappleRipperMainAi(Place, frame);
}
