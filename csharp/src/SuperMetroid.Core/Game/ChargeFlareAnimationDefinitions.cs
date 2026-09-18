namespace SuperMetroid.Core.Game;

/// <summary>NTSC cartridge charge-flare cadence and loop commands, independent of sprite composition.</summary>
internal static class ChargeFlareAnimationDefinitions
{
    /// <summary>$90:C487 FlareAnimationDelays_MainFlare: thirty delays followed by rewind fourteen.</summary>
    private const ushort MainFlare = 0xc487;
    /// <summary>$90:C4A7 FlareAnimationDelays_FlareSlowSparks: six delays followed by restart.</summary>
    private const ushort SlowSparks = 0xc4a7;
    /// <summary>$90:C4AE FlareAnimationDelays_FlareFastSparks: six delays followed by restart.</summary>
    private const ushort FastSparks = 0xc4ae;
    /// <summary>$FF in a flare delay stream resets the frame to zero.</summary>
    internal const byte Restart = 0xff;
    /// <summary>$FE subtracts the following byte from the selected frame.</summary>
    internal const byte Rewind = 0xfe;
    /// <summary>$9B:C049, HandleGrappleBeamFlare: counter one force-selects main frame sixteen.</summary>
    internal const ushort GrappleInitialFrame = 16;
    /// <summary>$9B:C04F, HandleGrappleBeamFlare: counter one seeds three before the ordinary decrement.</summary>
    internal const ushort GrappleInitialDelay = 3;
    private static ReadOnlySpan<ushort> Pointers => [MainFlare, SlowSparks, FastSparks];
    private static ReadOnlySpan<byte> Delays =>
    [
        3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3,
        3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, Rewind, 14,
        5, 4, 3, 3, 3, 3, Restart,
        4, 3, 2, 2, 2, 2, Restart,
    ];

    internal static byte ReadByte(int address)
    {
        int pointerByte = address - SamusProjectileRomData.Beams.ChargeFlareDelayListPointers;
        if ((uint)pointerByte < Pointers.Length * 2)
            return (byte)(Pointers[pointerByte / 2] >> (pointerByte % 2 * 8));
        int delay = address - (SamusProjectileRomData.Banks.Movement | MainFlare);
        return (uint)delay < Delays.Length
            ? Delays[delay]
            : throw new InvalidDataException(
                $"Charge-flare cadence byte ${address:X6} is outside the compiled definitions.");
    }

    internal static ushort ReadWord(int address) =>
        (ushort)(ReadByte(address) | ReadByte((address & 0xff0000) | ((address + 1) & 0xffff)) << 8);
}
