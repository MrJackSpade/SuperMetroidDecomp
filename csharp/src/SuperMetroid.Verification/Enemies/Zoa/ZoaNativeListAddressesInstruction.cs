using SuperMetroid.Core.Enemies.Zoa;

namespace SuperMetroid.Verification.Enemies.Zoa;

/// <summary>The cue one Zoa native list instruction becomes, and its operand roles.</summary>
/// <param name="Cue">The cue, with the launch speed it selects.</param>
/// <param name="Operands">The operands.</param>
internal readonly record struct ZoaNativeListAddressesInstruction(ZoaCue Cue, string Operands);
