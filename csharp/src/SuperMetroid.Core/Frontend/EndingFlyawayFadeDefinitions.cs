namespace SuperMetroid.Core.Frontend;

internal static class EndingFlyawayFadeDefinitions
{
    /// <summary>Func120 sets COLDATA to maximum white on each five-bit component.</summary>
    public const byte InitialWhite = 31;
    /// <summary>Func120 initializes the first Func122 fade countdown to one.</summary>
    public const int InitialCountdown = 1;
    /// <summary>Func122 decreases fixed white once every eight calls thereafter.</summary>
    public const int Interval = 8;
}
