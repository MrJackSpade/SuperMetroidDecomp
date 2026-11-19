namespace SuperMetroid.Core.Enemies.Magdollite;

/// <summary>
/// Where the room lists a Magdollite's parts: its head, body and overlay in a row, and the enemies a part's follow-up to
/// a hit reaches.
/// </summary>
internal static class MagdollitePartListingDefinitions
{
    /// <summary>The places after its head the room lists a Magdollite's body: right after it.</summary>
    internal const int BodyPlacesAfterHead = 1;

    /// <summary>The places after its head the room lists a Magdollite's overlay: after its body.</summary>
    internal const int OverlayPlacesAfterHead = 2;

    /// <summary>The enemies a part's follow-up to a hit reaches: the two the room lists after the struck part.</summary>
    internal const int FollowUpReach = 2;
}
