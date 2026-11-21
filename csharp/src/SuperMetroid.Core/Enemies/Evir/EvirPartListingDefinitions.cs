namespace SuperMetroid.Core.Enemies.Evir;

/// <summary>
/// Where the room lists an Evir's parts: its arms and its spit in a row after its body, and the enemies a part's
/// follow-up to a hit reaches.
/// </summary>
internal static partial class EvirPartListingDefinitions
{
    /// <summary>The body, then its arms, then its spit.</summary>
    internal static EnemyComposite<EvirMember> Listing { get; } = new([EvirMember.Body, EvirMember.Arms, EvirMember.Spit]);

    /// <summary>The enemies a body's or its arms' follow-up to a hit reaches: the two the room lists after the struck part.</summary>
    internal const int FollowUpReach = 2;
}
