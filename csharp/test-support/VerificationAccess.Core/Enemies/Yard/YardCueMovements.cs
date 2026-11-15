namespace SuperMetroid.Core.Enemies.Yard;

/// <summary>
/// The movement each of the Yard's event cues begins (#2022), which the native oracle compares with the routine word the
/// cartridge's list installed at that cue.
/// </summary>
internal static class YardCueMovements
{
    /// <summary>The movement <paramref name="cue"/> begins, or null for a cue that begins none.</summary>
    internal static YardMovement? Begun(YardCue cue) => cue switch
    {
        YardCue.Signal { Kind: YardCueKind.BeginTurn } => YardMovement.HoldingStill,
        YardCue.BeginCrawling crawling => crawling.Crawl,
        YardCue.Signal { Kind: YardCueKind.BeginHiding } => YardMovement.Hiding,
        YardCue.Signal { Kind: YardCueKind.BeginFlight } => YardMovement.Airborne,
        _ => null,
    };
}
