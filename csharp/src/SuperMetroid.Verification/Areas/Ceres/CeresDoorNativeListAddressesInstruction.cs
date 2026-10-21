using SuperMetroid.Core.Areas.Ceres;

namespace SuperMetroid.Verification.Areas.Ceres;

/// <summary>The kind and operands of one ceres door native list addresses instruction.</summary>
/// <param name="Kind">The kind.</param>
/// <param name="Operands">The operands.</param>
internal readonly record struct CeresDoorNativeListAddressesInstruction(CeresDoorCueKind Kind, string Operands);
