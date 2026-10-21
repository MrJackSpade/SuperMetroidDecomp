using SuperMetroid.Core.Enemies.MaridiaSnails;

namespace SuperMetroid.Verification.Enemies.MaridiaSnails;

/// <summary>The kind and operands of one maridia large snail native list addresses instruction.</summary>
/// <param name="Kind">The kind.</param>
/// <param name="Operands">The operands.</param>
internal readonly record struct MaridiaLargeSnailNativeListAddressesInstruction(MaridiaLargeSnailCueKind Kind, string Operands);
