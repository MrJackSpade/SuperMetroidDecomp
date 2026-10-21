using SuperMetroid.Core.Enemies.WorkRobots;

namespace SuperMetroid.Verification.Enemies.WorkRobots;

/// <summary>The kind and operands of one work robot native list addresses instruction.</summary>
/// <param name="Kind">The kind.</param>
/// <param name="Operands">The operands.</param>
internal readonly record struct WorkRobotNativeListAddressesInstruction(WorkRobotCueKind Kind, string Operands);
