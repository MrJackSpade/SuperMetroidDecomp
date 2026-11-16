using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>
/// A piece of Lower Norfair Ridley's breakup explosion (#2146): it flies apart by its own AI and animation, answers
/// Ridley's hitboxes, and takes the shared reactions its definition names.
/// </summary>
/// <param name="ridley">The Lower Norfair Ridley owner.</param>
/// <param name="place">The place the piece occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class RidleyBreakupPieceEntity(NorfairRidleyBehavior ridley, RoomEnemyPlace place, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(place, common), IEnemyInitialization, IEnemyMainAi, IEnemyTypedAnimation, IEnemyFrameHitboxes
{
    /// <inheritdoc/>
    public void Initialize() => ridley.Death.InitializeNorfairRidleyExplosion(Place);

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame) => ridley.Death.RunNorfairRidleyExplosionMain(Place, frame.Samus);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => RidleyBodyAnimation.AdvanceRidleyBreakupAnimation(Place);

    /// <summary>None: the piece's definition ignores Samus's touch.</summary>
    public EnemyReaction? SpeciesTouch => null;

    /// <summary>None: the piece's definition ignores shots.</summary>
    public EnemyReaction? SpeciesShot => null;

    /// <inheritdoc/>
    public bool? FindHitboxReaction(EnemyHitboxProbe probe, out EnemyReaction? reaction) =>
        RidleyInteractions.FindRidleyHitboxReaction(Place, probe, out reaction);
}
