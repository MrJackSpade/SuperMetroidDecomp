namespace SuperMetroid.Core.Game;

/// <summary>One fixed-point environmental damage contribution per accepted frame.</summary>
internal readonly record struct SamusLiquidDamageRate(ushort SubDamage, ushort WholeDamage);

/// <summary>Compiled liquid-damage rates from Samus's fixed bank-$90 physics constants.</summary>
internal static class SamusLiquidDamageDefinitions
{
    /// <summary>Lava rate at <c>$90:9E8B-$9E8E</c>: one half energy per frame.</summary>
    internal static readonly SamusLiquidDamageRate Lava = new(0x8000, 0x0000);

    /// <summary>Acid rate at <c>$90:9E8F-$9E92</c>: one and one-half energy per frame.</summary>
    internal static readonly SamusLiquidDamageRate Acid = new(0x8000, 0x0001);
}

/// <summary>Calculated uniform/progressive frame cadence with narrowly retained original atmospheric exposure choreography. These selected animation holds are not a fluid or damage integration law.</summary>
internal static class SamusAtmosphericAnimationDefinitions
{
    private enum EffectType : byte
    {
        /// <summary>$90:8BA5, AtmosphericGraphics_1_FootstepSplashes.</summary>
        FootstepSplash = 1,
        /// <summary>$90:8BAD, AtmosphericGraphics_2_FootstepSplashes.</summary>
        AlternateFootstepSplash = 2,
        /// <summary>$90:8BB5, AtmosphericGraphics_3_DivingSplash.</summary>
        DivingSplash = 3,
        /// <summary>$90:8BC7, AtmosphericGraphics_4_LavaSurfaceDamage.</summary>
        LavaSurfaceDamage = 4,
        /// <summary>$90:8BCF, AtmosphericGraphics_5_Bubbles.</summary>
        Bubbles = 5,
        /// <summary>$90:8BDF, AtmosphericGraphics_6_Dust.</summary>
        Dust = 6,
        /// <summary>$90:8BE7, AtmosphericGraphics_7_Dust.</summary>
        AlternateDust = 7,
    }

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
    internal static byte FrameCount(byte type) => (EffectType)type switch
    {
        EffectType.DivingSplash => checked((byte)RetainedDivingSplashTimers.Length),
        EffectType.Bubbles => 8,
        EffectType.FootstepSplash or EffectType.AlternateFootstepSplash or EffectType.LavaSurfaceDamage
            or EffectType.Dust or EffectType.AlternateDust => 4,
        _ => throw new InvalidDataException($"Atmospheric animation type {type} is outside active types one through seven."),
    };

    /// <summary>Uniform splash/lava/bubble cadence and linearly increasing dust holds, with independent diving-splash input.</summary>
    internal static ushort FrameTimer(byte type, byte frame)
    {
        byte count = FrameCount(type);
        if (frame >= count)
            throw new InvalidDataException($"Atmospheric type {type} frame {frame} is outside {count} authored frames.");
        return (EffectType)type switch
        {
            EffectType.FootstepSplash or EffectType.AlternateFootstepSplash => FootstepFrameTicks,
            EffectType.DivingSplash => RetainedDivingSplashTimers[frame],
            EffectType.LavaSurfaceDamage => LavaSurfaceFrameTicks,
            EffectType.Bubbles => BubbleFrameTicks,
            _ => (ushort)(DustInitialFrameTicks + DustFrameTickIncrement * frame),
        };
    }
}