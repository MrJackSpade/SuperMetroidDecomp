using SuperMetroid.Core.Frontend.Intro.Artwork;

namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>One of the six cartridge-authored shell fragments spawned by egg opcode $A918.</summary>
internal sealed class IntroEggParticle
{
    /// <summary>Generic cinematic sprite retaining this fragment's motion timer and current instruction cursor.</summary>
    internal readonly IntroDiscoverySprite sprite;

    /// <summary>Spawns one of six shell fragments at its compiled native position with its selected instruction list.</summary>
    public IntroEggParticle(byte index)
    {
        IntroEggEffectActorDefinition definition = IntroEggEffectDefinitions.Particle(index);
        // $A958 indexes interleaved (X-10h,Y-3Bh) pairs with parameter*4, then restores
        // the two fixed biases. The compiled catalog is independently checked against all
        // twelve native words so this physical spawn does not require a cartridge read.
        sprite = new IntroDiscoverySprite(
            IntroEggMotionDefinitions.FragmentInitialPosition(index),
            paletteBits: IntroCinematicRomData.Objects.DiscoveryPalette.Raw,
            program: definition.Program)
        {
            GeneralTimer = index,
        };
        sprite.PreInstructionPointerForDiscovery(definition.PreInstruction);
    }

    /// <summary>Applies native 16.16 fragment velocity and gravity, redirecting to deletion once it reaches the floor.</summary>
    public void Step()
    {
        if (!sprite.IsActive)
            return;
        if (sprite.PreInstructionPointer != IntroEggEffectDefinitions.Particle(
                SnesLittleEndian.LowByte(sprite.GeneralTimer)).PreInstruction)
        {
            throw new InvalidDataException(
                $"Intro egg particle names invalid pre-instruction $8B:{sprite.PreInstructionPointer:X4}.");
        }

        // $A994 uses the low timer byte as the immutable fragment number and the high byte
        // as a gravity-table index. Each velocity is a signed 16.16 pair stored high-word
        // first in ROM, exactly matching the actor's whole/subposition addition order.
        int xIndex = SnesLittleEndian.LowByte(sprite.GeneralTimer);
        AddSixteenSixteenVelocity(sprite, horizontal: true, IntroEggMotionDefinitions.FragmentX(xIndex));

        int yIndex = SnesLittleEndian.HighByte(sprite.GeneralTimer);
        AddSixteenSixteenVelocity(sprite, horizontal: false, IntroEggMotionDefinitions.FragmentY(yIndex));
        if (unchecked((short)(sprite.WholePosition.Y - 0x00a8)) >= 0)
        {
            // Native code primes the shared one-word delete list, which the generic handler
            // consumes later in this same object call.
            sprite.Start(CinematicSpriteProgram.Delete);
        }
        else
        {
            sprite.GeneralTimer = unchecked((ushort)(sprite.GeneralTimer + 0x0100));
        }

        sprite.Step();
    }

    /// <summary>Draws the fragment's current cinematic pose using installed art when supplied.</summary>
    public void Draw(OamBuffer oam,
        IntroEggEffectSpritePresentation? installedArt = null) =>
        sprite.Draw(oam, installedArt: installedArt);

    /// <summary>Adds a compiled signed whole/fraction velocity to one axis without changing the other coordinate.</summary>
    private static void AddSixteenSixteenVelocity(
        IntroDiscoverySprite sprite,
        bool horizontal,
        (ushort Whole, ushort Fraction) velocity)
    {
        sprite.Position = horizontal
            ? sprite.Position.WithX(IntroCinematicMotion.AddSixteenSixteen(sprite.Position.X, velocity.Whole, velocity.Fraction))
            : sprite.Position.WithY(IntroCinematicMotion.AddSixteenSixteen(sprite.Position.Y, velocity.Whole, velocity.Fraction));
    }
}
