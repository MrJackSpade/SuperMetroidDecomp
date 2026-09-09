namespace SuperMetroid.Core.Frontend;

/// <summary>Func115–117's shared palette/camera transition constants.</summary>
internal static class EndingExplosionFadeDefinitions
{
    /// <summary>Func115 seeds var4 to 63; Func117 exits when its decrement becomes negative.</summary>
    public const int InitialCountdown = 63;
    /// <summary>Func117 increments the Mode-7 scale by four per call.</summary>
    public const int ZoomStep = 4;
    /// <summary>Func117 fades CGRAM colors $70–7F out.</summary>
    public const int BackgroundStart = 112;
    /// <summary>Func115 clears, and Func117 fades in, OBJ colors $D0–DF.</summary>
    public const int FirstObjectStart = 208;
    /// <summary>Func115 clears, and Func117 fades in, OBJ colors $F0–FF.</summary>
    public const int SecondObjectStart = 240;
    /// <summary>Each affected palette contains sixteen colors.</summary>
    public const int Colors = 16;
    /// <summary>There are thirty-two palette steps over sixty-four calls.</summary>
    public const int Steps = 32;
}
