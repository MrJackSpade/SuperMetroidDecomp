namespace SuperMetroid.Core.Samus.Drawing;

/// <summary>Calculated uniform/progressive frame cadence with narrowly retained original atmospheric exposure choreography. These selected animation holds are not a fluid or damage integration law.</summary>
internal static class SamusAtmosphericAnimationDefinitions
{
    /// <summary>$90:8BA5/8BAD uniform footstep hold3; the exact selected exposure is retained under the reviewed original-animation disposition.</summary>
    private const ushort FootstepFrameTicks = 3;
    /// <summary>$90:8BC7 uniform lava-surface hold2; the exact selected exposure is retained under the reviewed original-animation disposition.</summary>
    private const ushort LavaSurfaceFrameTicks = 2;
    /// <summary>$90:8BCF uniform bubble hold5; the exact selected exposure is retained under the reviewed original-animation disposition.</summary>
    private const ushort BubbleFrameTicks = 5;
    /// <summary>$90:8BDF/8BE7 starts dust holds at3; this exact initial exposure is retained under the reviewed original-animation disposition.</summary>
    private const ushort DustInitialFrameTicks = 3;
    /// <summary>$90:8BDF..8BED advances dust holds by1 tick per frame; the exact progression increment is retained under the reviewed original-animation disposition.</summary>
    private const ushort DustFrameTickIncrement = 1;
    /// <summary>$90:8BB5..8BC6, nine retained exposure choices for the drawn ripple/jet/droplet choreography; discrete frame art does not determine these holds. Preserve old-frame reload, slot admission/sound-RNG consequences and terminal stale7.</summary>
    private static readonly ushort[] RetainedDivingSplashTimers = [2, 2, 3, 3, 3, 5, 5, 6, 7];

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
        byte count = FrameCount(type);
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
