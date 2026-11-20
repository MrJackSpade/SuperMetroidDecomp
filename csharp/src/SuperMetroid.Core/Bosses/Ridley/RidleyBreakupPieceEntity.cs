using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>
/// A piece of Lower Norfair Ridley's breakup explosion (#2146): it flies apart by its own AI and animation, answers
/// Ridley's hitboxes, and takes the shared reactions its definition names.
/// </summary>
/// <param name="ridley">The Lower Norfair Ridley owner.</param>
/// <param name="place">The place the piece occupies.</param>
/// <param name="common">The shared participation.</param>
/// <param name="setup">The spawn joining the breakup the piece starts from.</param>
internal sealed class RidleyBreakupPieceEntity(NorfairRidleyBehavior ridley, RoomEnemyPlace place, EnemyCommonParticipation common, EnemySetup setup)
    : CommonParticipationEnemyEntity(place, common), IEnemyAi, IEnemyTypedAnimation, IEnemyFrameHitboxes
{
    /// <summary>The enemy's state, which its initialization builds as the enemy is built (#2554).</summary>
    internal RidleyExplosionFragmentState State { get; } = ridley.Death.InitializeNorfairRidleyExplosion(place, setup.EncounterOf<RidleyBreakupSource>(place));

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame) => ridley.Death.RunNorfairRidleyExplosionMain(Place, State, frame.Samus);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => RidleyBodyAnimation.AdvanceRidleyBreakupAnimation(Place, State);

    /// <summary>None: the piece's definition ignores Samus's touch.</summary>
    public EnemyReaction? SpeciesTouch => null;

    /// <summary>None: the piece's definition ignores shots.</summary>
    public EnemyReaction? SpeciesShot => null;

    /// <summary>None: the game defines no hitboxes for a breakup piece's frames (#2335).</summary>
    /// <param name="probe">The colliding edges and walker.</param>
    /// <param name="reaction">No reaction.</param>
    public bool? FindHitboxReaction(EnemyHitboxProbe probe, out EnemyReaction? reaction)
    {
        reaction = null;
        return null;
    }
}
