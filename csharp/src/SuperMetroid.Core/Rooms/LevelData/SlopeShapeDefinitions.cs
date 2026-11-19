namespace SuperMetroid.Core.Rooms.LevelData;

/// <summary>
/// Every slope shape (#1805), each with its surface's height under each pixel column, the share of a grounded horizontal
/// move that crosses it (<see cref="SlopeSpeedDefinitions"/> names the families) and the share of a crawler's tangent
/// speed that carries along it. Heights count pixels down from the block's top: sixteen is a column whose surface is the
/// block's bottom edge, with nothing solid in the block, and twenty a surface four pixels below it. Both are distances the
/// collision arithmetic subtracts from and compares like any other height, not markers standing for a missing surface
/// (#2558). The coefficients are the game's authored tuning (#1165).
/// </summary>
public static class SlopeShapeDefinitions
{
    /// <summary>A square shape whose bottom half is solid.</summary>
    public static SlopeShape SquareBottomHalf { get; } = new("SquareBottomHalf", SlopeShapeKind.Square,
        [8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8],
        SlopeSpeedDefinitions.Unscaled, SlopeSpeedDefinitions.Unscaled);

    /// <summary>A square shape whose right half is solid.</summary>
    public static SlopeShape SquareRightHalf { get; } = new("SquareRightHalf", SlopeShapeKind.Square,
        [16, 16, 16, 16, 16, 16, 16, 16, 0, 0, 0, 0, 0, 0, 0, 0],
        SlopeSpeedDefinitions.Unscaled, SlopeSpeedDefinitions.Unscaled);

    /// <summary>A square shape whose bottom right quarter alone is solid.</summary>
    public static SlopeShape SquareBottomRightQuarter { get; } = new("SquareBottomRightQuarter", SlopeShapeKind.Square,
        [16, 16, 16, 16, 16, 16, 16, 16, 8, 8, 8, 8, 8, 8, 8, 8],
        SlopeSpeedDefinitions.Unscaled, SlopeSpeedDefinitions.Unscaled);

    /// <summary>A square shape solid in every quarter but the top left.</summary>
    public static SlopeShape SquareAllButTopLeft { get; } = new("SquareAllButTopLeft", SlopeShapeKind.Square,
        [8, 8, 8, 8, 8, 8, 8, 8, 0, 0, 0, 0, 0, 0, 0, 0],
        SlopeSpeedDefinitions.Unscaled, SlopeSpeedDefinitions.Unscaled);

    /// <summary>A square shape solid in every quarter.</summary>
    public static SlopeShape SquareFull { get; } = new("SquareFull", SlopeShapeKind.Square,
        [0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0],
        SlopeSpeedDefinitions.Unscaled, SlopeSpeedDefinitions.Unscaled);

    /// <summary>A low peak: the surface rises from the block's bottom edges to seven pixels up in its middle.</summary>
    public static SlopeShape LowPeak { get; } = new("LowPeak", SlopeShapeKind.Profile,
        [16, 15, 14, 13, 12, 11, 10, 9, 9, 10, 11, 12, 13, 14, 15, 16],
        SlopeSpeedDefinitions.Unscaled, SlopeSpeedDefinitions.Unscaled);

    /// <summary>A high peak: the surface rises from the block's bottom edges to fourteen pixels up in its middle.</summary>
    public static SlopeShape HighPeak { get; } = new("HighPeak", SlopeShapeKind.Profile,
        [16, 14, 12, 10, 8, 6, 4, 2, 2, 4, 6, 8, 10, 12, 14, 16],
        SlopeSpeedDefinitions.Unscaled, SlopeSpeedDefinitions.Unscaled);

    /// <summary>A flat surface halfway down the block.</summary>
    public static SlopeShape HalfHeight { get; } = new("HalfHeight", SlopeShapeKind.Profile,
        [8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8],
        SlopeSpeedDefinitions.Unscaled, SlopeSpeedDefinitions.Unscaled);

    /// <summary>
    /// A flat surface at the block's top that no slope uses, numbered six ways by the game. It is the surface of
    /// <see cref="Solid"/>, kept apart from it because a block keeps its behaviour through a change of collision kind
    /// (<see cref="SuperMetroid.Core.Compatibility.BlockParameterCarryOver{TBehavior, TReading}"/>): a slope written as
    /// <see cref="Solid"/> keeps that shape's own behaviour, which this shape's would not (#1909).
    /// </summary>
    public static SlopeShape UnusedSolid { get; } = new("UnusedSolid", SlopeShapeKind.Profile,
        [0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0],
        SlopeSpeedDefinitions.Unscaled, SlopeSpeedDefinitions.Unscaled);

    /// <summary>Stairs rising to the right four pixels a step, every four columns.</summary>
    public static SlopeShape FourPixelStairs { get; } = new("FourPixelStairs", SlopeShapeKind.Profile,
        [12, 12, 12, 12, 8, 8, 8, 8, 4, 4, 4, 4, 0, 0, 0, 0],
        SlopeSpeedDefinitions.DescendingStairs, SlopeSpeedDefinitions.DescendingStairs);

    /// <summary>Stairs rising to the right two pixels a step, every two columns.</summary>
    public static SlopeShape TwoPixelStairs { get; } = new("TwoPixelStairs", SlopeShapeKind.Profile,
        [14, 14, 12, 12, 10, 10, 8, 8, 6, 6, 4, 4, 2, 2, 0, 0],
        SlopeSpeedDefinitions.DescendingStairs, SlopeSpeedDefinitions.DescendingStairs);

    /// <summary>No surface in the block.</summary>
    public static SlopeShape Open { get; } = new("Open", SlopeShapeKind.Profile,
        [16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16],
        SlopeSpeedDefinitions.Unscaled, SlopeSpeedDefinitions.Unscaled);

    /// <summary>A surface below the block until its last three columns, which have none in it.</summary>
    public static SlopeShape Overhang { get; } = new("Overhang", SlopeShapeKind.Profile,
        [20, 20, 20, 20, 20, 20, 20, 20, 20, 20, 20, 20, 20, 16, 16, 16],
        SlopeSpeedDefinitions.Unscaled, SlopeSpeedDefinitions.Unscaled);

    /// <summary>A slope rising one pixel a column across the block.</summary>
    public static SlopeShape UnitRise { get; } = new("UnitRise", SlopeShapeKind.Profile,
        [16, 15, 14, 13, 12, 11, 10, 9, 8, 7, 6, 5, 4, 3, 2, 1],
        SlopeSpeedDefinitions.UnitRise, SlopeSpeedDefinitions.UnitRise);

    /// <summary>A flat surface at the block's top: the slope blocks the Wrecked Ship Chozo statue opens use it.</summary>
    public static SlopeShape Solid { get; } = new("Solid", SlopeShapeKind.Profile,
        [0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0],
        SlopeSpeedDefinitions.Unscaled, SlopeSpeedDefinitions.Unscaled);

    /// <summary>The lower block of a slope rising one pixel a column that starts halfway across the block before.</summary>
    public static SlopeShape UnitRiseLower { get; } = new("UnitRiseLower", SlopeShapeKind.Profile,
        [16, 16, 16, 16, 16, 16, 16, 16, 16, 15, 14, 13, 12, 11, 10, 9],
        SlopeSpeedDefinitions.UnitRise, SlopeSpeedDefinitions.UnitRise);

    /// <summary>The upper block of a slope rising one pixel a column that starts halfway across the block before.</summary>
    public static SlopeShape UnitRiseUpper { get; } = new("UnitRiseUpper", SlopeShapeKind.Profile,
        [8, 7, 6, 5, 4, 3, 2, 1, 0, 0, 0, 0, 0, 0, 0, 0],
        SlopeSpeedDefinitions.UnitRise, SlopeSpeedDefinitions.UnitRise);

    /// <summary>The lower block of a slope rising half a pixel a column over two blocks.</summary>
    public static SlopeShape HalfRiseLower { get; } = new("HalfRiseLower", SlopeShapeKind.Profile,
        [16, 16, 15, 15, 14, 14, 13, 13, 12, 12, 11, 11, 10, 10, 9, 9],
        SlopeSpeedDefinitions.HalfRise, SlopeSpeedDefinitions.HalfRise);

    /// <summary>The upper block of a slope rising half a pixel a column over two blocks.</summary>
    public static SlopeShape HalfRiseUpper { get; } = new("HalfRiseUpper", SlopeShapeKind.Profile,
        [8, 8, 7, 7, 6, 6, 5, 5, 4, 4, 3, 3, 2, 2, 1, 1],
        SlopeSpeedDefinitions.HalfRise, SlopeSpeedDefinitions.HalfRise);

    /// <summary>The lowest block of a slope rising a third of a pixel a column over three blocks.</summary>
    public static SlopeShape ThirdRiseLower { get; } = new("ThirdRiseLower", SlopeShapeKind.Profile,
        [16, 16, 16, 15, 15, 15, 14, 14, 14, 13, 13, 13, 12, 12, 12, 11],
        SlopeSpeedDefinitions.ThirdRise, SlopeSpeedDefinitions.ThirdRise);

    /// <summary>The middle block of a slope rising a third of a pixel a column over three blocks.</summary>
    public static SlopeShape ThirdRiseMiddle { get; } = new("ThirdRiseMiddle", SlopeShapeKind.Profile,
        [11, 11, 10, 10, 10, 9, 9, 9, 8, 8, 8, 7, 7, 7, 6, 6],
        SlopeSpeedDefinitions.ThirdRise, SlopeSpeedDefinitions.ThirdRise);

    /// <summary>The highest block of a slope rising a third of a pixel a column over three blocks.</summary>
    public static SlopeShape ThirdRiseUpper { get; } = new("ThirdRiseUpper", SlopeShapeKind.Profile,
        [6, 5, 5, 5, 4, 4, 4, 3, 3, 3, 2, 2, 2, 1, 1, 1],
        SlopeSpeedDefinitions.ThirdRise, SlopeSpeedDefinitions.ThirdRise);

    /// <summary>The lower block of a slope rising two pixels a column over two blocks' height.</summary>
    public static SlopeShape DoubleRiseLower { get; } = new("DoubleRiseLower", SlopeShapeKind.Profile,
        [20, 20, 20, 20, 20, 20, 20, 20, 16, 14, 12, 10, 8, 6, 4, 2],
        SlopeSpeedDefinitions.DoubleRise, SlopeSpeedDefinitions.DoubleRise);

    /// <summary>The upper block of a slope rising two pixels a column over two blocks' height.</summary>
    public static SlopeShape DoubleRiseUpper { get; } = new("DoubleRiseUpper", SlopeShapeKind.Profile,
        [16, 14, 12, 10, 8, 6, 4, 2, 0, 0, 0, 0, 0, 0, 0, 0],
        SlopeSpeedDefinitions.DoubleRise, SlopeSpeedDefinitions.DoubleRise);

    /// <summary>The lowest block of a slope rising three pixels a column over three blocks' height.</summary>
    public static SlopeShape TripleRiseLower { get; } = new("TripleRiseLower", SlopeShapeKind.Profile,
        [20, 20, 20, 20, 20, 20, 20, 20, 20, 20, 20, 15, 12, 9, 6, 3],
        SlopeSpeedDefinitions.TripleRise, SlopeSpeedDefinitions.TripleRise);

    /// <summary>The middle block of a slope rising three pixels a column over three blocks' height.</summary>
    public static SlopeShape TripleRiseMiddle { get; } = new("TripleRiseMiddle", SlopeShapeKind.Profile,
        [20, 20, 20, 20, 20, 20, 14, 11, 8, 5, 2, 0, 0, 0, 0, 0],
        SlopeSpeedDefinitions.TripleRise, SlopeSpeedDefinitions.TripleRise);

    /// <summary>The highest block of a slope rising three pixels a column over three blocks' height.</summary>
    public static SlopeShape TripleRiseUpper { get; } = new("TripleRiseUpper", SlopeShapeKind.Profile,
        [16, 13, 10, 7, 4, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0],
        SlopeSpeedDefinitions.TripleRise, SlopeSpeedDefinitions.TripleRise);

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
        UnusedSolid,
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
