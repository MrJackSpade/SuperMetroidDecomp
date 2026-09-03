using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

namespace SuperMetroid.Core.Frontend;

/// <summary>One of the six cartridge-authored shell fragments spawned by egg opcode $A918.</summary>
internal sealed class IntroEggParticle
{
    private const int InitialPositionTable = 0x8ba97c;
    private const int XVelocityTable = 0x8ba9ea;
    private const int YVelocityTable = 0x8baa02;
    private const ushort DeleteList = 0xce53;

    private readonly IntroDiscoverySprite sprite;

    public IntroEggParticle(ISnesAddressSpace bus, byte index)
    {
        if (index >= 6)
            throw new ArgumentOutOfRangeException(nameof(index));

        // $A958 indexes interleaved (X-10h,Y-3Bh) pairs with parameter*4, then restores
        // the two documented biases. Reading the table prevents six plausible host values
        // from becoming another unauditable transcription.
        int table = InitialPositionTable + index * 4;
        ushort x = unchecked((ushort)(RomDataReader.ReadWordFixedBank(bus, table) + 0x0010));
        ushort y = unchecked((ushort)(RomDataReader.ReadWordFixedBank(bus, table + 2) + 0x003b));
        sprite = new IntroDiscoverySprite(
            x,
            y,
            paletteBits: IntroCinematicRomData.Objects.DiscoveryPalette.Raw,
            instructionPointer: unchecked((ushort)(0xcd39 + index * 8)))
        {
            GeneralTimer = index,
        };
    }

    public bool IsActive => sprite.IsActive;

    public void Step(ISnesAddressSpace bus)
    {
        if (!sprite.IsActive)
            return;

        // $A994 uses the low timer byte as the immutable fragment number and the high byte
        // as a gravity-table index. Each velocity is a signed 16.16 pair stored high-word
        // first in ROM, exactly matching the actor's whole/subposition addition order.
        int xIndex = (sprite.GeneralTimer & 0x00ff) * 4;
        AddSixteenSixteenVelocity(bus, sprite, horizontal: true, XVelocityTable + xIndex);

        int yIndex = (sprite.GeneralTimer >> 8) * 4;
        AddSixteenSixteenVelocity(bus, sprite, horizontal: false, YVelocityTable + yIndex);
        if (unchecked((short)(sprite.YPosition - 0x00a8)) >= 0)
        {
            // Native code primes the shared one-word delete list, which the generic handler
            // consumes later in this same object call.
            sprite.Redirect(DeleteList);
        }
        else
        {
            sprite.GeneralTimer = unchecked((ushort)(sprite.GeneralTimer + 0x0100));
        }

        sprite.Step(bus);
    }

    public void Draw(ISnesAddressSpace bus, OamBuffer oam) => sprite.Draw(bus, oam);

    private static void AddSixteenSixteenVelocity(
        ISnesAddressSpace bus,
        IntroDiscoverySprite sprite,
        bool horizontal,
        int velocityAddress)
    {
        ushort whole = horizontal ? sprite.XPosition : sprite.YPosition;
        ushort fraction = horizontal ? sprite.XSubPosition : sprite.YSubPosition;
        IntroCinematicMotion.AddSixteenSixteen(
            ref whole,
            ref fraction,
            RomDataReader.ReadWordFixedBank(bus, velocityAddress),
            RomDataReader.ReadWordFixedBank(bus, velocityAddress + 2));
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
