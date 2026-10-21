using SuperMetroid.Core.Bosses.Torizo;
using SuperMetroid.Verification.Enemies;

namespace SuperMetroid.Verification.Bosses.Torizo;

/// <summary>The cue and operands of one torizo native list addresses body instruction.</summary>
/// <param name="Cue">The cue.</param>
/// <param name="Operands">The operands.</param>
internal readonly record struct TorizoNativeListAddressesBodyInstruction(NativeCueShape<TorizoCue> Cue, TorizoOperand[] Operands);
