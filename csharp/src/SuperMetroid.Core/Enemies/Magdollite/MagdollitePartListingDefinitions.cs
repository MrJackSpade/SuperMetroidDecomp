namespace SuperMetroid.Core.Enemies.Magdollite;

/// <summary>
/// How the room lists a Magdollite's parts (#2556): its head, body and overlay together, and the enemies a part's
/// follow-up to a hit reaches.
/// </summary>
internal static class MagdollitePartListingDefinitions
{
    /// <summary>The head, then the body, then the overlay.</summary>
    internal static EnemyComposite<MagdolliteMember> Listing { get; } = new([MagdolliteMember.Head, MagdolliteMember.Body, MagdolliteMember.Overlay]);

    /// <summary>The enemies a part's follow-up to a hit reaches: the two the room lists after the struck part.</summary>
    internal const int FollowUpReach = 2;
}
