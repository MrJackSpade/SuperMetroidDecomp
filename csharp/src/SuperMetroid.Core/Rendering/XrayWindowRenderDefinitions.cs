using SuperMetroid.Core.Assets;
namespace SuperMetroid.Core.Rendering;

/// <summary>Where the X-ray beam starts relative to Samus, its tint and the backdrop it shows.</summary>
public static partial class XrayWindowRenderDefinitions
{
    /// <summary>The grey tint the X-ray blends a revealable room with.</summary>
    public static FadeLevelColour RevealTint { get; } = new(7, 7, 7);
    /// <summary>The tint of a room the X-ray does not tint: black, which neither brightens nor darkens.</summary>
    public static FadeLevelColour NoTint { get; } = new(0, 0, 0);
    /// <summary>The dark grey backdrop shown while the X-ray beam is out.</summary>
    public static RgbColor ActiveBackdrop { get; } = new(24, 24, 24);
    /// <summary>The beam starts three pixels ahead of Samus in the direction she faces.</summary>
    public const int EyeHorizontalOffset = 3;
    /// <summary>How far above Samus's centre the beam starts while she stands or turns.</summary>
    public const int StandingEyeHeight = 16;
    /// <summary>How far above Samus's centre the beam starts while she crouches.</summary>
    public const int CrouchingEyeHeight = 12;
}
