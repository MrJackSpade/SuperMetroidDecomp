using static SuperMetroid.Core.Samus.Drawing.SamusAtmosphericAnimationDefinitions;

namespace SuperMetroid.Core.Samus.Drawing;

/// <summary>The frame count and timer of each atmospheric animation.</summary>
internal static class SamusAtmosphericAnimations
{
    /// <summary>$90:8BEF..8BFE supplies each effect's native frame domain.</summary>
    internal static byte FrameCount(SamusAtmosphericEffectType type) => type switch
    {
        SamusAtmosphericEffectType.DivingSplash => checked((byte)RetainedDivingSplashTimers.Length),
        SamusAtmosphericEffectType.Bubbles => 8,
        SamusAtmosphericEffectType.FootstepSplash or SamusAtmosphericEffectType.AlternateFootstepSplash or
            SamusAtmosphericEffectType.LavaSurfaceDamage or SamusAtmosphericEffectType.Dust or
            SamusAtmosphericEffectType.AlternateDust => 4,
        SamusAtmosphericEffectType.Inactive or _ => throw new InvalidDataException($"Atmospheric animation type {type} is outside active types one through seven."),
    };

    /// <summary>Uniform splash/lava/bubble cadence and linearly increasing dust holds, with independent diving-splash input.</summary>
    internal static ushort FrameTimer(SamusAtmosphericEffectType type, int frame)
    {
        int count = FrameCount(type);
        if (frame < 0 || frame >= count)
            throw new InvalidDataException($"Atmospheric type {type} frame {frame} is outside {count} authored frames.");
        return type switch
        {
            SamusAtmosphericEffectType.FootstepSplash or SamusAtmosphericEffectType.AlternateFootstepSplash => FootstepFrameTicks,
            SamusAtmosphericEffectType.DivingSplash => RetainedDivingSplashTimers[frame],
            SamusAtmosphericEffectType.LavaSurfaceDamage => LavaSurfaceFrameTicks,
            SamusAtmosphericEffectType.Bubbles => BubbleFrameTicks,
            SamusAtmosphericEffectType.Dust or SamusAtmosphericEffectType.AlternateDust =>
                (ushort)(DustInitialFrameTicks + DustFrameTickIncrement * frame),
            SamusAtmosphericEffectType.Inactive or _ =>
                throw new InvalidOperationException($"Undefined atmospheric type {type}."),
        };
    }
}
