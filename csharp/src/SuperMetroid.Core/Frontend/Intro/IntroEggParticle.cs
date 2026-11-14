using SuperMetroid.Core.Frontend.Intro.Artwork;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>One of the six authored shell fragments spawned when the egg hatches.</summary>
internal sealed class IntroEggParticle
{
    /// <summary>The cinematic sprite showing the fragment: its position and animation.</summary>
    internal readonly IntroDiscoverySprite sprite;

    /// <summary>Which piece of the shell this is: it selects the fragment's animation and horizontal velocity, and never changes.</summary>
    private readonly IntroEggShellFragment fragment;

    /// <summary>
    /// The frames the fragment has fallen: its step along the fall's gravity curve, then past it. A plain count (#1781):
    /// every fragment reaches the floor within its curve or the installed steps past it, and a later step throws, so the
    /// count never approaches the 256 at which the game's byte would have wrapped.
    /// </summary>
    private int framesFallen;

    /// <summary>The installed steps the fragment falls by once it has fallen past its gravity curve.</summary>
    private readonly ShellFragmentFallPastCurve fallPastCurve;

    /// <summary>Spawns one of six shell fragments at its compiled position with its selected animation.</summary>
    /// <param name="fragment">Which piece of the shell to spawn.</param>
    /// <param name="fallPastCurve">The installed steps a fragment falls by once it has fallen past its gravity curve.</param>
    public IntroEggParticle(IntroEggShellFragment fragment, ShellFragmentFallPastCurve fallPastCurve)
    {
        this.fallPastCurve = Ensure.NotNull(fallPastCurve);
        CinematicActorDefinition definition = IntroEggParticleActors.Particle(fragment);
        this.fragment = fragment;
        // The compiled start of each fragment is independently checked against all
        // twelve authored values so this physical spawn does not read game data.
        sprite = new IntroDiscoverySprite(
            IntroEggMotion.FragmentInitialPosition(fragment),
            colouring: IntroShades.Drawn(IntroSpriteColouring.DiscoveryActors),
            segment: definition.Animation);
        sprite.InstallFrameBehavior(definition.FrameBehavior);
    }

    /// <summary>Applies 1/65536-pixel fragment velocity and gravity, redirecting to deletion once it reaches the floor.</summary>
    public void Step()
    {
        if (!sprite.IsActive)
            return;
        if (sprite.FrameBehavior != IntroEggParticleActors.Particle(fragment).FrameBehavior)
        {
            throw new InvalidDataException(
                $"Intro egg particle names invalid frame behaviour {sprite.FrameBehavior}.");
        }

        // The fragment motion indexes the horizontal velocities by fragment and the gravity table by the
        // frames fallen. Each velocity is a signed 1/65536-pixel pair stored whole part first,
        // exactly matching the actor's whole/subposition addition order.
        AddSixteenSixteenVelocity(sprite, axis: Axis.Horizontal, IntroEggMotion.FragmentX(fragment));
        AddSixteenSixteenVelocity(sprite, axis: Axis.Vertical, IntroEggMotion.FragmentY(framesFallen, fallPastCurve));
        if (CoordinateAxis.Difference(sprite.WholePosition.Y, IntroEggMotionDefinitions.FragmentFloorY) >= 0)
        {
            // The game primes the shared one-step delete animation, which the generic handler
            // consumes later in this same object call.
            sprite.Start(CinematicSpriteSegment.Delete);
        }
        else
        {
            framesFallen++;
        }

        sprite.Step();
    }

    /// <summary>Draws the fragment's current cinematic pose using installed art when supplied.</summary>
    public void Draw(ScreenSprites objects,
        IIntroCinematicSpritePresentation? installedArt = null) =>
        sprite.Draw(objects, installedArt: installedArt);

    /// <summary>Adds a compiled signed whole/fraction velocity to one axis without changing the other coordinate.</summary>
    private static void AddSixteenSixteenVelocity(
        IntroDiscoverySprite sprite,
        Axis axis,
        SignedSixteenSixteen velocity)
    {
        sprite.Position = axis == Axis.Horizontal
            ? sprite.Position.WithX(sprite.Position.X.MovedBy(velocity))
            : sprite.Position.WithY(sprite.Position.Y.MovedBy(velocity));
    }
}
