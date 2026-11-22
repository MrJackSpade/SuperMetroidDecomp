namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// The part of the shared random number Norfair Ridley's breakup fragments read for the fraction of their sideways
/// speed (#2563): neither the number's ordinary roll, its second roll nor its range, but its fifth and sixth places.
/// </summary>
/// <remarks>
/// Read at each choice's draw, through the definition or behaviour that names the choice. Evidence: each choice's
/// original masking and shifting of the shared random word, which a field reproduces bit for bit; the SMV replays
/// compare the random value and every outcome it drives on every update.
/// </remarks>
[CompatibilityService("which part of the shared random number Norfair Ridley's breakup fragments read for their speed, exactly as the game read it")]
internal static partial class RidleyRandomRolls
{
    /// <summary>
    /// The roll of four a breakup fragment's sideways speed takes its sixteenths of a pixel from: the number's fifth and
    /// sixth places.
    /// </summary>
    internal static RandomSampleField FragmentSpeedSixteenths { get; } = new(4, 2);
}
