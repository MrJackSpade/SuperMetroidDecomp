using static SuperMetroid.Core.Samus.Projectiles.ChargeFlareAnimationDefinitions;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>How the charge flare's animation lists are read: delays, starts and commands.</summary>
internal static class ChargeFlareAnimations
{
    /// <summary>
    /// The delay or command byte at <paramref name="frame"/> of charge-flare <paramref name="component"/>'s program: zero
    /// the main flare, one the slow sparks, two the fast sparks. The index wraps within sixteen bits, so a frame past a
    /// program's end reads the following programs, and one wrapping below the first program's start reads the delays
    /// <paramref name="below"/> decoded there.
    /// </summary>
    internal static int ReadDelay(int component, int frame, ChargeFlareDelaysBelowPrograms below)
    {
        Ensure.NotNull(below);
        int offset = WordWrap.Unsigned(Start(component) + frame);
        if (offset < RegionByteCount)
            return ReadByte(offset);
        int distanceBelow = ushort.MaxValue + 1 - offset;
        if (distanceBelow > ProgramTableByteCount)
            throw new InvalidDataException($"Charge-flare cadence byte {offset} is outside the compiled programs.");
        return Ensure.BetweenInclusive(0, byte.MaxValue, below.DelayBelow(distanceBelow));
    }

    /// <summary>The main flare's delay or command byte at <paramref name="frame"/>, read without wrapping, as the grapple flare reads it.</summary>
    internal static int ReadMainFlareDelay(int frame) => ReadByte(MainFlare + frame);

    /// <summary>Where charge-flare <paramref name="component"/>'s program starts in the region.</summary>
    private static int Start(int component) => component switch
    {
        0 => MainFlare,
        1 => SlowSparks,
        2 => FastSparks,
        _ => throw new ArgumentOutOfRangeException(nameof(component), component, "A charge flare has three components."),
    };

    /// <summary>The main-flare, slow-spark or fast-spark cadence byte <paramref name="offset"/> bytes into the region.</summary>
    internal static int ReadByte(int offset)
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
        return frame == 6 ? RestartByte : WordWrap.LowByte(Math.Max(minimum, minimum + 2 - frame));
    }

    /// <summary>Distinguishes a flare delay-stream command byte from an ordinary delay.</summary>
    internal static bool TryDecodeCommand(int streamByte, out ChargeFlareDelayCommand command)
    {
        command = default;
        return (uint)streamByte <= byte.MaxValue && CommandBytes.TryGetValue(WordWrap.LowByte(streamByte), out command);
    }
}
