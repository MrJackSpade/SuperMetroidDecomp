namespace SuperMetroid.Core.Enemies.Evir;

/// <summary>
/// An Evir's spit (#2146): it waits in its body's mouth, flies at Samus and regenerates, with the shared reactions its
/// definition names. Its definition names its touch, shot and Power Bomb reactions and withholds its frames' shot
/// hitboxes, so none falls to the Evir's own.
/// </summary>
/// <param name="species">The Evir species owner.</param>
/// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
/// <param name="place">The place the spit occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed partial class EvirSpitEntity(EvirBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(occupant, place, common), IEnemyInitialization, IEnemyAi, IEnemyTypedAnimation
{
    /// <inheritdoc/>
    public void Initialize() => species.InitializeEvirProjectile(Place);

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame) => EvirBehavior.RunEvirProjectileMainAi(Place, frame);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => species.AdvanceEvirSpitTypedAnimation(Place);
}
