using System.Collections.Immutable;

namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>
/// How the debris falling in Ceres's pre-elevator hall during the escape chooses its piece, from the current random
/// sample read without drawing (#1548), and where each piece falls from.
/// </summary>
internal static class CeresFallingDebrisDefinitions
{
    /// <summary>The random selection of the column a piece falls in: one of sixteen.</summary>
    internal static readonly RandomSampleField Column = new(0, 4);

    /// <summary>The random selection, set for a dark piece and clear for a light one.</summary>
    internal static readonly RandomSampleField Dark = new(15, 1);

    /// <summary>
    /// The room X each of the sixteen columns <see cref="Column"/> chooses drops its piece at (#2033): sixteen pixels apart
    /// from 80 through 240, then thirty-two pixels apart through 400.
    /// </summary>
    internal static readonly ImmutableArray<int> ColumnX =
        [80, 96, 112, 128, 144, 160, 176, 192, 208, 224, 240, 272, 304, 336, 368, 400];
}
