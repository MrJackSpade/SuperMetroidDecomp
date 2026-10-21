using SuperMetroid.Core.Enemies.Animals;

namespace SuperMetroid.Verification.Enemies.Animals;

/// <summary>The parameter and expected of one initializer case.</summary>
/// <param name="Parameter">The parameter.</param>
/// <param name="Expected">The expected.</param>
internal readonly record struct InitializerCase(ushort Parameter, DachoraProgram Expected);
