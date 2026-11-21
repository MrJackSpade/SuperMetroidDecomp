using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Samus.Artwork;

namespace SuperMetroid.Core.Samus.Drawing;

/// <summary>
/// Where one of Samus's particles stands (#2554): waiting hidden before it shows, or showing its animation, which always
/// shows a picture, so a shown particle draws the picture its stage holds and a hidden one is its own case.
/// </summary>
internal abstract class SamusParticleStage
{
    /// <summary>Only the two stages below derive from it.</summary>
    private SamusParticleStage()
    {
    }

    /// <summary>The particle waits hidden, its animation not yet started.</summary>
    /// <param name="wait">The wait before it shows, counting the update it first shows on.</param>
    /// <param name="animation">The animation it shows once the wait ends, from its first picture.</param>
    internal sealed class Waiting(UpdateCountdown wait, Animation<SamusParticleKind> animation) : SamusParticleStage
    {
        /// <summary>The wait before it shows, counting the update it first shows on.</summary>
        internal UpdateCountdown Wait { get; } = wait;

        /// <summary>The animation it shows once the wait ends, from its first picture.</summary>
        internal Animation<SamusParticleKind> Animation { get; } = animation;
    }

    /// <summary>The particle shows its animation's pictures in turn.</summary>
    /// <param name="animation">The play of its animation, which always shows a picture.</param>
    internal sealed class Showing(ShowingAnimationPlayer<SamusParticleKind, SamusAtmosphereFrame, NoAnimationCue> animation) : SamusParticleStage
    {
        /// <summary>The play of its animation, which always shows a picture.</summary>
        internal ShowingAnimationPlayer<SamusParticleKind, SamusAtmosphereFrame, NoAnimationCue> Animation { get; } = animation;

        /// <summary>The picture the particle shows.</summary>
        internal SamusAtmosphereFrame Picture => Animation.Showing;
    }
}
