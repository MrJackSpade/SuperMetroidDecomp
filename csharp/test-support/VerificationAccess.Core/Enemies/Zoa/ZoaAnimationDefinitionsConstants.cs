namespace SuperMetroid.Core.Enemies.Zoa;

/// <summary>Cartridge values of Zoa's animation table that only verification reads.</summary>
internal static class ZoaAnimationDefinitionsConstants
{
    /// <summary>
    /// Left-shooting, left-rising, right-shooting, and right-rising instruction lists at
    /// <c>$A3:B40D-$A3:B414</c>, indexed by the selector word <see cref="ZoaNativeWords.AnimationSelectorWord"/> gives.
    /// </summary>
    public const int ReferenceAddress = 0xa3b40d;
}
