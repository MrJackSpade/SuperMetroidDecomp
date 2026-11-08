namespace SuperMetroid.Core.Rendering;

/// <summary>Where the X-ray beam starts relative to Samus, and its colour.</summary>
public static class XrayWindowRenderDefinitions
{
    /// <summary>The fade level, in every channel, of the grey tint the X-ray blends a revealable room with.</summary>
    public const int RevealTintLevel = 7;
    /// <summary>The beam starts three pixels ahead of Samus in the direction she faces.</summary>
    public const int EyeHorizontalOffset = 3;
    /// <summary>How far above Samus's centre the beam starts while she stands or turns.</summary>
    public const int StandingEyeHeight = 16;
    /// <summary>How far above Samus's centre the beam starts while she crouches.</summary>
    public const int CrouchingEyeHeight = 12;
}
