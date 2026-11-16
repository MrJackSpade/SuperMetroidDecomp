using SuperMetroid.Core.Enemies.WorkRobots;

namespace SuperMetroid.Verification.Enemies.WorkRobots;

/// <summary>The cue and operands of one work robot native list addresses instruction.</summary>
/// <param name="Cue">The cue it becomes.</param>
/// <param name="Operands">The operands.</param>
internal readonly record struct WorkRobotNativeListAddressesInstruction(WorkRobotCue Cue, string Operands);
