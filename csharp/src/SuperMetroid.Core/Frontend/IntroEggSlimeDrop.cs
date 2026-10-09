using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Frontend;

/// <summary>One of four Baby-Metroid slime drops created by $8B:BA73.</summary>
internal sealed class IntroEggSlimeDrop
{
    /// <summary>Discovery sprite actor whose timer carries this drop's trajectory and gravity state.</summary>
    private readonly IntroDiscoverySprite sprite;

    /// <summary>Whether the actor should continue falling before its ground-impact animation.</summary>
    private bool motionEnabled = true;

    /// <summary>Creates a slime drop at the confused baby's position with its indexed trajectory.</summary>
    /// <param name="babyX">Horizontal starting coordinate copied from the baby actor.</param>
    /// <param name="babyY">Vertical starting coordinate copied from the baby actor.</param>
    /// <param name="index">Zero-based drop index selecting one of the available horizontal trajectories.</param>
    public IntroEggSlimeDrop(ushort babyX, ushort babyY, byte index)
    {
        if (index >= IntroEggEffectDefinitions.SlimeDropCount)
            throw new ArgumentOutOfRangeException(nameof(index));

        IntroEggEffectActorDefinition definition = IntroEggEffectDefinitions.SlimeDrop;

        // $AA9A copies the confused baby's slot coordinates rather than using a separate
        // literal position table. Its low timer byte remains the horizontal trajectory ID.
        sprite = new IntroDiscoverySprite(
            babyX,
            babyY,
            paletteBits: IntroCinematicRomData.Objects.DiscoveryPalette.Raw,
            instructionPointer: definition.InstructionList)
        {
            GeneralTimer = index,
        };
        sprite.PreInstructionPointerForDiscovery(definition.PreInstruction);
    }

    /// <summary>Advances the drop's ballistic motion, switches it to the puddle sequence on impact, and steps its sprite instructions.</summary>
    /// <param name="bus">Address space used by the active sprite instruction list.</param>
    public void Step(ISnesAddressSpace bus)
    {
        if (!sprite.IsActive)
            return;

        if (motionEnabled)
        {
            if (sprite.PreInstructionPointer != IntroEggEffectDefinitions.SlimeDrop.PreInstruction)
            {
                throw new InvalidDataException(
                    $"Intro egg slime names invalid pre-instruction $8B:{sprite.PreInstructionPointer:X4}.");
            }
            AddVelocity(horizontal: true, IntroEggMotionDefinitions.SlimeX(sprite.GeneralTimer & 0xff));

            // BIT #1 selects two different gravity curves. This is the parameter's parity,
            // not an animation-frame toggle: odd drops begin at -2 px/frame, evens at -3.
            AddVelocity(horizontal: false, IntroEggMotionDefinitions.SlimeY(
                sprite.GeneralTimer >> 8, odd: (sprite.GeneralTimer & 1) != 0));

            if (unchecked((short)(sprite.YPosition - 0x00a8)) >= 0)
            {
                // $AAB3 changes both list and pre-instruction, freezing the impact point
                // while CD71 plays four ten-frame puddle frames and deletes the actor.
                sprite.Redirect(CinematicCodePointers.Lists.MetroidEggParticleHitGround);
                sprite.PreInstructionPointerForDiscovery(
                    CinematicCodePointers.CinematicSpriteObject_PreInstruction_NoOp);
                motionEnabled = false;
            }
            else
            {
                sprite.GeneralTimer = unchecked((ushort)(sprite.GeneralTimer + 0x0100));
            }
        }

        sprite.Step(bus, instructionWord: IntroEggEffectInstructionDefinitions.ReadWord);
    }

    /// <summary>Draws the drop's current sprite state into the scene OAM buffer.</summary>
    /// <param name="bus">Address space used to resolve sprite instruction data.</param>
    /// <param name="oam">Object attribute buffer receiving the drop's visible tiles.</param>
    /// <param name="installedArt">Optional installed sprite presentation used in place of cartridge artwork.</param>
    public void Draw(ISnesAddressSpace bus, OamBuffer oam,
        IntroEggEffectSpritePresentation? installedArt = null) =>
        sprite.Draw(bus, oam, installedArt: installedArt);

    /// <summary>Adds one fixed-point velocity component to the actor's selected position.</summary>
    /// <param name="horizontal"><see langword="true"/> to update X; otherwise updates Y.</param>
    /// <param name="velocity">Whole-pixel and fractional parts of the velocity to accumulate.</param>
    private void AddVelocity(bool horizontal, (ushort Whole, ushort Fraction) velocity)
    {
        (ushort wholeVelocity, ushort fractionVelocity) = velocity;
        if (horizontal)
        {
            ushort whole = sprite.XPosition;
            ushort fraction = sprite.XSubPosition;
            IntroCinematicMotion.AddSixteenSixteen(
                ref whole, ref fraction, wholeVelocity, fractionVelocity);
            sprite.XPosition = whole;
            sprite.XSubPosition = fraction;
        }
        else
        {
            ushort whole = sprite.YPosition;
            ushort fraction = sprite.YSubPosition;
            IntroCinematicMotion.AddSixteenSixteen(
                ref whole, ref fraction, wholeVelocity, fractionVelocity);
            sprite.YPosition = whole;
            sprite.YSubPosition = fraction;
        }
    }
}
