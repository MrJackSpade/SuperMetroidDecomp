using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>Periodic eye-particle definition shared by both eyes' particle steps.</summary>
public static partial class DraygonEyeEffects
{
    /// <summary>The eye's breath-bubble steps pass the long breath-bubble variant to the shared dust setup.</summary>
    public const DustExplosionAnimation DustVariant = DustExplosionAnimation.LongDraygonBreathBubbles;
    /// <summary>How far both eyes offset the particle toward the facing side.</summary>
    public const int HorizontalOffset = 24;
    /// <summary>How far above the eye both eyes place the particle.</summary>
    public const int VerticalOffset = -32;
}
