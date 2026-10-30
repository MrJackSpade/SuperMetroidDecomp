namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>How Phantoon's enraged and spiral flames move and sound (the NTSC Japan/USA values; PAL values differ).</summary>
internal static class PhantoonFlameMotionDefinitions
{
    /// <summary>$86:9885/$988D, enraged initializer: signed clockwise/counterclockwise angle step magnitude.</summary>
    internal const int RageAngleStep = 2;
    /// <summary>Updates an intro flame circles at its full radius before the circle starts closing.</summary>
    internal const int IntroCircleDelay = 180;
    /// <summary>The radius, in pixels, an intro flame starts circling at; it closes by one every other update.</summary>
    internal const int IntroCircleRadius = 48;
    /// <summary>$86:9A49, enraged pre-instruction: radius increase per frame.</summary>
    internal const ushort RageRadiusStep = 4;
    /// <summary>$86:9ADE, spiral pre-instruction: radius increase per frame.</summary>
    internal const ushort SpiralRadiusStep = 2;
    /// <summary>$86:9AE8, spiral pre-instruction: clockwise angle increase per frame.</summary>
    internal const ushort SpiralAngleStep = 2;
    /// <summary>$86:9AA0/$9AD2: sound requested when rain starts falling and when it hits terrain.</summary>
    internal static SoundEffectId RainFallSound => Library3Sound.PhantoonFlame;
    /// <summary>$86:9AA3/$9AD5 call QueueSound_Lib3_Max6.</summary>
    internal static readonly SoundQueueLimit RainSoundQueueCapacity = SoundQueueLimit.Max6;
}
