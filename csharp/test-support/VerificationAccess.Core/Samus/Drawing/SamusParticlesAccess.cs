using System.Reflection;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Samus.Artwork;

namespace SuperMetroid.Core.Samus.Drawing;

/// <summary>
/// Verification access to the particles Samus's presentation shows (#1556). The presentation only advances and draws
/// them; checks read the list to assert each particle's kind, step and countdown, the step and countdown read from its
/// animation as the original counts them (#2388).
/// </summary>
internal static class SamusParticlesAccess
{
    /// <summary>Exposes the particle list.</summary>
    extension(SamusParticles self)
    {
        /// <summary>The particles shown or waiting to show, oldest first.</summary>
        internal IReadOnlyList<SamusParticle> All =>
            typeof(SamusParticles).GetField("_particles", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(self) as List<SamusParticle>
                ?? throw new InvalidOperationException("SamusParticles no longer keeps its particles in _particles.");
    }

    /// <summary>Reads a particle as the original's step and countdown.</summary>
    extension(SamusParticle self)
    {
        /// <summary>The step of its animation it shows, counted from the first; the first while it waits hidden.</summary>
        internal int AnimationStep => self.Stage is SamusParticleStage.Showing showing
            ? Math.Max(showing.Animation.Playback.Place.Beat - 1, 0)
            : 0;

        /// <summary>The updates its picture still shows, counting the next; zero while it is hidden or finished.</summary>
        internal int StepUpdatesLeft => self.Stage is SamusParticleStage.Showing { Animation: { HasStopped: false } animation }
            ? animation.UpdatesLeft
            : 0;

        /// <summary>The hidden wait it has left before it shows; over once it shows.</summary>
        internal UpdateCountdown Hidden => self.Stage is SamusParticleStage.Waiting waiting ? waiting.Wait : UpdateCountdown.Ended();
    }

    /// <summary>Starts particles on the original's steps.</summary>
    extension(SamusParticle)
    {
        /// <summary>A particle of <paramref name="kind"/> starting on step <paramref name="step"/> of its animation.</summary>
        internal static SamusParticle AtStep(SamusParticleKind kind, int step, SamusParticleStart start, Position16<RoomPixels> position,
            Offset16<RoomPixels> drift) =>
            new(kind, step == 0
                ? AnimationPlace.StartOfAnimation
                : AnimationSegments<SamusParticleKind, SamusAtmosphereFrame, NoAnimationCue>.PlaceOf(SamusParticleAnimations.Animations[kind], kind, step),
                start, position, drift);
    }
}
