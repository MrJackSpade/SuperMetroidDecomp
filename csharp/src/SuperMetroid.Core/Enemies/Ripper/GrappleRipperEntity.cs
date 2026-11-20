namespace SuperMetroid.Core.Enemies.Ripper;

/// <summary>The grapple Ripper (#2146), which Samus's grapple can hold.</summary>
/// <param name="species">The Ripper species owner.</param>
/// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
/// <param name="place">The place the grapple Ripper occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class GrappleRipperEntity(RipperBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common)
    : RipperEntity(species, occupant, place, common)
{
    /// <inheritdoc/>
    public override void Initialize() => RipperBehavior.InitializeGrappleRipper(Occupant, Place, Place.RequireSpeciesState<RipperEnemyState>());

    /// <inheritdoc/>
    public override void RunMainAi(IEnemyAiFrame frame) => Species.RunGrappleRipperMainAi(Occupant, Place, Place.RequireSpeciesState<RipperEnemyState>(), Place.RequireSpeciesState<RipperEnemyState>(), Place.RequireSpeciesState<RipperEnemyState>(), frame);
}
