using System.Buffers.Binary;
using static SuperMetroid.Core.Samus.Projectiles.ChargeFlareAnimationDefinitions;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>How the charge flare's animation lists are read: delays, starts and commands.</summary>
internal static class ChargeFlareAnimations
{
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
        Span<byte> programWord = stackalloc byte[sizeof(ushort)];
        BinaryPrimitives.WriteUInt16LittleEndian(programWord, unchecked((ushort)program));
        return programWord[tableByte % sizeof(ushort)];
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
    internal static bool TryDecodeCommand(int streamByte, out ChargeFlareDelayCommand command)
    {
        command = default;
        return (uint)streamByte <= byte.MaxValue && CommandBytes.TryGetValue((byte)streamByte, out command);
    }
}
