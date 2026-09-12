using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Frontend;

/// <summary>One of four Baby-Metroid slime drops created by $8B:BA73.</summary>
internal sealed class IntroEggSlimeDrop
{
    private readonly IntroDiscoverySprite sprite;
    private bool motionEnabled = true;

    public IntroEggSlimeDrop(ushort babyX, ushort babyY, byte index)
    {
        if (index >= 4)
            throw new ArgumentOutOfRangeException(nameof(index));

        // $AA9A copies the confused baby's slot coordinates rather than using a separate
        // literal position table. Its low timer byte remains the horizontal trajectory ID.
        sprite = new IntroDiscoverySprite(
            babyX,
            babyY,
            paletteBits: IntroCinematicRomData.Objects.DiscoveryPalette.Raw,
            instructionPointer: CinematicCodePointers.Lists.MetroidEggSlimeDrops)
        {
            GeneralTimer = index,
        };
    }

    public bool IsActive => sprite.IsActive;

    public void Step(ISnesAddressSpace bus)
    {
        if (!sprite.IsActive)
            return;

        if (motionEnabled)
        {
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
                motionEnabled = false;
            }
            else
            {
                sprite.GeneralTimer = unchecked((ushort)(sprite.GeneralTimer + 0x0100));
            }
        }

        sprite.Step(bus);
    }

    public void Draw(ISnesAddressSpace bus, OamBuffer oam) => sprite.Draw(bus, oam);

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
