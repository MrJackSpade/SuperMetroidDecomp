using SuperMetroid.Core.Frontend.Intro.Artwork;

namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>One of four Baby-Metroid slime drops created by $8B:BA73.</summary>
internal sealed class IntroEggSlimeDrop
{
    /// <summary>Discovery sprite owning this drop's position, timer, and impact instruction list.</summary>
    internal readonly IntroDiscoverySprite sprite;
    /// <summary>Stops velocity integration when the drop reaches the puddle-animation height.</summary>
    private bool motionEnabled = true;

    /// <summary>Starts one of four drops at the confused Baby's position, using its index as trajectory selector.</summary>
    public IntroEggSlimeDrop(Position16<ScreenPixels> babyPosition, byte index)
    {
        if (index >= IntroEggEffectDefinitions.SlimeDropCount)
            throw new ArgumentOutOfRangeException(nameof(index));

        IntroEggEffectActorDefinition definition = IntroEggEffectDefinitions.SlimeDrop;

        // $AA9A copies the confused baby's slot coordinates rather than using a separate
        // literal position table. Its low timer byte remains the horizontal trajectory ID.
        sprite = new IntroDiscoverySprite(
            babyPosition,
            paletteBits: IntroCinematicRomData.Objects.DiscoveryPalette.Raw,
            program: definition.Program)
        {
            GeneralTimer = index,
        };
        sprite.PreInstructionPointerForDiscovery(definition.PreInstruction);
    }

    /// <summary>Applies index-specific horizontal motion and parity-specific gravity until Y $A8, then runs the puddle list in place.</summary>
    public void Step()
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
            AddVelocity(horizontal: true, IntroEggMotionDefinitions.SlimeX(SnesLittleEndian.LowByte(sprite.GeneralTimer)));

            // BIT #1 selects two different gravity curves. This is the parameter's parity,
            // not an animation-frame toggle: odd drops begin at -2 px/frame, evens at -3.
            AddVelocity(horizontal: false, IntroEggMotionDefinitions.SlimeY(
                SnesLittleEndian.HighByte(sprite.GeneralTimer), odd: (sprite.GeneralTimer & 1) != 0));

            if (unchecked((short)(sprite.WholePosition.Y - 0x00a8)) >= 0)
            {
                // $AAB3 changes both list and pre-instruction, freezing the impact point
                // while CD71 plays four ten-frame puddle frames and deletes the actor.
                sprite.Start(CinematicSpriteProgram.MetroidEggParticleHitGround);
                sprite.PreInstructionPointerForDiscovery(
                    CinematicCodePointers.CinematicSpriteObject_PreInstruction_NoOp);
                motionEnabled = false;
            }
            else
            {
                sprite.GeneralTimer = unchecked((ushort)(sprite.GeneralTimer + 0x0100));
            }
        }

        sprite.Step();
    }

    /// <summary>Draws the drop's current frame with installed egg-effect artwork when provided.</summary>
    public void Draw(OamBuffer oam,
        IntroEggEffectSpritePresentation? installedArt = null) =>
        sprite.Draw(oam, installedArt: installedArt);

    /// <summary>Adds native 16.16 motion to one screen coordinate while preserving its fractional part.</summary>
    private void AddVelocity(bool horizontal, (ushort Whole, ushort Fraction) velocity)
    {
        (ushort wholeVelocity, ushort fractionVelocity) = velocity;
        sprite.Position = horizontal
            ? sprite.Position.WithX(IntroCinematicMotion.AddSixteenSixteen(sprite.Position.X, wholeVelocity, fractionVelocity))
            : sprite.Position.WithY(IntroCinematicMotion.AddSixteenSixteen(sprite.Position.Y, wholeVelocity, fractionVelocity));
    }
}
