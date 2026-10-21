namespace SuperMetroid.Verification.Enemies.Spike;

/// <summary>The input, x, y, base and extra of one carried frame.</summary>
/// <param name="Input">The input.</param>
/// <param name="X">The x.</param>
/// <param name="Y">The y.</param>
/// <param name="Base">The base.</param>
/// <param name="Extra">The extra.</param>
internal readonly record struct CarriedFrame(ushort Input, uint X, uint Y, uint Base, ushort Extra);
