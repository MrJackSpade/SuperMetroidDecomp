namespace SuperMetroid.Core.Rendering;

/// <summary>Native X-ray eye position and fixed-point raster units.</summary>
public static class XrayWindowRenderDefinitions
{
    /// <summary>$88:8709-$8716 sets each COLDATA component to seven for revealable rooms.</summary>
    public const int FixedColorComponent = 7;
    /// <summary>$88:88B8-$88F3 places the eye three pixels toward Samus's facing direction.</summary>
    public const int EyeHorizontalOffset = 3;
    /// <summary>$88:88B8-$88F3 standing/turning eye height above Samus's center.</summary>
    public const int StandingEyeHeight = 16;
    /// <summary>$88:88B8-$88F3 stable crouching eye height above Samus's center.</summary>
    public const int CrouchingEyeHeight = 12;
    /// <summary>$91:C901 discards the fractional byte of the 8.8 scanline accumulator.</summary>
    public const int SubpixelTolerance = 0xFF;
}
