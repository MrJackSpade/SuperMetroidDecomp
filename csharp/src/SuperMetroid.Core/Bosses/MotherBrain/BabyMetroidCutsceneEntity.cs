using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// The cutscene Baby: its own AI, with the shared reactions. It holds the Baby its late fight made in its place, which it
/// joins as it is built (#2218, #2554).
/// </summary>
/// <param name="species">The BabyMetroidCutscene species owner.</param>
/// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
/// <param name="place">The place the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
/// <param name="baby">The Baby its late fight made in its place.</param>
internal sealed partial class BabyMetroidCutsceneEntity(BabyMetroidCutsceneBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common,
    BabyMetroidCutsceneState baby)
    : CommonParticipationEnemyEntity(occupant, place, common), IEnemyAi, IEnemyTypedAnimation
{
    /// <summary>The Baby, set up as it arrives.</summary>
    private BabyMetroidCutsceneState Baby { get; } = BabyMetroidCutsceneBehavior.InitializeMotherBrainBabyMetroid(place, baby);

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame) => species.RunBabyMetroidCutsceneMainAi(Place, Baby, frame);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => BabyMetroidCutsceneBehavior.AdvanceBabyMetroidCutsceneAnimation(Place, Baby);
}
