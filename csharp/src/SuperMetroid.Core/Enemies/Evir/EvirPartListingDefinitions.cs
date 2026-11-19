namespace SuperMetroid.Core.Enemies.Evir;

/// <summary>
/// Where the room lists an Evir's parts: its arms and its spit in a row after its body, and the enemies a part's
/// follow-up to a hit reaches.
/// </summary>
internal static class EvirPartListingDefinitions
{
    /// <summary>The places after its body the room lists an Evir's arms: right after it.</summary>
    internal const int ArmsPlacesAfterBody = 1;

    /// <summary>The places after its body the room lists an Evir's spit: behind its arms.</summary>
    internal const int SpitPlacesAfterBody = 2;

    /// <summary>The enemies a body's or its arms' follow-up to a hit reaches: the two the room lists after the struck part.</summary>
    internal const int FollowUpReach = 2;
}
