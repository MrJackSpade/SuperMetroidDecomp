namespace SuperMetroid.Core.Enemies.Corpses;

/// <summary>The random choices that spread a dying corpse's dust clouds: how far right each cloud lies.</summary>
internal static class CorpseDustDefinitions
{

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
    internal static readonly RandomSampleField TorizoSpread = RandomSampleField.OneOf(32);
}
