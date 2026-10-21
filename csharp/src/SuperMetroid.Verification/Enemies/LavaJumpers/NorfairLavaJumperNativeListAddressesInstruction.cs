using SuperMetroid.Core.Enemies.LavaJumpers;

namespace SuperMetroid.Verification.Enemies.LavaJumpers;

/// <summary>The kind and operands of one norfair lava jumper native list addresses instruction.</summary>
/// <param name="Kind">The kind.</param>
/// <param name="Operands">The operands.</param>
internal readonly record struct NorfairLavaJumperNativeListAddressesInstruction(NorfairLavaJumperCueKind Kind, string Operands);
