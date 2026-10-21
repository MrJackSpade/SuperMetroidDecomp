namespace SuperMetroid.Verification.Enemies.Puyo;

/// <summary>The x, y and delay of one population.</summary>
/// <param name="X">The x.</param>
/// <param name="Y">The y.</param>
/// <param name="Delay">The delay.</param>
internal readonly record struct Population(ushort X, ushort Y, ushort Delay);
