namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>The form of one Zebetite barrier generation (#2012). Its values are arbitrary.</summary>
public enum ZebetiteBarrierForm
{
    /// <summary>One large barrier on the generation's centre line.</summary>
    Single,
    /// <summary>
    /// Two smaller halves, above and below the generation's centre line: the primary half spawns its linked second half.
    /// </summary>
    SplitPair,
}
