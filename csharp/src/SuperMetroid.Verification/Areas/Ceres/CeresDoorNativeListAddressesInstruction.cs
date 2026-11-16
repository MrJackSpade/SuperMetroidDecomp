using SuperMetroid.Core.Areas.Ceres;

namespace SuperMetroid.Verification.Areas.Ceres;

/// <summary>The cues one Ceres door native list instruction may become, and its operands.</summary>
/// <param name="Kinds">The cues the instruction may become: one, or for the Samus-distance test each door event it signals (#2189).</param>
/// <param name="Operands">The operands.</param>
internal readonly record struct CeresDoorNativeListAddressesInstruction(IReadOnlyList<CeresDoorCueKind> Kinds, string Operands);
