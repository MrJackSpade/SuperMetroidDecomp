namespace SuperMetroid.Core.Enemies.Corpses;

/// <summary>The random choices that spread a dying corpse's dust clouds: how far right each cloud lies.</summary>
internal static class CorpseDustDefinitions
{
    /// <summary>
    /// The random field that moves a Tourian or Sidehopper corpse's dust cloud zero or one <see cref="SpreadFineStep"/> to
    /// the right (#2479): a count of steps, zero or one by an even choice of the random number. With <see cref="SpreadCoarseSteps"/> it spreads the
    /// cloud 0-26 pixels, even.
    /// </summary>
    internal static readonly RandomSampleField SpreadFineSteps = new(1, 1);

    /// <summary>
    /// The random field that moves a Tourian or Sidehopper corpse's dust cloud zero to three
    /// <see cref="SpreadCoarseStep"/>s to the right.
    /// </summary>
    internal static readonly RandomSampleField SpreadCoarseSteps = new(3, 2);

    /// <summary>The pixels one fine spread step moves a Tourian or Sidehopper corpse's dust cloud.</summary>
    internal const int SpreadFineStep = 2;

    /// <summary>The pixels one coarse spread step moves a Tourian or Sidehopper corpse's dust cloud.</summary>
    internal const int SpreadCoarseStep = 8;

    /// <summary>
    /// Where a Tourian or Sidehopper corpse row's dust cloud appears before its spread, from the corpse's middle: 14 pixels
    /// left and 16 below.
    /// </summary>
    internal static readonly Offset16<RoomPixels> CloudOrigin = new(-14, 16);

    /// <summary>The random field that spreads Dead Torizo's dust clouds 0-31 pixels.</summary>
    internal static readonly RandomSampleField TorizoSpread = new(0, 5);
}
