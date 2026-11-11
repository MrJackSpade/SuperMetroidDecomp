namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>The counter phases that cycle the Ceres elevator room's glow and turning-floor platform (#1606).</summary>
internal static class CeresDoorCadenceDefinitions
{
    /// <summary>
    /// The phase of the door's own update count that picks the turning floor's elevator platform picture: two pictures,
    /// each shown for two updates in turn.
    /// </summary>
    internal static readonly UpdateCounterField PlatformPicture = new(1, 1);

    /// <summary>
    /// The phase of the door's own update count that picks the elevator room's glow step: eight steps, each held for
    /// eight updates in turn.
    /// </summary>
    internal static readonly UpdateCounterField GlowStep = new(3, 3);
}
