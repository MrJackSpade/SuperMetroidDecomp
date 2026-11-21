using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>One BabyMetroidCutscene occupant: its own AI, with the shared reactions.</summary>
/// <param name="species">The BabyMetroidCutscene species owner.</param>
/// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
/// <param name="place">The place the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed partial class BabyMetroidCutsceneEntity(BabyMetroidCutsceneBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(occupant, place, common), IEnemyInitialization, IEnemyAi, IEnemyTypedAnimation
{
    /// <inheritdoc/>
    public void Initialize() => BabyMetroidCutsceneBehavior.InitializeMotherBrainBabyMetroid(Place);

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame) => species.RunBabyMetroidCutsceneMainAi(Place, frame);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => BabyMetroidCutsceneBehavior.AdvanceBabyMetroidCutsceneAnimation(Place);
}
