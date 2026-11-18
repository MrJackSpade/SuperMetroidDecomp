using System.Collections.Immutable;

namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>
/// How the debris falling in Ceres's pre-elevator hall during the escape chooses its piece, from the current random
/// sample read without drawing (#1548), where each piece falls from and how it falls.
/// </summary>
internal static class CeresFallingDebrisDefinitions
{
    /// <summary>The random selection of the column a piece falls in: one of sixteen.</summary>
    internal static readonly RandomSampleField Column = new(0, 4);

    /// <summary>
    /// Whether a falling piece is dark rather than light (#2479): taken by a sample of 32768 or more, one piece in two.
    /// </summary>
    internal static readonly RandomChance PieceIsDark = RandomChance.Highest(1, new RandomSampleField(15, 1));

    /// <summary>
    /// The room X each of the sixteen columns <see cref="Column"/> chooses drops its piece at (#2033): sixteen pixels apart
    /// from 80 through 240, then thirty-two pixels apart through 400.
    /// </summary>
    internal static readonly ImmutableArray<int> ColumnX =
        [80, 96, 112, 128, 144, 160, 176, 192, 208, 224, 240, 272, 304, 336, 368, 400];

    /// <summary>The room Y, in whole pixels, every piece starts its fall at, whatever its column.</summary>
    internal const int StartingY = 42;

    /// <summary>The downward speed a piece starts its fall with: a sixteenth of a pixel per update.</summary>
    internal static readonly SignedEightEight StartingFallSpeed = SignedEightEight.FromParts(0, 16);

    /// <summary>
    /// The speed a falling piece gains each update before it moves: a sixteenth of a pixel per update, so its first
    /// update already moves it an eighth of a pixel.
    /// </summary>
    internal static readonly SignedEightEight FallAcceleration = SignedEightEight.FromParts(0, 16);
}
