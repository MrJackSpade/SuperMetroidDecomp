using SuperMetroid.Core.Areas.Ceres;

namespace SuperMetroid.Verification.Areas.Ceres;

/// <summary>One Ceres elevator arrival actor's animation and the native definition that names its list.</summary>
/// <param name="Animation">The animation the actor starts with.</param>
/// <param name="DefinitionPointer">The native definition.</param>
internal readonly record struct VerifyCompiledDefinitionsCase(CeresElevatorArrivalSegment Animation, ushort DefinitionPointer);
