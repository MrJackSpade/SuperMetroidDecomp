namespace SuperMetroid.Core.Enemies.Magdollite;

/// <summary>A Magdollite's head: it faces Samus, sinks into the lava to start an attack and emerges when its arm lands.</summary>
/// <param name="species">The Magdollite species owner.</param>
/// <param name="place">The place the head occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class MagdolliteHeadEntity(MagdolliteBehavior species, RoomEnemyPlace place, EnemyCommonParticipation common)
    : MagdollitePartEntity(species, place, common), IEnemySurroundingsInitialization
{
    /// <inheritdoc/>
    internal override MagdollitePartState? State => Place.SpeciesState<MagdolliteHeadState>();

    /// <inheritdoc/>
    public void Initialize(EnemyPopulationSurroundings surroundings) => Species.InitializeMagdolliteHead(Place, Ensure.NotNull(surroundings).Samus);

    /// <inheritdoc/>
    public override void RunMainAi(IEnemyAiFrame frame) =>
        Species.RunMagdolliteHead(Place, Place.RequireSpeciesState<MagdolliteHeadState>(), Ensure.NotNull(frame).Samus);

    /// <inheritdoc/>
    public override void AdvanceTypedAnimation(EnemyAnimationCueContext context) =>
        Species.AdvanceMagdolliteTypedAnimation<MagdolliteHeadState>(Place, context);
}
