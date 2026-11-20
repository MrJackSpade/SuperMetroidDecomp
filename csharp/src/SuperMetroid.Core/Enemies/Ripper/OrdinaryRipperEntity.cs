namespace SuperMetroid.Core.Enemies.Ripper;

/// <summary>The ordinary Ripper (#2146).</summary>
/// <param name="species">The Ripper species owner.</param>
/// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
/// <param name="place">The place the Ripper occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class OrdinaryRipperEntity(RipperBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common)
    : RipperEntity(species, occupant, place, common)
{
    /// <inheritdoc/>
    public override void Initialize() => RipperBehavior.InitializeRipper(Occupant, Place);

    /// <inheritdoc/>
    public override void RunMainAi(IEnemyAiFrame frame) => Species.RunRipperMainAi(Occupant, Place, Place.RequireSpeciesState<RipperEnemyState>(), Place.RequireSpeciesState<RipperEnemyState>(), frame);
}
