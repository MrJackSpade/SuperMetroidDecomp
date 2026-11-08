namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>
/// How the debris falling in Ceres's pre-elevator hall during the escape chooses its piece, from the current random
/// sample read without drawing (#1548).
/// </summary>
internal static class CeresFallingDebrisDefinitions
{
    /// <summary>The random selection of the column a piece falls in: one of sixteen.</summary>
    internal static readonly RandomSampleField Column = new(0, 4);

    /// <summary>The random selection, set for a dark piece and clear for a light one.</summary>
    internal static readonly RandomSampleField Dark = new(15, 1);
}
