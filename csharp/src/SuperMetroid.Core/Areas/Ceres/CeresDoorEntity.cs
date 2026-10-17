using SuperMetroid.Core.Bosses.Ridley;
using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>One CeresDoor occupant: its own AI handlers, with the shared reactions.</summary>
/// <param name="species">The CeresDoor species owner.</param>
/// <param name="slot">The slot the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class CeresDoorEntity(CeresDoorBehavior species, RoomEnemySlot slot, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(slot, common), IEnemyInitialization, IEnemyMainAi, IEnemyTypedAnimation, IDrawnByCeresRidleyScene
{
    /// <inheritdoc/>
    public EnemyInitializationAiHandler? FindInitialization() => species.RunCeresDoorInitializationAi;

    /// <inheritdoc/>
    public EnemyMainAiHandler? FindMain() => species.RunCeresDoorMainAi;

    /// <inheritdoc/>
    public Action<RoomEnemySlot, EnemyInstructionContext>? FindTypedAnimation() => species.AdvanceCeresDoorTypedAnimation;

    /// <summary>
    /// Whether the door's program has handed its drawing to Ridley's scene (<see cref="CeresDoorCueKind.HandDrawingToRidley"/>)
    /// and not yet taken it back (<see cref="CeresDoorCueKind.MakeVisibleAndTakeDrawingBack"/>).
    /// </summary>
    public bool HandsDrawingToRidley => Slot.VariableB != 0;
}
