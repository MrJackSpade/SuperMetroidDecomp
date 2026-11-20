namespace SuperMetroid.Core.Enemies.Gunship;

/// <summary>
/// The gunship hull's and entrance pad's typed animations (#641).
/// Their steps are <see cref="GunshipAnimations"/>.
/// </summary>
internal enum GunshipSegment
{
    /// <summary>The entrance pad shut, rising, then opening wider and wider until it stands open, held.</summary>
    EntrancePadOpening,
    /// <summary>The entrance pad narrowing from wide open until it is shut, then resting shut.</summary>
    EntrancePadClosing,
    /// <summary>The entrance pad resting shut, held.</summary>
    BottomEntrancePad,
    /// <summary>The gunship's top hull, held.</summary>
    TopHull,
    /// <summary>The gunship's bottom hull, held.</summary>
    BottomHull,
}
