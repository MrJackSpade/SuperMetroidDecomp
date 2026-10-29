using SuperMetroid.Core.Areas.Ceres;

namespace SuperMetroid.Verification.Areas.Ceres;

/// <summary>A Ceres Baby shade and the native palette operand its draw-list instruction carries.</summary>
/// <param name="Shade">The shade.</param>
/// <param name="Operand">The operand.</param>
internal readonly record struct CeresBabyNativeListAddressesShadeOperand(CeresBabyShade Shade, ushort Operand);
