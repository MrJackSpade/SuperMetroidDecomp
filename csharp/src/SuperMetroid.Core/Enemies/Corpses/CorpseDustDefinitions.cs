namespace SuperMetroid.Core.Enemies.Corpses;

/// <summary>The random bits that spread a dying corpse's dust clouds.</summary>
internal static class CorpseDustDefinitions
{
    /// <summary>
    /// The random bit that moves a Tourian or Sidehopper corpse's dust cloud one <see cref="SpreadFineStep"/> to the
    /// right: bit one. With <see cref="SpreadCoarseSteps"/> it spreads the cloud 0-26 pixels, even.
    /// </summary>
    internal static readonly RandomSampleField SpreadFineSteps = new(1, 1);

    /// <summary>
    /// The random field that moves a Tourian or Sidehopper corpse's dust cloud zero to three
    /// <see cref="SpreadCoarseStep"/>s to the right: bits three and four.
    /// </summary>
    internal static readonly RandomSampleField SpreadCoarseSteps = new(3, 2);

    /// <summary>The pixels one fine spread step moves a Tourian or Sidehopper corpse's dust cloud.</summary>
    internal const int SpreadFineStep = 2;

    /// <summary>The pixels one coarse spread step moves a Tourian or Sidehopper corpse's dust cloud.</summary>
    internal const int SpreadCoarseStep = 8;

    /// <summary>The random field that spreads Dead Torizo's dust clouds 0-31 pixels: bits zero through four.</summary>
    internal static readonly RandomSampleField TorizoSpread = new(0, 5);
}
