using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>Periodic eye-particle definition shared by the two eye handlers.</summary>
public static class DraygonEyeEffects
{
    /// <summary>The eye's breath-bubble steps test the eye frame counter's low seven bits before spawning.</summary>
    public const int CadenceMask = 127;
    /// <summary>The eye's breath-bubble steps pass the long breath-bubble variant to the shared dust initializer.</summary>
    public const MiscDustAnimation DustVariant = MiscDustAnimation.LongDraygonBreathBubbles;
    /// <summary>Both eye handlers offset the particle 24 pixels toward the facing side.</summary>
    public const int HorizontalOffset = 24;
    /// <summary>Both eye handlers place the particle 32 pixels above the eye record.</summary>
    public const int VerticalOffset = -32;
}
