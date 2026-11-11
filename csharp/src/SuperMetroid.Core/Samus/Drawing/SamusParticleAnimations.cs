using static SuperMetroid.Core.Samus.Drawing.SamusAtmosphericAnimationDefinitions;

namespace SuperMetroid.Core.Samus.Drawing;

/// <summary>The steps of each particle's animation, how long each shows, and how long a particle is shown in all.</summary>
internal static class SamusParticleAnimations
{
    /// <summary>The animation steps <paramref name="kind"/> shows.</summary>
    internal static int StepCount(SamusParticleKind kind) => kind switch
    {
        SamusParticleKind.DivingSplash => DivingSplashStepUpdates.Count,
        SamusParticleKind.Bubbles => 8,
        SamusParticleKind.SurfaceSplash or SamusParticleKind.LavaSpray or SamusParticleKind.Dust => 4,
        _ => throw new InvalidDataException($"Undefined particle kind {kind}."),
    };

    /// <summary>
    /// The updates step <paramref name="step"/> of <paramref name="kind"/> lasts: uniform splash, lava and bubble holds,
    /// linearly lengthening dust holds, and the diving splash's authored holds.
    /// </summary>
    internal static int StepUpdates(SamusParticleKind kind, int step)
    {
        int count = StepCount(kind);
        if (step < 0 || step >= count)
            throw new InvalidDataException($"Particle kind {kind} step {step} is outside its {count} steps.");
        return kind switch
        {
            SamusParticleKind.SurfaceSplash => FootstepFrameTicks,
            SamusParticleKind.DivingSplash => DivingSplashStepUpdates[step],
            SamusParticleKind.LavaSpray => LavaSurfaceFrameTicks,
            SamusParticleKind.Bubbles => BubbleFrameTicks,
            SamusParticleKind.Dust => DustInitialFrameTicks + DustFrameTickIncrement * step,
            _ => throw new InvalidDataException($"Undefined particle kind {kind}."),
        };
    }

    /// <summary>
    /// The updates a particle of <paramref name="kind"/> on step <paramref name="step"/>, begun as <paramref name="start"/>
    /// says, lasts until it runs out, counting the update it runs out on. A step that ends takes the duration of the step
    /// it ends, not of the step it moves to, so each step after the current one lasts as long as the step before it; the
    /// last step's own duration is never used.
    /// </summary>
    internal static int UpdatesUntilFinished(SamusParticleKind kind, int step, SamusParticleStart start)
    {
        int count = StepCount(kind);
        if (step < 0 || step >= count)
            throw new InvalidDataException($"Particle kind {kind} step {step} is outside its {count} steps.");
        if ((start.StepUpdates > 0) == (start.HiddenUpdates > 0))
            throw new ArgumentOutOfRangeException(nameof(start), start, "A particle either shows at once or starts hidden.");
        int later = 0;
        for (int earlier = step; earlier < count - 1; earlier++)
            later += StepUpdates(kind, earlier);
        return start.HiddenUpdates > 0
            ? start.HiddenUpdates + StepUpdates(kind, step) + later
            : start.StepUpdates + later;
    }
}
