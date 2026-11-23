namespace SuperMetroid.Core.Rooms.LevelData;

/// <summary>
/// The shares of a grounded horizontal move, and of a crawler's tangent speed, that cross each family of slope shapes
/// (<see cref="SlopeShapeDefinitions"/>).
/// </summary>
public static partial class SlopeSpeedDefinitions
{
    /// <summary>The whole distance, for the square, flat, peaked, open and overhanging shapes.</summary>
    /// <remarks>Family membership follows the authored height profiles. The shares are authored tuning, not a function of a
    /// slope's rise: neither cos(atan r), 1/(1+r^2) nor a share linear in the rise gives them.</remarks>
    internal static SpeedShare Unscaled { get; } = SpeedShare.Whole;
    /// <summary>Eleven sixteenths of the distance, for the four-pixel and two-pixel stairs.</summary>
    internal static SpeedShare DescendingStairs { get; } = SpeedShare.Of(11, 16);
    /// <summary>Three quarters of the distance, for the slopes rising one pixel a column.</summary>
    internal static SpeedShare UnitRise { get; } = SpeedShare.Of(3, 4);
    /// <summary>Twenty-seven thirty-seconds of the distance, for the two blocks of the slope rising half a pixel a column.</summary>
    internal static SpeedShare HalfRise { get; } = SpeedShare.Of(27, 32);
    /// <summary>Fifteen sixteenths of the distance, for the three blocks of the slope rising a third of a pixel a column.</summary>
    internal static SpeedShare ThirdRise { get; } = SpeedShare.Of(15, 16);
    /// <summary>Half the distance, for the two blocks of the slope rising two pixels a column.</summary>
    internal static SpeedShare DoubleRise { get; } = SpeedShare.Of(1, 2);
    /// <summary>Five sixteenths of the distance, for the three blocks of the slope rising three pixels a column.</summary>
    internal static SpeedShare TripleRise { get; } = SpeedShare.Of(5, 16);
}
