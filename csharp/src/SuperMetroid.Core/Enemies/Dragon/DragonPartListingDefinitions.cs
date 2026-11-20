namespace SuperMetroid.Core.Enemies.Dragon;

/// <summary>Where the room lists a Dragon's wing, and the enemy a part's follow-up to a hit reaches.</summary>
internal static class DragonPartListingDefinitions
{
    /// <summary>The places after its body the room lists a Dragon's wing: right after it.</summary>
    internal const int WingPlacesAfterBody = 1;

    /// <summary>The enemies a part's follow-up to a hit reaches: the one the room lists right after the struck part.</summary>
    internal const int FollowUpReach = 1;
}
