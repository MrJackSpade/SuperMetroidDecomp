using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Fx;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// Mother Brain's fight changed her room's acid (#2562): her initialization asks for the acid rising and her fake death
/// for its draining (<see cref="MotherBrainRoomFxDefinitions"/>). The room applies it once, on the pass that asked for it;
/// applying it each update would reset its delay and keep the acid from moving.
/// </summary>
/// <param name="Change">The acid's change.</param>
public sealed record MotherBrainAcidChange(RoomEffectDefinition Change) : EnemyPassEvent;
