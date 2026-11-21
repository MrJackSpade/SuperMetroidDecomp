namespace SuperMetroid.Core.Enemies.Dragon;

/// <summary>
/// A Dragon's cosmetic wing (#2175), which shares the Dragon's definition: its placement names it the wing of the body its
/// room lists right before it, which moves it and starts its flapping. It has no AI of its own, and keeps the Dragon's
/// reactions where its definition leaves them to it; its intangibility normally keeps Samus and her shots from reaching it.
/// </summary>
/// <param name="species">The Dragon species owner.</param>
/// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
/// <param name="place">The place the wing occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed partial class DragonWingEntity(DragonBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common)
    : DragonPartEntity(species, occupant, place, common), IEnemyInitialization, IEnemyTypedAnimation
{
    /// <inheritdoc/>
    public void Initialize() => Species.InitializeDragonWing(Place);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => DragonBehavior.AdvanceDragonWingTypedAnimation(Place);
}
