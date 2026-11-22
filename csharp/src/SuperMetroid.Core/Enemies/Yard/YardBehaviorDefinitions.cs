namespace SuperMetroid.Core.Enemies.Yard;

/// <summary>Each of the Yard's behaviours (<see cref="YardBehavior"/>).</summary>
internal static partial class YardBehaviorDefinitions
{
    /// <summary>
    /// Each behaviour: crawling normally or hiding, the Yard watches Samus and takes hits; crawling aggressively it watches
    /// her no longer; dropped or kicked it is in the air and takes no hits, a dropped Yard being no longer solid; launched by
    /// a beam it is in the air and not solid, but still takes hits.
    /// </summary>
    internal static readonly IReadOnlyDictionary<YardBehavior, YardBehaviorDefinition> Behaviors = new Dictionary<YardBehavior, YardBehaviorDefinition>
    {
        [YardBehavior.NormalCrawling] = new(Airborne: false, WatchesSamus: true, Solid: true, TakesHits: true),
        [YardBehavior.AggressiveCrawling] = new(Airborne: false, WatchesSamus: false, Solid: true, TakesHits: true),
        [YardBehavior.Hiding] = new(Airborne: false, WatchesSamus: true, Solid: true, TakesHits: true),
        [YardBehavior.Dropped] = new(Airborne: true, WatchesSamus: false, Solid: false, TakesHits: false),
        [YardBehavior.Kicked] = new(Airborne: true, WatchesSamus: false, Solid: true, TakesHits: false),
        [YardBehavior.BeamLaunched] = new(Airborne: true, WatchesSamus: false, Solid: false, TakesHits: true),
    };
}
