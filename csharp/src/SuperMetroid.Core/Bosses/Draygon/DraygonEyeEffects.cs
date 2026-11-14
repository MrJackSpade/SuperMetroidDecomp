using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>Periodic eye-particle definition shared by both eyes' particle steps.</summary>
public static class DraygonEyeEffects
{
    /// <summary>The eye's breath-bubble steps pass the long breath-bubble variant to the shared dust initializer.</summary>
    public const MiscDustAnimation DustVariant = MiscDustAnimation.LongDraygonBreathBubbles;
    /// <summary>Both eyes offset the particle 24 pixels toward the facing side.</summary>
    public const int HorizontalOffset = 24;
    /// <summary>Both eyes place the particle 32 pixels above the eye.</summary>
    public const int VerticalOffset = -32;
}
