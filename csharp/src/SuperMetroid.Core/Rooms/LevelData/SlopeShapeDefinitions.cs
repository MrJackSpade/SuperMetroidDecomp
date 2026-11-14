namespace SuperMetroid.Core.Rooms.LevelData;

/// <summary>
/// Every slope shape (#1805), each with its surface's height under each pixel column, the share of a grounded horizontal
/// move that crosses it (<see cref="SlopeSpeedDefinitions"/> names the families) and the share of a crawler's tangent
/// speed that carries along it. Heights count pixels down from the block's top: sixteen is a column with no surface in the
/// block, and twenty a surface below it. The coefficients are the game's authored tuning (#1165).
/// </summary>
public static class SlopeShapeDefinitions
{
    /// <summary>A square shape whose bottom half is solid.</summary>
    public static SlopeShape SquareBottomHalf { get; } = new(nameof(SquareBottomHalf), SlopeShapeKind.Square,
        [8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8],
        SlopeSpeedDefinitions.Unscaled, 256);

    /// <summary>A square shape whose right half is solid.</summary>
    public static SlopeShape SquareRightHalf { get; } = new(nameof(SquareRightHalf), SlopeShapeKind.Square,
        [16, 16, 16, 16, 16, 16, 16, 16, 0, 0, 0, 0, 0, 0, 0, 0],
        SlopeSpeedDefinitions.Unscaled, 256);

    /// <summary>A square shape whose bottom right quarter alone is solid.</summary>
    public static SlopeShape SquareBottomRightQuarter { get; } = new(nameof(SquareBottomRightQuarter), SlopeShapeKind.Square,
        [16, 16, 16, 16, 16, 16, 16, 16, 8, 8, 8, 8, 8, 8, 8, 8],
        SlopeSpeedDefinitions.Unscaled, 256);

    /// <summary>A square shape solid in every quarter but the top left.</summary>
    public static SlopeShape SquareAllButTopLeft { get; } = new(nameof(SquareAllButTopLeft), SlopeShapeKind.Square,
        [8, 8, 8, 8, 8, 8, 8, 8, 0, 0, 0, 0, 0, 0, 0, 0],
        SlopeSpeedDefinitions.Unscaled, 256);

    /// <summary>A square shape solid in every quarter.</summary>
    public static SlopeShape SquareFull { get; } = new(nameof(SquareFull), SlopeShapeKind.Square,
        [0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0],
        SlopeSpeedDefinitions.Unscaled, 256);

    /// <summary>A low peak: the surface rises from the block's bottom edges to seven pixels up in its middle.</summary>
    public static SlopeShape LowPeak { get; } = new(nameof(LowPeak), SlopeShapeKind.Profile,
        [16, 15, 14, 13, 12, 11, 10, 9, 9, 10, 11, 12, 13, 14, 15, 16],
        SlopeSpeedDefinitions.Unscaled, 256);

    /// <summary>A high peak: the surface rises from the block's bottom edges to fourteen pixels up in its middle.</summary>
    public static SlopeShape HighPeak { get; } = new(nameof(HighPeak), SlopeShapeKind.Profile,
        [16, 14, 12, 10, 8, 6, 4, 2, 2, 4, 6, 8, 10, 12, 14, 16],
        SlopeSpeedDefinitions.Unscaled, 256);

    /// <summary>A flat surface halfway down the block.</summary>
    public static SlopeShape HalfHeight { get; } = new(nameof(HalfHeight), SlopeShapeKind.Profile,
        [8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8],
        SlopeSpeedDefinitions.Unscaled, 256);

    /// <summary>A flat surface at the block's top that no retail slope uses; the game defines six alike.</summary>
    [NumberedIdentities("The game defines six identical full-height profiles no retail slope uses; the number only tells apart the shapes a block keeps, and nothing indexes them.")]
    public static SlopeShape UnusedSolid1 { get; } = new(nameof(UnusedSolid1), SlopeShapeKind.Profile,
        [0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0],
        SlopeSpeedDefinitions.Unscaled, 256);

    /// <summary>A flat surface at the block's top that no retail slope uses; the game defines six alike.</summary>
    [NumberedIdentities("The game defines six identical full-height profiles no retail slope uses; the number only tells apart the shapes a block keeps, and nothing indexes them.")]
    public static SlopeShape UnusedSolid2 { get; } = new(nameof(UnusedSolid2), SlopeShapeKind.Profile,
        [0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0],
        SlopeSpeedDefinitions.Unscaled, 256);

    /// <summary>A flat surface at the block's top that no retail slope uses; the game defines six alike.</summary>
    [NumberedIdentities("The game defines six identical full-height profiles no retail slope uses; the number only tells apart the shapes a block keeps, and nothing indexes them.")]
    public static SlopeShape UnusedSolid3 { get; } = new(nameof(UnusedSolid3), SlopeShapeKind.Profile,
        [0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0],
        SlopeSpeedDefinitions.Unscaled, 256);

    /// <summary>A flat surface at the block's top that no retail slope uses; the game defines six alike.</summary>
    [NumberedIdentities("The game defines six identical full-height profiles no retail slope uses; the number only tells apart the shapes a block keeps, and nothing indexes them.")]
    public static SlopeShape UnusedSolid4 { get; } = new(nameof(UnusedSolid4), SlopeShapeKind.Profile,
        [0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0],
        SlopeSpeedDefinitions.Unscaled, 256);

    /// <summary>A flat surface at the block's top that no retail slope uses; the game defines six alike.</summary>
    [NumberedIdentities("The game defines six identical full-height profiles no retail slope uses; the number only tells apart the shapes a block keeps, and nothing indexes them.")]
    public static SlopeShape UnusedSolid5 { get; } = new(nameof(UnusedSolid5), SlopeShapeKind.Profile,
        [0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0],
        SlopeSpeedDefinitions.Unscaled, 256);

    /// <summary>A flat surface at the block's top that no retail slope uses; the game defines six alike.</summary>
    [NumberedIdentities("The game defines six identical full-height profiles no retail slope uses; the number only tells apart the shapes a block keeps, and nothing indexes them.")]
    public static SlopeShape UnusedSolid6 { get; } = new(nameof(UnusedSolid6), SlopeShapeKind.Profile,
        [0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0],
        SlopeSpeedDefinitions.Unscaled, 256);

    /// <summary>Stairs rising to the right four pixels a step, every four columns.</summary>
    public static SlopeShape FourPixelStairs { get; } = new(nameof(FourPixelStairs), SlopeShapeKind.Profile,
        [12, 12, 12, 12, 8, 8, 8, 8, 4, 4, 4, 4, 0, 0, 0, 0],
        SlopeSpeedDefinitions.DescendingStairs, 176);

    /// <summary>Stairs rising to the right two pixels a step, every two columns.</summary>
    public static SlopeShape TwoPixelStairs { get; } = new(nameof(TwoPixelStairs), SlopeShapeKind.Profile,
        [14, 14, 12, 12, 10, 10, 8, 8, 6, 6, 4, 4, 2, 2, 0, 0],
        SlopeSpeedDefinitions.DescendingStairs, 176);

    /// <summary>No surface in the block.</summary>
    public static SlopeShape Open { get; } = new(nameof(Open), SlopeShapeKind.Profile,
        [16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16],
        SlopeSpeedDefinitions.Unscaled, 256);

    /// <summary>A surface below the block until its last three columns, which have none in it.</summary>
    public static SlopeShape Overhang { get; } = new(nameof(Overhang), SlopeShapeKind.Profile,
        [20, 20, 20, 20, 20, 20, 20, 20, 20, 20, 20, 20, 20, 16, 16, 16],
        SlopeSpeedDefinitions.Unscaled, 256);

    /// <summary>A slope rising one pixel a column across the block.</summary>
    public static SlopeShape UnitRise { get; } = new(nameof(UnitRise), SlopeShapeKind.Profile,
        [16, 15, 14, 13, 12, 11, 10, 9, 8, 7, 6, 5, 4, 3, 2, 1],
        SlopeSpeedDefinitions.UnitRise, 192);

    /// <summary>A flat surface at the block's top: the slope blocks the Wrecked Ship Chozo statue opens use it.</summary>
    public static SlopeShape Solid { get; } = new(nameof(Solid), SlopeShapeKind.Profile,
        [0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0],
        SlopeSpeedDefinitions.Unscaled, 256);

    /// <summary>The lower block of a slope rising one pixel a column that starts halfway across the block before.</summary>
    public static SlopeShape UnitRiseLower { get; } = new(nameof(UnitRiseLower), SlopeShapeKind.Profile,
        [16, 16, 16, 16, 16, 16, 16, 16, 16, 15, 14, 13, 12, 11, 10, 9],
        SlopeSpeedDefinitions.UnitRise, 192);

    /// <summary>The upper block of a slope rising one pixel a column that starts halfway across the block before.</summary>
    public static SlopeShape UnitRiseUpper { get; } = new(nameof(UnitRiseUpper), SlopeShapeKind.Profile,
        [8, 7, 6, 5, 4, 3, 2, 1, 0, 0, 0, 0, 0, 0, 0, 0],
        SlopeSpeedDefinitions.UnitRise, 192);

    /// <summary>The lower block of a slope rising half a pixel a column over two blocks.</summary>
    public static SlopeShape HalfRiseLower { get; } = new(nameof(HalfRiseLower), SlopeShapeKind.Profile,
        [16, 16, 15, 15, 14, 14, 13, 13, 12, 12, 11, 11, 10, 10, 9, 9],
        SlopeSpeedDefinitions.HalfRise, 216);

    /// <summary>The upper block of a slope rising half a pixel a column over two blocks.</summary>
    public static SlopeShape HalfRiseUpper { get; } = new(nameof(HalfRiseUpper), SlopeShapeKind.Profile,
        [8, 8, 7, 7, 6, 6, 5, 5, 4, 4, 3, 3, 2, 2, 1, 1],
        SlopeSpeedDefinitions.HalfRise, 216);

    /// <summary>The lowest block of a slope rising a third of a pixel a column over three blocks.</summary>
    public static SlopeShape ThirdRiseLower { get; } = new(nameof(ThirdRiseLower), SlopeShapeKind.Profile,
        [16, 16, 16, 15, 15, 15, 14, 14, 14, 13, 13, 13, 12, 12, 12, 11],
        SlopeSpeedDefinitions.ThirdRise, 240);

    /// <summary>The middle block of a slope rising a third of a pixel a column over three blocks.</summary>
    public static SlopeShape ThirdRiseMiddle { get; } = new(nameof(ThirdRiseMiddle), SlopeShapeKind.Profile,
        [11, 11, 10, 10, 10, 9, 9, 9, 8, 8, 8, 7, 7, 7, 6, 6],
        SlopeSpeedDefinitions.ThirdRise, 240);

    /// <summary>The highest block of a slope rising a third of a pixel a column over three blocks.</summary>
    public static SlopeShape ThirdRiseUpper { get; } = new(nameof(ThirdRiseUpper), SlopeShapeKind.Profile,
        [6, 5, 5, 5, 4, 4, 4, 3, 3, 3, 2, 2, 2, 1, 1, 1],
        SlopeSpeedDefinitions.ThirdRise, 240);

    /// <summary>The lower block of a slope rising two pixels a column over two blocks' height.</summary>
    public static SlopeShape DoubleRiseLower { get; } = new(nameof(DoubleRiseLower), SlopeShapeKind.Profile,
        [20, 20, 20, 20, 20, 20, 20, 20, 16, 14, 12, 10, 8, 6, 4, 2],
        SlopeSpeedDefinitions.DoubleRise, 128);

    /// <summary>The upper block of a slope rising two pixels a column over two blocks' height.</summary>
    public static SlopeShape DoubleRiseUpper { get; } = new(nameof(DoubleRiseUpper), SlopeShapeKind.Profile,
        [16, 14, 12, 10, 8, 6, 4, 2, 0, 0, 0, 0, 0, 0, 0, 0],
        SlopeSpeedDefinitions.DoubleRise, 128);

    /// <summary>The lowest block of a slope rising three pixels a column over three blocks' height.</summary>
    public static SlopeShape TripleRiseLower { get; } = new(nameof(TripleRiseLower), SlopeShapeKind.Profile,
        [20, 20, 20, 20, 20, 20, 20, 20, 20, 20, 20, 15, 12, 9, 6, 3],
        SlopeSpeedDefinitions.TripleRise, 80);

    /// <summary>The middle block of a slope rising three pixels a column over three blocks' height.</summary>
    public static SlopeShape TripleRiseMiddle { get; } = new(nameof(TripleRiseMiddle), SlopeShapeKind.Profile,
        [20, 20, 20, 20, 20, 20, 14, 11, 8, 5, 2, 0, 0, 0, 0, 0],
        SlopeSpeedDefinitions.TripleRise, 80);

    /// <summary>The highest block of a slope rising three pixels a column over three blocks' height.</summary>
    public static SlopeShape TripleRiseUpper { get; } = new(nameof(TripleRiseUpper), SlopeShapeKind.Profile,
        [16, 13, 10, 7, 4, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0],
        SlopeSpeedDefinitions.TripleRise, 80);

    /// <summary>Every slope shape, in the order the installation names them.</summary>
    public static IReadOnlyList<SlopeShape> All { get; } =
    [
        SquareBottomHalf,
        SquareRightHalf,
        SquareBottomRightQuarter,
        SquareAllButTopLeft,
        SquareFull,
        LowPeak,
        HighPeak,
        HalfHeight,
        UnusedSolid1,
        UnusedSolid2,
        UnusedSolid3,
        UnusedSolid4,
        UnusedSolid5,
        UnusedSolid6,
        FourPixelStairs,
        TwoPixelStairs,
        Open,
        Overhang,
        UnitRise,
        Solid,
        UnitRiseLower,
        UnitRiseUpper,
        HalfRiseLower,
        HalfRiseUpper,
        ThirdRiseLower,
        ThirdRiseMiddle,
        ThirdRiseUpper,
        DoubleRiseLower,
        DoubleRiseUpper,
        TripleRiseLower,
        TripleRiseMiddle,
        TripleRiseUpper,
    ];
}
