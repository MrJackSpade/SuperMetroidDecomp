namespace SuperMetroid.Core.Areas.Tourian;

/// <summary>
/// The three Tourian entrance-statue records' animations, each of which deletes its record at once, (#641), one per bank-$AA list a spawn, branch or resumed sleep enters, in authored order.
/// Generated from the verified native lists; see <see cref="TourianEntranceStatueAnimations"/>.
/// </summary>
internal enum TourianEntranceStatueProgram
{
    /// <summary><c>InstList_TourianStatue_Ridley_0</c> at $AA:D7A5.</summary>
    Ridley,
    /// <summary><c>InstList_TourianStatue_Phantoon_0</c> at $AA:D7AF.</summary>
    Phantoon,
    /// <summary><c>InstList_TourianStatue_BaseDecoration_0</c> at $AA:D7B9.</summary>
    BaseDecoration,
}
