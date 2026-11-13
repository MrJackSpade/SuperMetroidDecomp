namespace SuperMetroid.Core.Areas.Tourian;

/// <summary>
/// The three Tourian entrance-statue records' animations, each of which deletes its record at once, (#641): one segment per animation a spawn, branch or resumed sleep enters, in authored order.
/// Their steps are <see cref="TourianEntranceStatueAnimations"/>.
/// </summary>
internal enum TourianEntranceStatueSegment
{
    /// <summary>The Ridley animation.</summary>
    Ridley,
    /// <summary>The Phantoon animation.</summary>
    Phantoon,
    /// <summary>The base decoration animation.</summary>
    BaseDecoration,
}
