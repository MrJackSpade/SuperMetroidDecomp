using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Frontend;

/// <summary>One of the six cartridge-authored shell fragments spawned by egg opcode $A918.</summary>
internal sealed class IntroEggParticle
{
    private readonly IntroDiscoverySprite sprite;

    public IntroEggParticle(byte index)
    {
        IntroEggEffectActorDefinition definition = IntroEggEffectDefinitions.Particle(index);
        // $A958 indexes interleaved (X-10h,Y-3Bh) pairs with parameter*4, then restores
        // the two fixed biases. The compiled catalog is independently checked against all
        // twelve native words so this physical spawn does not require a cartridge read.
        var (x, y) = IntroEggMotionDefinitions.FragmentInitialPosition(index);
        sprite = new IntroDiscoverySprite(
            x,
            y,
            paletteBits: IntroCinematicRomData.Objects.DiscoveryPalette.Raw,
            instructionPointer: definition.InstructionList)
        {
            GeneralTimer = index,
        };
        sprite.PreInstructionPointerForDiscovery(definition.PreInstruction);
    }

    public bool IsActive => sprite.IsActive;

    public void Step(ISnesAddressSpace bus)
    {
        if (!sprite.IsActive)
            return;
        if (sprite.PreInstructionPointer != IntroEggEffectDefinitions.Particle(
                sprite.GeneralTimer & 0x00ff).PreInstruction)
        {
            throw new InvalidDataException(
                $"Intro egg particle names invalid pre-instruction $8B:{sprite.PreInstructionPointer:X4}.");
        }

        // $A994 uses the low timer byte as the immutable fragment number and the high byte
        // as a gravity-table index. Each velocity is a signed 16.16 pair stored high-word
        // first in ROM, exactly matching the actor's whole/subposition addition order.
        int xIndex = sprite.GeneralTimer & 0x00ff;
        AddSixteenSixteenVelocity(sprite, horizontal: true, IntroEggMotionDefinitions.FragmentX(xIndex));

        int yIndex = sprite.GeneralTimer >> 8;
        AddSixteenSixteenVelocity(sprite, horizontal: false, IntroEggMotionDefinitions.FragmentY(yIndex));
        if (unchecked((short)(sprite.YPosition - 0x00a8)) >= 0)
        {
            // Native code primes the shared one-word delete list, which the generic handler
            // consumes later in this same object call.
            sprite.Redirect(CinematicCodePointers.Lists.Delete);
        }
        else
        {
            sprite.GeneralTimer = unchecked((ushort)(sprite.GeneralTimer + 0x0100));
        }

        sprite.Step(bus, instructionWord: IntroEggEffectInstructionDefinitions.ReadWord);
    }

    public void Draw(ISnesAddressSpace bus, OamBuffer oam,
        IntroEggEffectSpritePresentation? installedArt = null) =>
        sprite.Draw(bus, oam, installedArt: installedArt);

    private static void AddSixteenSixteenVelocity(
        IntroDiscoverySprite sprite,
        bool horizontal,
        (ushort Whole, ushort Fraction) velocity)
    {
        ushort whole = horizontal ? sprite.XPosition : sprite.YPosition;
        ushort fraction = horizontal ? sprite.XSubPosition : sprite.YSubPosition;
        IntroCinematicMotion.AddSixteenSixteen(
            ref whole,
            ref fraction,
            velocity.Whole,
            velocity.Fraction);
        if (horizontal)
        {
            sprite.XPosition = whole;
            sprite.XSubPosition = fraction;
        }
        else
        {
            sprite.YPosition = whole;
            sprite.YSubPosition = fraction;
        }
    }
}
