namespace SuperMetroid.Core.Enemies.Dragon;

/// <summary>
/// A Dragon body (#2175): it waits below the lava, rises to spit its fireballs at Samus and sinks again, moving the cosmetic
/// wing its room lists right after it (<see cref="DragonWingEntity"/>) and carrying each hit on to that wing.
/// </summary>
/// <param name="species">The Dragon species owner.</param>
/// <param name="place">The place the body occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class DragonEntity(DragonBehavior species, RoomEnemyPlace place, EnemyCommonParticipation common)
    : DragonPartEntity(species, place, common), IEnemyInitialization, IEnemyAi, IEnemyTypedAnimation
{
    /// <inheritdoc/>
    public void Initialize() => Species.InitializeDragonBody(Place);

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame) => Species.RunDragonMain(Place, Place.RequireSpeciesState<DragonEnemyState>(), frame.Samus);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => DragonBehavior.AdvanceDragonBodyTypedAnimation(Place);
}
