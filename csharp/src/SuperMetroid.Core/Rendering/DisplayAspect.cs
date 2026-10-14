namespace SuperMetroid.Core.Rendering;

/// <summary>Consumer display aspect, distinct from the native SNES sample dimensions.</summary>
internal static class DisplayAspect
{
    /// <summary>Horizontal component of the desktop's 4:3 TV correction.</summary>
    internal const int Numerator = 4;
    /// <summary>Vertical component of the desktop's 4:3 TV correction.</summary>
    internal const int Denominator = 3;
}
