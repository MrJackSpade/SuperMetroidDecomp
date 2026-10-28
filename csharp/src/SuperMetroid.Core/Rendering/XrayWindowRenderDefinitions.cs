namespace SuperMetroid.Core.Rendering;

/// <summary>Where the X-ray beam starts relative to Samus, its colour, and how its edges are rounded.</summary>
public static class XrayWindowRenderDefinitions
{
    /// <summary>The intensity, in every channel, of the grey X-ray blends a revealable room with.</summary>
    public const int FixedColorComponent = 7;
    /// <summary>The beam starts three pixels ahead of Samus in the direction she faces.</summary>
    public const int EyeHorizontalOffset = 3;
    /// <summary>How far above Samus's centre the beam starts while she stands or turns.</summary>
    public const int StandingEyeHeight = 16;
    /// <summary>How far above Samus's centre the beam starts while she crouches.</summary>
    public const int CrouchingEyeHeight = 12;
    /// <summary>The fraction of a pixel, in 8.8 fixed point, a beam edge drops when it is rounded to a whole column.</summary>
    public const int SubpixelTolerance = 0xFF;
}
