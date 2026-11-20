using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>One BabyMetroidCutscene occupant: its own AI, with the shared reactions.</summary>
/// <param name="species">The BabyMetroidCutscene species owner.</param>
/// <param name="place">The place the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
/// <param name="setup">The spawn joining the Baby the late fight made.</param>
internal sealed class BabyMetroidCutsceneEntity(BabyMetroidCutsceneBehavior species, RoomEnemyPlace place, EnemyCommonParticipation common, EnemySetup setup)
    : CommonParticipationEnemyEntity(place, common), IEnemyAi, IEnemyTypedAnimation
{
    /// <summary>The Baby the late fight made, which the Baby joins as it is built (#2218, #2471, #2554).</summary>
    internal BabyMetroidCutsceneState State { get; } = BabyMetroidCutsceneBehavior.InitializeMotherBrainBabyMetroid(place, setup.EncounterOf<BabyMetroidCutsceneState>(place));

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame) => species.RunBabyMetroidCutsceneMainAi(Place, frame);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => BabyMetroidCutsceneBehavior.AdvanceBabyMetroidCutsceneAnimation(Place);
}
