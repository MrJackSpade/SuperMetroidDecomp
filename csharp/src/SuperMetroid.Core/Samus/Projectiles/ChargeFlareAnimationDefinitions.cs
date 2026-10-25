namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>NTSC cartridge charge-flare cadence and loop commands, independent of sprite composition.</summary>
internal static class ChargeFlareAnimationDefinitions
{
    /// <summary>FlareAnimationDelays_MainFlare, the region's first program: thirty delays followed by rewind fourteen.</summary>
    private const int MainFlare = 0;
    /// <summary>FlareAnimationDelays_FlareSlowSparks: six delays followed by restart.</summary>
    private const int SlowSparks = 32;
    /// <summary>FlareAnimationDelays_FlareFastSparks: six delays followed by restart.</summary>
    private const int FastSparks = 39;
    /// <summary>The bytes the three programs fill, end to end.</summary>
    private const int RegionByteCount = 46;
    /// <summary>The three words just before the programs, naming the main-flare, slow-spark and fast-spark programs.</summary>
    private const int ProgramTableByteCount = 6;
    /// <summary>$9B:C049, HandleGrappleBeamFlare: counter one force-selects main frame sixteen.</summary>
    internal const ushort GrappleInitialFrame = 16;
    /// <summary>$9B:C04F, HandleGrappleBeamFlare: counter one seeds three before the ordinary decrement.</summary>
    internal const ushort GrappleInitialDelay = 3;
    /// <summary>
    /// The delay or command byte at <paramref name="frame"/> of charge-flare <paramref name="component"/>'s program: zero
    /// the main flare, one the slow sparks, two the fast sparks. The index wraps within sixteen bits, so a frame past a
    /// program's end reads the following programs, and one wrapping below a program's start reads the words naming them,
    /// whose values are the programs' places in <paramref name="windows"/>.
    /// </summary>
    internal static byte ReadDelay(int component, int frame, SamusAnimationWindows? windows)
    {
        int offset = unchecked((ushort)(Start(component) + frame));
        if (offset < RegionByteCount)
            return ReadByte(offset);
        int tableByte = unchecked((ushort)(offset + ProgramTableByteCount));
        if (tableByte >= ProgramTableByteCount)
            throw new InvalidDataException($"Charge-flare cadence byte {offset} is outside the compiled programs.");
        int program = (windows ?? throw new InvalidOperationException(
            "A charge-flare read below its programs needs the installed Samus overread catalog's animation windows.")).ChargeFlarePrograms
            + Start(tableByte / sizeof(ushort));
        return tableByte % sizeof(ushort) == 0
            ? SnesLittleEndian.LowByte(unchecked((ushort)program))
            : SnesLittleEndian.HighByte(unchecked((ushort)program));
    }

    /// <summary>The main flare's delay or command byte at <paramref name="frame"/>, read without wrapping, as the grapple flare reads it.</summary>
    internal static byte ReadMainFlareDelay(int frame) => ReadByte(MainFlare + frame);

    /// <summary>Where charge-flare <paramref name="component"/>'s program starts in the region.</summary>
    private static int Start(int component) => component switch
    {
        0 => MainFlare,
        1 => SlowSparks,
        2 => FastSparks,
        _ => throw new ArgumentOutOfRangeException(nameof(component), component, "A charge flare has three components."),
    };

    /// <summary>The main-flare, slow-spark or fast-spark cadence byte <paramref name="offset"/> bytes into the region.</summary>
    internal static byte ReadByte(int offset)
    {
        int delay = offset - MainFlare;
        if ((uint)delay >= RegionByteCount)
            throw new InvalidDataException($"Charge-flare cadence byte {offset} is outside the compiled programs.");
        if (delay < 30) return 3;
        if (delay == 30) return RewindByte;
        if (delay == 31) return 14;

        // Each spark cycle ramps down for two frames, holds its minimum for
        // four frames, then restarts. Fast sparks are one frame quicker throughout.
        int sparkOffset = offset - SlowSparks;
        int frame = sparkOffset % 7;
        int minimum = sparkOffset < 7 ? 3 : 2;
        return frame == 6 ? RestartByte : (byte)Math.Max(minimum, minimum + 2 - frame);
    }
    /// <summary>Distinguishes a flare delay-stream command byte from an ordinary delay.</summary>
    internal static bool TryDecodeCommand(byte streamByte, out ChargeFlareDelayCommand command)
    {
        switch (streamByte)
        {
            case RewindByte:
                command = ChargeFlareDelayCommand.Rewind;
                return true;
            case RestartByte:
                command = ChargeFlareDelayCommand.Restart;
                return true;
            default:
                command = default;
                return false;
        }
    }

    /// <summary>The stream byte that encodes <see cref="ChargeFlareDelayCommand.Rewind"/>.</summary>
    private const byte RewindByte = 0xfe;

    /// <summary>The stream byte that encodes <see cref="ChargeFlareDelayCommand.Restart"/>.</summary>
    private const byte RestartByte = 0xff;

}
