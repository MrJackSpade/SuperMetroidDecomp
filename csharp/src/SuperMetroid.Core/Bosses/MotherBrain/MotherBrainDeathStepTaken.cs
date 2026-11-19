using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// One step of Mother Brain's late fight and death ran (#2562); the room starts the escape's colour effects and the
/// room-level effects the step asked for (<see cref="MotherBrainStepEffect"/>).
/// </summary>
/// <param name="Step">What the step asked of the room.</param>
public sealed record MotherBrainDeathStepTaken(MotherBrainRainbowBeamAttackStepResult Step) : EnemyPassEvent;
