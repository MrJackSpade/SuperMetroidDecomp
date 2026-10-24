namespace SuperMetroid.Rendering.Direct3D11.Presentation;

/// <summary>The desktop window's display aspect, distinct from the native picture's pixel dimensions.</summary>
internal static class DisplayAspect
{
    /// <summary>Horizontal component of the desktop's 4:3 TV correction.</summary>
    internal const int Numerator = 4;
    /// <summary>Vertical component of the desktop's 4:3 TV correction.</summary>
    internal const int Denominator = 3;
}
