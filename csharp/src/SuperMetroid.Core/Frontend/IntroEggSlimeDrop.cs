using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.Core.Frontend;

/// <summary>One of four Baby-Metroid slime drops created by $8B:BA73.</summary>
internal sealed class IntroEggSlimeDrop
{
    internal readonly IntroDiscoverySprite sprite;
    private bool motionEnabled = true;

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
            instructionPointer: definition.InstructionList)
        {
            GeneralTimer = index,
        };
        sprite.PreInstructionPointerForDiscovery(definition.PreInstruction);
    }

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

        sprite.Step(instructionWord: IntroEggEffectInstructionDefinitions.ReadWord);
    }

    public void Draw(OamBuffer oam,
        IntroEggEffectSpritePresentation? installedArt = null) =>
        sprite.Draw(oam, installedArt: installedArt);

    private void AddVelocity(bool horizontal, (ushort Whole, ushort Fraction) velocity)
    {
        (ushort wholeVelocity, ushort fractionVelocity) = velocity;
        sprite.Position = horizontal
            ? sprite.Position.WithX(IntroCinematicMotion.AddSixteenSixteen(sprite.Position.X, wholeVelocity, fractionVelocity))
            : sprite.Position.WithY(IntroCinematicMotion.AddSixteenSixteen(sprite.Position.Y, wholeVelocity, fractionVelocity));
    }
}
