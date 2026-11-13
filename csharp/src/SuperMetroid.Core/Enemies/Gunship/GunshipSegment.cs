namespace SuperMetroid.Core.Enemies.Gunship;

/// <summary>
/// The gunship hull's and entrance pad's typed programs (#641), one per animation a spawn, branch or resumed sleep enters, in authored order.
/// Their steps are <see cref="GunshipAnimations"/>.
/// </summary>
internal enum GunshipSegment
{
    /// <summary>The entrance pad opening animation.</summary>
    EntrancePadOpening,
    /// <summary>The entrance pad open animation.</summary>
    EntrancePadOpen,
    /// <summary>The entrance pad closing animation.</summary>
    EntrancePadClosing,
    /// <summary>The bottom entrance pad animation.</summary>
    BottomEntrancePad,
    /// <summary>The top hull animation.</summary>
    TopHull,
    /// <summary>The bottom hull animation.</summary>
    BottomHull,
}
