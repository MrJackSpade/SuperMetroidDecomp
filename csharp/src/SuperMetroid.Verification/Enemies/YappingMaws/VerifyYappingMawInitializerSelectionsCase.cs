using SuperMetroid.Core.Enemies.YappingMaws;

namespace SuperMetroid.Verification.Enemies.YappingMaws;

/// <summary>The parameter2, expected and name of one verify yapping maw initializer selections case.</summary>
/// <param name="Parameter2">The parameter2.</param>
/// <param name="Expected">The expected.</param>
/// <param name="Name">The name.</param>
internal readonly record struct VerifyYappingMawInitializerSelectionsCase(ushort Parameter2, YappingMawProgram Expected, string Name);
