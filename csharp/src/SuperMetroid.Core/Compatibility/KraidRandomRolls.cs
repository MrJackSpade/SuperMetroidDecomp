namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// The parts of the shared random number Kraid's choices read where the game read neither the number's ordinary roll,
/// its second roll nor its range (#2563): his second-phase walk, his rising rocks' flight and sideways speed, and the
/// launch of a spat rock.
/// </summary>
/// <remarks>
/// Read at each choice's draw, through the definition or behaviour that names the choice. Evidence: each choice's
/// original masking and shifting of the shared random word, which a field reproduces bit for bit; the SMV replays
/// compare the random value and every outcome it drives on every update.
/// </remarks>
[CompatibilityService("which part of the shared random number each of Kraid's random choices reads, exactly as the game read it")]
internal static class KraidRandomRolls
{
    /// <summary>
    /// The roll of eight the second-phase walk is chosen by: the number's third through fifth places.
    /// </summary>
    internal static RandomSampleField SecondPhaseWalk { get; } = new(2, 3);

    /// <summary>
    /// The roll of two that sends a rising rock flying to the right: the number's fifth place.
    /// </summary>
    internal static RandomSampleField RisingRockFlight { get; } = new(4, 1);

    /// <summary>
    /// The roll of sixty-four a rising rock's sideways speed reads: the number's fifth through tenth places.
    /// </summary>
    internal static RandomSampleField RisingRockSideSpeed { get; } = new(4, 6);

    /// <summary>
    /// The roll of eight a spat rock's launch speed is chosen by: the number's second through fourth places.
    /// </summary>
    internal static RandomSampleField SpitLaunch { get; } = new(1, 3);
}
