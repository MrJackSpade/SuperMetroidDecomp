namespace SuperMetroid.Core.Rooms.LevelData;

/// <summary>The shares of a grounded horizontal move that cross each family of slope shapes (<see cref="SlopeShapeDefinitions"/>), in 256ths.</summary>
public static class SlopeSpeedDefinitions
{
    /// <summary>The whole distance, 256/256, for the square, flat, peaked, open and overhanging shapes.</summary>
    /// <remarks>Family membership follows the authored height profiles. The coefficients are authored tuning: cos(atan r),
    /// 1/(1+r^2) and linear-in-rise all fail to reproduce them (see the review inventory's residual scalar inputs).</remarks>
    internal const int Unscaled = 256;
    /// <summary>Eleven sixteenths of the distance, 176/256, for the four-pixel and two-pixel stairs.</summary>
    internal const int DescendingStairs = 176;
    /// <summary>Three quarters of the distance, 192/256, for the slopes rising one pixel a column.</summary>
    internal const int UnitRise = 192;
    /// <summary>Twenty-seven thirty-seconds of the distance, 216/256, for the two blocks of the slope rising half a pixel a column.</summary>
    internal const int HalfRise = 216;
    /// <summary>Fifteen sixteenths of the distance, 240/256, for the three blocks of the slope rising a third of a pixel a column.</summary>
    internal const int ThirdRise = 240;
    /// <summary>Half the distance, 128/256, for the two blocks of the slope rising two pixels a column.</summary>
    internal const int DoubleRise = 128;
    /// <summary>Five sixteenths of the distance, 80/256, for the three blocks of the slope rising three pixels a column.</summary>
    internal const int TripleRise = 80;
}
