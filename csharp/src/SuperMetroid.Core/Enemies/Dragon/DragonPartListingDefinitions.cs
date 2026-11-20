namespace SuperMetroid.Core.Enemies.Dragon;

/// <summary>Where the room lists a Dragon's wing, and the enemy a part's follow-up to a hit reaches.</summary>
internal static class DragonPartListingDefinitions
{
    /// <summary>The body, then its wing.</summary>
    internal static EnemyComposite<DragonPart> Listing { get; } = new([DragonPart.Body, DragonPart.Wing]);

}
