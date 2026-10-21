using SuperMetroid.Core.Enemies.Boyon;

namespace SuperMetroid.Verification.Enemies.Boyon;

/// <summary>The kind and operands of one boyon native list addresses instruction.</summary>
/// <param name="Kind">The kind.</param>
/// <param name="Operands">The operands.</param>
internal readonly record struct BoyonNativeListAddressesInstruction(BoyonCueKind Kind, string Operands);
