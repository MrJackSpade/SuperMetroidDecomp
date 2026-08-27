using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

namespace SuperMetroid.Core.Frontend;

/// <summary>One of four Baby-Metroid slime drops created by $8B:BA73.</summary>
internal sealed class IntroEggSlimeDrop
{
    private const int XVelocityTable = 0x8bab35;
    private const int OddYVelocityTable = 0x8bab49;
    private const int EvenYVelocityTable = 0x8bac41;
    private const ushort HitGroundList = 0xcd71;

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
            paletteBits: 0x0e00,
            instructionPointer: 0xcd69)
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
            int xVelocity = XVelocityTable + (sprite.GeneralTimer & 0x00ff) * 4;
            AddVelocity(bus, horizontal: true, xVelocity);

            // BIT #1 selects two different gravity curves. This is the parameter's parity,
            // not an animation-frame toggle: odd drops begin at -2 px/frame, evens at -3.
            int yTable = (sprite.GeneralTimer & 1) != 0
                ? OddYVelocityTable
                : EvenYVelocityTable;
            int yVelocity = yTable + (sprite.GeneralTimer >> 8) * 4;
            AddVelocity(bus, horizontal: false, yVelocity);

            if (unchecked((short)(sprite.YPosition - 0x00a8)) >= 0)
            {
                // $AAB3 changes both list and pre-instruction, freezing the impact point
                // while CD71 plays four ten-frame puddle frames and deletes the actor.
                sprite.Redirect(HitGroundList);
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

    private void AddVelocity(ISnesAddressSpace bus, bool horizontal, int address)
    {
        ushort wholeVelocity = RomDataReader.ReadWordFixedBank(bus, address);
        ushort fractionVelocity = RomDataReader.ReadWordFixedBank(bus, address + 2);
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
