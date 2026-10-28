using SuperMetroid.Core.Frontend.Intro.Artwork;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>One of the six cartridge-authored shell fragments spawned by egg opcode $A918.</summary>
internal sealed class IntroEggParticle
{
    /// <summary>Generic cinematic sprite retaining this fragment's motion timer and current instruction cursor.</summary>
    internal readonly IntroDiscoverySprite sprite;

    /// <summary>Spawns one of six shell fragments at its compiled native position with its selected instruction list.</summary>
    public IntroEggParticle(int index)
    {
        CinematicActorDefinition definition = IntroEggEffectDefinitions.Particle(index);
        // $A958 indexes interleaved (X-10h,Y-3Bh) pairs with parameter*4, then restores
        // the two fixed biases. The compiled catalog is independently checked against all
        // twelve native words so this physical spawn does not require a cartridge read.
        sprite = new IntroDiscoverySprite(
            IntroEggMotionDefinitions.FragmentInitialPosition(index),
            palette: IntroCinematicDefinitions.Objects.DiscoveryPalette,
            program: definition.Program)
        {
            GeneralTimer = index,
        };
        sprite.InstallPreInstruction(definition.PreInstruction);
    }

    /// <summary>
    /// The actor's timer word holds the fragment number below the frames the fragment has fallen, which each fall step
    /// counts up by one; the frames wrap within their eight bits without touching the fragment. The two stay in the
    /// actor's one timer word because instruction-program counter steps also count that word.
    /// </summary>
    private const int FragmentBits = 8;

    /// <summary>The fragment number the spawn stores, 0 through 5; it never changes.</summary>
    private int Fragment => unchecked((byte)sprite.GeneralTimer);

    /// <summary>The frames the fragment has fallen, 0 through 255: its gravity-table index.</summary>
    private int FramesFallen => unchecked((byte)(sprite.GeneralTimer >> FragmentBits));

    /// <summary>Applies native 16.16 fragment velocity and gravity, redirecting to deletion once it reaches the floor.</summary>
    public void Step()
    {
        if (!sprite.IsActive)
            return;
        if (sprite.PreInstruction != IntroEggEffectDefinitions.Particle(Fragment).PreInstruction)
        {
            throw new InvalidDataException(
                $"Intro egg particle names invalid pre-instruction {sprite.PreInstruction}.");
        }

        // $A994 indexes the horizontal velocities by fragment and the gravity table by the
        // frames fallen. Each velocity is a signed 16.16 pair stored high-word first in ROM,
        // exactly matching the actor's whole/subposition addition order.
        AddSixteenSixteenVelocity(sprite, axis: Axis.Horizontal, IntroEggMotionDefinitions.FragmentX(Fragment));
        AddSixteenSixteenVelocity(sprite, axis: Axis.Vertical, IntroEggMotion.FragmentY(FramesFallen));
        if (unchecked((short)(sprite.WholePosition.Y - 168)) >= 0)
        {
            // Native code primes the shared one-word delete list, which the generic handler
            // consumes later in this same object call.
            sprite.Start(CinematicSpriteProgram.Delete);
        }
        else
        {
            sprite.GeneralTimer = unchecked((ushort)(sprite.GeneralTimer + 256));
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
