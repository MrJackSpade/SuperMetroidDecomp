using SuperMetroid.Core.Areas.Ceres;

namespace SuperMetroid.Verification.Areas.Ceres;

/// <summary>The definition and definition pointer of one verify compiled definitions case.</summary>
/// <param name="Definition">The definition.</param>
/// <param name="DefinitionPointer">The definition pointer.</param>
internal readonly record struct VerifyCompiledDefinitionsCase(CeresElevatorProjectileDefinition Definition, ushort DefinitionPointer);
