using SuperMetroid.Core.Frontend.Intro.Artwork;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>One of the six authored shell fragments spawned when the egg hatches.</summary>
internal sealed class IntroEggParticle
{
    /// <summary>Generic cinematic sprite retaining this fragment's motion timer and current instruction cursor.</summary>
    internal readonly IntroDiscoverySprite sprite;

    /// <summary>The installed steps the fragment falls by once it has fallen past its gravity curve.</summary>
    private readonly ShellFragmentFallPastCurve fallPastCurve;

    /// <summary>Spawns one of six shell fragments at its compiled position with its selected animation.</summary>
    /// <param name="index">The fragment, 0 through 5.</param>
    /// <param name="fallPastCurve">The installed steps a fragment falls by once it has fallen past its gravity curve.</param>
    public IntroEggParticle(int index, ShellFragmentFallPastCurve fallPastCurve)
    {
        this.fallPastCurve = Ensure.NotNull(fallPastCurve);
        CinematicActorDefinition definition = IntroEggParticleActors.Particle(index);
        // The spawn indexes interleaved (X-10h,Y-3Bh) pairs with parameter*4, then restores
        // the two fixed biases. The compiled catalog is independently checked against all
        // twelve original words so this physical spawn does not read the original data.
        sprite = new IntroDiscoverySprite(
            IntroEggMotion.FragmentInitialPosition(index),
            colouring: ScreenColourGroup.IntroDiscoveryActors,
            program: definition.Program)
        {
            GeneralTimer = index,
        };
        sprite.InstallFrameBehavior(definition.FrameBehavior);
    }

    /// <summary>
    /// The actor's timer word holds the fragment number below the frames the fragment has fallen, which each fall step
    /// counts up by one; the frames wrap within their eight bits without touching the fragment. The two stay in the
    /// actor's one timer word because instruction-program counter steps also count that word.
    /// </summary>
    private const int FragmentBits = 8;

    /// <summary>The fragment number the spawn stores, 0 through 5; it never changes.</summary>
    private int Fragment => WordWrap.LowByte(sprite.GeneralTimer);

    /// <summary>The frames the fragment has fallen, 0 through 255: its gravity-table index.</summary>
    private int FramesFallen => WordWrap.LowByte(sprite.GeneralTimer >> FragmentBits);

    /// <summary>Applies 16.16 fragment velocity and gravity, redirecting to deletion once it reaches the floor.</summary>
    public void Step()
    {
        if (!sprite.IsActive)
            return;
        if (sprite.FrameBehavior != IntroEggParticleActors.Particle(Fragment).FrameBehavior)
        {
            throw new InvalidDataException(
                $"Intro egg particle names invalid pre-instruction {sprite.FrameBehavior}.");
        }

        // The fragment motion indexes the horizontal velocities by fragment and the gravity table by the
        // frames fallen. Each velocity is a signed 16.16 pair stored high-word first,
        // exactly matching the actor's whole/subposition addition order.
        AddSixteenSixteenVelocity(sprite, axis: Axis.Horizontal, IntroEggMotion.FragmentX(Fragment));
        AddSixteenSixteenVelocity(sprite, axis: Axis.Vertical, IntroEggMotion.FragmentY(FramesFallen, fallPastCurve));
        if (CoordinateAxis.Difference(sprite.WholePosition.Y, IntroEggMotionDefinitions.FragmentFloorY) >= 0)
        {
            // The game primes the shared one-word delete animation, which the generic handler
            // consumes later in this same object call.
            sprite.Start(CinematicSpriteProgram.Delete);
        }
        else
        {
            sprite.GeneralTimer = WordWrap.Unsigned(sprite.GeneralTimer + 256);
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
            ? sprite.Position.WithX(IntroCinematicMotion.AddSixteenSixteen(sprite.Position.X, velocity.WholeWord, velocity.Fraction))
            : sprite.Position.WithY(IntroCinematicMotion.AddSixteenSixteen(sprite.Position.Y, velocity.WholeWord, velocity.Fraction));
    }
}
