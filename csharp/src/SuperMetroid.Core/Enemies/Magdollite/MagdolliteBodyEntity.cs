namespace SuperMetroid.Core.Enemies.Magdollite;

/// <summary>A Magdollite's rising body, the arm that grows out of the lava under the head and sinks back.</summary>
/// <param name="species">The Magdollite species owner.</param>
/// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
/// <param name="place">The place the body occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class MagdolliteBodyEntity(MagdolliteBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common)
    : MagdollitePartEntity(species, occupant, place, common), IEnemyInitialization
{
    /// <inheritdoc/>
    internal override MagdollitePartState? State => Place.SpeciesState<MagdolliteBodyState>();

    /// <inheritdoc/>
    public void Initialize() => Species.InitializeMagdolliteBody(Place);

    /// <inheritdoc/>
    public override void RunMainAi(IEnemyAiFrame frame) =>
        Species.RunMagdolliteBody(Place, Place.RequireSpeciesState<MagdolliteBodyState>(), Ensure.NotNull(frame).Samus);

    /// <inheritdoc/>
    public override void AdvanceTypedAnimation(EnemyAnimationCueContext context) =>
        Species.AdvanceMagdolliteTypedAnimation<MagdolliteBodyState>(Occupant, Place, Place.RequireSpeciesState<MagdolliteBodyState>(), Place.RequireSpeciesState<MagdolliteHeadState>(), Place.RequireSpeciesState<MagdolliteOverlayState>(), context);
}
