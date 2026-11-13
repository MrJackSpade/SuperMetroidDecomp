using static SuperMetroid.Core.Samus.Projectiles.ChargeFlareAnimationDefinitions;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>How the charge flare's animation lists are read: delays, starts and commands.</summary>
internal static class ChargeFlareAnimations
{
    /// <summary>
    /// The places a charge flare's position in its programs cycles through: a frame stepped past the last place
    /// continues from the first, and one rewound below the first continues from the last, so a rewind past a program's
    /// start reads the delays below the programs.
    /// </summary>
    internal const int FramePlaces = 65536;

    /// <summary>
    /// The flare frames a draw selects its picture from: the draw takes a frame's place within a cycle of 256, so a frame
    /// rewound below the first selects the cycle's last picture, which the flare does not have.
    /// </summary>
    private const int DrawnFramePlaces = 256;

    /// <summary>The flare position <paramref name="delta"/> frames on from <paramref name="frame"/>, within its cycle of places.</summary>
    /// <param name="frame">The current position, 0 through 65535.</param>
    /// <param name="delta">The signed step: one to advance, minus a rewind's length to rewind.</param>
    internal static int StepFrame(int frame, int delta) => Place(Ensure.BetweenInclusive(0, FramePlaces - 1, frame) + delta);

    /// <summary>The place within the drawn cycle of 256 that the draw selects <paramref name="frame"/>'s picture by.</summary>
    /// <param name="frame">The flare position, 0 through 65535.</param>
    internal static int DrawnFrame(int frame) => Ensure.BetweenInclusive(0, FramePlaces - 1, frame) % DrawnFramePlaces;

    /// <summary>
    /// The delay or command at <paramref name="frame"/> of charge-flare <paramref name="component"/>'s program: zero
    /// the main flare, one the slow sparks, two the fast sparks. The program's place and the frame add within the cycle
    /// of places, so a frame past a program's end reads the following programs, and one rewound below the first
    /// program's start reads the delays <paramref name="below"/> decoded there.
    /// </summary>
    internal static int ReadDelay(int component, int frame, ChargeFlareDelaysBelowPrograms below)
    {
        Ensure.NotNull(below);
        int offset = Place(Start(component) + frame);
        if (offset < RegionLength)
            return ReadCadence(offset);
        int distanceBelow = FramePlaces - offset;
        if (distanceBelow > ProgramTableLength)
            throw new InvalidDataException($"Charge-flare cadence byte {offset} is outside the compiled programs.");
        return Ensure.BetweenInclusive(0, byte.MaxValue, below.DelayBelow(distanceBelow));
    }

    /// <summary>The main flare's delay or command at <paramref name="frame"/>, read without wrapping, as the grapple flare reads it.</summary>
    internal static int ReadMainFlareDelay(int frame) => ReadCadence(MainFlare + frame);

    /// <summary>The place of <paramref name="position"/> within the cycle of flare places.</summary>
    private static int Place(int position) => (position % FramePlaces + FramePlaces) % FramePlaces;

    /// <summary>Where charge-flare <paramref name="component"/>'s program starts in the region.</summary>
    private static int Start(int component) => component switch
    {
        0 => MainFlare,
        1 => SlowSparks,
        2 => FastSparks,
        _ => throw new ArgumentOutOfRangeException(nameof(component), component, "A charge flare has three components."),
    };

    /// <summary>The main-flare, slow-spark or fast-spark cadence entry <paramref name="offset"/> places into the region.</summary>
    internal static int ReadCadence(int offset)
    {
        int delay = offset - MainFlare;
        if ((uint)delay >= RegionLength)
            throw new InvalidDataException($"Charge-flare cadence byte {offset} is outside the compiled programs.");
        if (delay < 30) return 3;
        if (delay == 30) return RewindCode;
        if (delay == 31) return 14;

        // Each spark cycle ramps down for two frames, holds its minimum for
        // four frames, then restarts. Fast sparks are one frame quicker throughout.
        int sparkOffset = offset - SlowSparks;
        int frame = sparkOffset % 7;
        int minimum = sparkOffset < 7 ? 3 : 2;
        return frame == 6 ? RestartCode : Math.Max(minimum, minimum + 2 - frame);
    }

    /// <summary>Distinguishes a flare delay-stream command entry from an ordinary delay.</summary>
    internal static bool TryDecodeCommand(int streamEntry, out ChargeFlareDelayCommand command) =>
        CommandCodes.TryGetValue(streamEntry, out command);
}
