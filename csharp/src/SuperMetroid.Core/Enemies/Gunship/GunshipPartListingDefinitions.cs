namespace SuperMetroid.Core.Enemies.Gunship;

/// <summary>How the gunship's room lists its pieces (#2556): the bottom hull and the entrance pad right after the top hull.</summary>
internal static class GunshipPartListingDefinitions
{
    /// <summary>The top hull, then the bottom hull, then the entrance pad.</summary>
    internal static EnemyComposite<GunshipMember> Listing { get; } = new([GunshipMember.TopHull, GunshipMember.BottomHull, GunshipMember.EntrancePad]);
}
