namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>NTSC cartridge charge-flare cadence and loop commands, independent of sprite composition.</summary>
internal static class ChargeFlareAnimationDefinitions
{
    /// <summary>$90:C481: the three words naming the main-flare, slow-spark and fast-spark programs; a wrapped frame index reads them as delays.</summary>
    private const int ProgramTable = 0x90c481;
    /// <summary>$90:C487 FlareAnimationDelays_MainFlare: thirty delays followed by rewind fourteen.</summary>
    private const ushort MainFlare = 0xc487;
    /// <summary>$90:C4A7 FlareAnimationDelays_FlareSlowSparks: six delays followed by restart.</summary>
    private const ushort SlowSparks = 0xc4a7;
    /// <summary>$90:C4AE FlareAnimationDelays_FlareFastSparks: six delays followed by restart.</summary>
    private const ushort FastSparks = 0xc4ae;
    /// <summary>$9B:C049, HandleGrappleBeamFlare: counter one force-selects main frame sixteen.</summary>
    internal const ushort GrappleInitialFrame = 16;
    /// <summary>$9B:C04F, HandleGrappleBeamFlare: counter one seeds three before the ordinary decrement.</summary>
    internal const ushort GrappleInitialDelay = 3;
    /// <summary>
    /// The delay or command byte at <paramref name="frame"/> of charge-flare <paramref name="component"/>'s program: zero
    /// the main flare, one the slow sparks, two the fast sparks. The index wraps within a 16-bit window, so a frame past a
    /// program's end reads the following programs.
    /// </summary>
    internal static byte ReadDelay(int component, int frame) =>
        ReadByte(SamusProjectileRomData.Banks.Movement | unchecked((ushort)(Start(component) + frame)));

    /// <summary>The main flare's delay or command byte at <paramref name="frame"/>, read without wrapping, as the grapple flare reads it.</summary>
    internal static byte ReadMainFlareDelay(int frame) => ReadByte((SamusProjectileRomData.Banks.Movement | MainFlare) + frame);

    /// <summary>Where charge-flare <paramref name="component"/>'s program starts in its 16-bit window.</summary>
    private static ushort Start(int component) => component switch
    {
        0 => MainFlare,
        1 => SlowSparks,
        2 => FastSparks,
        _ => throw new ArgumentOutOfRangeException(nameof(component), component, "A charge flare has three components."),
    };

    /// <summary>Compiles the three program words and their main, slow-spark, and fast-spark cadence bytes.</summary>
    internal static byte ReadByte(int address)
    {
        int tableByte = address - ProgramTable;
        if ((uint)tableByte < 6)
            return (byte)(Start(tableByte / 2) >> (tableByte % 2 * 8));
        int delay = address - (SamusProjectileRomData.Banks.Movement | MainFlare);
        if ((uint)delay >= 46)
            throw new InvalidDataException($"Charge-flare cadence byte ${address:X6} is outside the compiled definitions.");
        if (delay < 30) return 3;
        if (delay == 30) return (byte)ChargeFlareDelayCommand.Rewind;
        if (delay == 31) return 14;

        // Each spark cycle ramps down for two frames, holds its minimum for
        // four frames, then restarts. Fast sparks are one frame quicker throughout.
        int sparkOffset = address - (SamusProjectileRomData.Banks.Movement | SlowSparks);
        int frame = sparkOffset % 7;
        int minimum = sparkOffset < 7 ? 3 : 2;
        return frame == 6 ? (byte)ChargeFlareDelayCommand.Restart : (byte)Math.Max(minimum, minimum + 2 - frame);
    }
    /// <summary>Distinguishes a flare delay-stream command byte from an ordinary delay.</summary>
    internal static bool TryDecodeCommand(byte streamByte, out ChargeFlareDelayCommand command)
    {
        if (!Enum.IsDefined((ChargeFlareDelayCommand)streamByte))
        {
            command = default;
            return false;
        }
        command = (ChargeFlareDelayCommand)streamByte;
        return true;
    }

}
