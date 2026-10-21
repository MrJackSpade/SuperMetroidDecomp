using SuperMetroid.Core.Areas.Ceres;

namespace SuperMetroid.Verification.Areas.Ceres;

/// <summary>The program and entry of one ceres baby native list addresses entry.</summary>
/// <param name="Program">The program.</param>
/// <param name="Entry">The entry.</param>
internal readonly record struct CeresBabyNativeListAddressesEntry(CeresBabyProgram Program, ushort Entry);
