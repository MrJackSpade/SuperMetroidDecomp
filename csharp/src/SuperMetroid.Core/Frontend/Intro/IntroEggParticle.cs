using SuperMetroid.Core.Frontend.Intro.Artwork;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>One of the six authored shell fragments spawned when the egg hatches.</summary>
internal sealed class IntroEggParticle
{
    /// <summary>The cinematic sprite showing the fragment: its position and animation.</summary>
    internal readonly IntroDiscoverySprite sprite;

    /// <summary>The fragment, 0 through 5: it selects the fragment's animation and horizontal velocity, and never changes.</summary>
    private readonly int fragment;

    /// <summary>The frames the fragment has fallen, 0 through 255: its gravity-table index.</summary>
    private int framesFallen;

    /// <summary>The installed steps the fragment falls by once it has fallen past its gravity curve.</summary>
    private readonly ShellFragmentFallPastCurve fallPastCurve;

    /// <summary>Spawns one of six shell fragments at its compiled position with its selected animation.</summary>
    /// <param name="index">The fragment, 0 through 5.</param>
    /// <param name="fallPastCurve">The installed steps a fragment falls by once it has fallen past its gravity curve.</param>
    public IntroEggParticle(int index, ShellFragmentFallPastCurve fallPastCurve)
    {
        this.fallPastCurve = Ensure.NotNull(fallPastCurve);
        CinematicActorDefinition definition = IntroEggParticleActors.Particle(index);
        fragment = index;
        // The spawn indexes interleaved (X-10h,Y-3Bh) pairs with parameter*4, then restores
        // the two fixed biases. The compiled catalog is independently checked against all
        // twelve authored values so this physical spawn does not read game data.
        sprite = new IntroDiscoverySprite(
            IntroEggMotion.FragmentInitialPosition(index),
            colouring: IntroShades.Drawn(IntroSpriteColouring.DiscoveryActors),
            segment: definition.Animation);
        sprite.InstallFrameBehavior(definition.FrameBehavior);
    }

    /// <summary>The frames fallen count runs past 255 back to zero.</summary>
    private const int FramesFallenWrap = 256;

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
            framesFallen = (framesFallen + 1) % FramesFallenWrap;
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
