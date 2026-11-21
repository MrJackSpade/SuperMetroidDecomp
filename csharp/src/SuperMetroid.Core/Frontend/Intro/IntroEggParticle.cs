using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Frontend.Intro.Artwork;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>One of the six authored shell fragments spawned when the egg hatches.</summary>
internal sealed class IntroEggParticle
{
    /// <summary>The cinematic sprite showing the fragment: its position and animation.</summary>
    internal readonly CinematicActor<IntroEggShellFragment, CinematicSpritePose, NoAnimationCue> sprite;

    /// <summary>Which piece of the shell this is: it selects the fragment's animation and horizontal velocity, and never changes.</summary>
    private readonly IntroEggShellFragment fragment;

    /// <summary>
    /// The updates the piece has fallen (#1781): its age is its step along the fall's gravity curve, then past it.
    /// Every piece reaches the floor within its curve or the installed steps past it.
    /// </summary>
    private readonly UpdateCountdown fall;

    /// <summary>The installed steps the fragment falls by once it has fallen past its gravity curve.</summary>
    private readonly ShellFragmentFallPastCurve fallPastCurve;

    /// <summary>Spawns one of six shell fragments at its defined position with its selected animation.</summary>
    /// <param name="fragment">Which piece of the shell to spawn.</param>
    /// <param name="fallPastCurve">The installed steps a fragment falls by once it has fallen past its gravity curve.</param>
    public IntroEggParticle(IntroEggShellFragment fragment, ShellFragmentFallPastCurve fallPastCurve)
    {
        this.fallPastCurve = Ensure.NotNull(fallPastCurve);
        fall = UpdateCountdown.Lasting(IntroEggMotionDefinitions.ShellFall.Updates + fallPastCurve.Updates);
        CinematicActorDefinition<IntroEggShellFragment> definition = IntroEggParticleActors.Particle(fragment);
        this.fragment = fragment;
        sprite = new(IntroEggMotion.FragmentInitialPosition(fragment), IntroShades.Drawn(IntroSpriteColouring.DiscoveryActors),
            IntroEggEffectAnimations.ShellPieces, definition.Animation);
    }

    /// <summary>Applies the fragment's velocity and gravity, redirecting to deletion once it reaches the floor.</summary>
    public void Step()
    {
        if (!sprite.IsActive)
            return;

        // The piece flies sideways at its own speed and vertically at its fall's speed for the updates it has flown.
        sprite.Position = WithVelocity(sprite.Position, Axis.Horizontal, IntroEggMotion.FragmentX(fragment));
        sprite.Position = WithVelocity(sprite.Position, Axis.Vertical, IntroEggMotion.FragmentFallSpeed(fall.Age, fallPastCurve));
        if (sprite.ScreenPlace.Y >= IntroEggMotionDefinitions.FragmentFloorY)
        {
            // The landed piece is gone from this update on.
            sprite.Delete();
        }
        else
        {
            fall.Count();
        }

        sprite.Step();
    }

    /// <summary>Draws the fragment's current cinematic pose with the installed egg-effect artwork.</summary>
    public void Draw(ScreenSprites screenSprites, IIntroCinematicSpritePresentation installedArt) =>
        sprite.Draw(screenSprites, installedArt: installedArt);

    /// <summary><paramref name="position"/> with a defined velocity added to one axis, the other unchanged.</summary>
    private static FixedVector<ScreenPixels> WithVelocity(FixedVector<ScreenPixels> position, Axis axis, SignedSixteenSixteen velocity) =>
        axis == Axis.Horizontal ? position.WithX(position.X + velocity) : position.WithY(position.Y + velocity);
}
