namespace SuperMetroid.Core.Enemies.Yard;

/// <summary>
/// The movement routine each of the Yard's event cues begins (#2022), which the native oracle compares with the routine
/// word the cartridge's list installed at that cue.
/// </summary>
internal static class YardCueMovements
{
    /// <summary>The routine word of the movement <paramref name="cue"/> begins, or null for a cue that begins none.</summary>
    internal static ushort? Begun(YardCue cue) => cue switch
    {
        YardCue.Signal { Kind: YardCueKind.BeginTurn } => YardNativeWords.YardMovements.Word(YardMovement.HoldingStill),
        YardCue.BeginCrawling crawling => YardNativeWords.Crawls.Word(crawling.Crawl),
        YardCue.Signal { Kind: YardCueKind.BeginHiding } => YardNativeWords.YardMovements.Word(YardMovement.Hiding),
        YardCue.BeginFlight => YardNativeWords.YardMovements.Word(YardMovement.Airborne),
        _ => null,
    };
}
