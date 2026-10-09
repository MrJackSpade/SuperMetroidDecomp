using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Frontend;

/// <summary>One of the six cartridge-authored shell fragments spawned by egg opcode $A918.</summary>
internal sealed class IntroEggParticle
{
    /// <summary>Owns this fragment's position, animation timer, and instruction-list state.</summary>
    private readonly IntroDiscoverySprite sprite;

    /// <summary>Creates a shell fragment using its cartridge-authored initial position and animation.</summary>
    /// <param name="index">The fragment number selecting its spawn position and instruction list.</param>
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

    /// <summary>Advances the fragment's gravity-driven motion and executes its current sprite instruction.</summary>
    /// <param name="bus">The address space used by the sprite instruction handler.</param>
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

    /// <summary>Submits the active fragment sprite to OAM, optionally using installed presentation data.</summary>
    /// <param name="bus">The address space used to resolve the sprite's graphics.</param>
    /// <param name="oam">The OAM buffer that receives the sprite entry.</param>
    /// <param name="installedArt">Optional compiled presentation data for the intro egg effect.</param>
    public void Draw(ISnesAddressSpace bus, OamBuffer oam,
        IntroEggEffectSpritePresentation? installedArt = null) =>
        sprite.Draw(bus, oam, installedArt: installedArt);

    /// <summary>Adds one signed 16.16 velocity to the selected sprite axis.</summary>
    /// <param name="sprite">The sprite whose position and subposition are updated.</param>
    /// <param name="horizontal"><see langword="true"/> selects X; <see langword="false"/> selects Y.</param>
    /// <param name="velocity">The whole-word and fractional-word velocity to accumulate.</param>
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
