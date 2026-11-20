namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// How long a rainbow-beam attack step asks the room to shake (#2013): a number of frames, on until a producer gives the
/// earthquake a length, or not at all, stopping it.
/// </summary>
public abstract record MotherBrainEarthquakeLength
{
    /// <summary>Only the nested cases derive.</summary>
    private MotherBrainEarthquakeLength()
    {
    }

    /// <summary>On until a producer gives the earthquake a length.</summary>
    public static MotherBrainEarthquakeLength Indefinite { get; } = new Unbounded();

    /// <summary>Not at all: the earthquake stops.</summary>
    public static MotherBrainEarthquakeLength Stopped { get; } = new Stopping();

    /// <summary>For <paramref name="Frames"/> frames.</summary>
    /// <param name="Frames">The frames to shake.</param>
    public sealed record Lasting(int Frames) : MotherBrainEarthquakeLength;

    /// <summary>On until a producer gives the earthquake a length.</summary>
    private sealed record Unbounded : MotherBrainEarthquakeLength;

    /// <summary>Not at all: the earthquake stops.</summary>
    private sealed record Stopping : MotherBrainEarthquakeLength;
}
