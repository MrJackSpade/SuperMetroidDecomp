using SuperMetroid.Core.Enemies.Rios;

namespace SuperMetroid.Verification.Enemies.Rios;

/// <summary>The cue one Geruta native list instruction becomes, and its operand roles.</summary>
/// <param name="Cue">The cue, with the flame offset it carries.</param>
/// <param name="Operands">The operands.</param>
internal readonly record struct GerutaNativeListAddressesInstruction(GerutaCue Cue, string Operands);
