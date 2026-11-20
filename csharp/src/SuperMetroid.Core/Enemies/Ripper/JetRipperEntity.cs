namespace SuperMetroid.Core.Enemies.Ripper;

/// <summary>The jet Ripper (#2146).</summary>
/// <param name="species">The Ripper species owner.</param>
/// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
/// <param name="place">The place the jet Ripper occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class JetRipperEntity(RipperBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common)
    : RipperEntity(species, occupant, place, common)
{
    /// <inheritdoc/>
    public override void Initialize() => RipperBehavior.InitializeJetRipper(Occupant, Occupant.SpawnSettings<RipperSpawnSettings>(), Place, Place.RequireSpeciesState<RipperEnemyState>());

    /// <inheritdoc/>
    public override void RunMainAi(IEnemyAiFrame frame) => Species.RunJetRipperMainAi(Occupant, Occupant.SpawnSettings<RipperSpawnSettings>(), Place, Place.RequireSpeciesState<RipperEnemyState>(), frame);
}
