namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>
/// The steps Ceres Ridley's colours pass through and the rules that select them (#1413). The colours themselves, and
/// their shapes, are the import's.
/// </summary>
/// <remarks>
/// The counts stay in Core (#2471): the getaway floor's zoom fade and the self-destruct alarm's fade are made of their
/// steps (<see cref="RidleyFadeDefinitions"/>), and the alarm's animation shows each colouring for its steps, as well as
/// the import reading them.
/// </remarks>
public static class CeresRidleyShadeDefinitions
{
    /// <summary>The getaway's zoom steps, selected by the whole part of the getaway zoom (zero to eight).</summary>
    public const int GetawayZoomStepCount = 9;

    /// <summary>The steps of the self-destruct warning's alarm colour cycle, after whose last it starts again.</summary>
    public const int AlarmStepCount = 16;

    /// <summary>
    /// The alarm steps each of the alarm's colourings shows for (#2409): one, as each step, every fourth update, shows the
    /// next.
    /// </summary>
    public const int AlarmStepsPerColouring = 1;
}
