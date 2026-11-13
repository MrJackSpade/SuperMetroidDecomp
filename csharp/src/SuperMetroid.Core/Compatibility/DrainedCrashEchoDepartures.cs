namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// The angles a Shinespark crash sends its two echoes out at when Samus is drained while it runs (#1500). The crash's
/// end picks the pair of angles by the pose Samus holds then, from a table that names one pair for each of the six crash
/// poses. Mother Brain's drain can replace the crash pose with a drained crouch while the crash keeps running, and the
/// crash then indexes the table by that pose, past its six pairs, reading the game's code that follows it as
/// angles. The import pipeline decodes each drained pose's pair once.
/// </summary>
/// <remarks>
/// Consulted when the crash finishes and releases its echoes, during Samus's movement update. Evidence: the crash's end
/// indexes the departure table by the pose's distance from the first crash pose, two bytes per pose, and the drained
/// poses lie past the six crash poses; the crash departure check compares each drained pose's pair with the cartridge's
/// bytes there.
/// </remarks>
/// <typeparam name="TPose">A Samus pose.</typeparam>
/// <typeparam name="TAngles">The two angles a crash's echoes leave at.</typeparam>
[CompatibilityService("a Shinespark crash finishing in a drained pose sends its echoes out at angles read past the crash's departure table")]
public sealed class DrainedCrashEchoDepartures<TPose, TAngles>
    where TPose : notnull
    where TAngles : struct
{
    /// <summary>The angles each drained pose's crash echoes leave at.</summary>
    private readonly IReadOnlyDictionary<TPose, TAngles> _departures;

    /// <summary>Keeps the decoded angles of every drained pose a crash can finish in.</summary>
    /// <param name="departures">The angles each drained pose's crash echoes leave at.</param>
    public DrainedCrashEchoDepartures(IReadOnlyDictionary<TPose, TAngles> departures) => _departures = Ensure.NotNull(departures);

    /// <summary>The angles the echoes of a crash finishing in <paramref name="pose"/> leave at.</summary>
    /// <exception cref="ArgumentOutOfRangeException">A crash reads no angles past its table for the pose.</exception>
    public TAngles Departures(TPose pose) =>
        _departures.TryGetValue(pose, out TAngles departures)
            ? departures
            : throw new ArgumentOutOfRangeException(nameof(pose), pose, "A Shinespark crash reads no departure angles past its table for this pose.");
}
