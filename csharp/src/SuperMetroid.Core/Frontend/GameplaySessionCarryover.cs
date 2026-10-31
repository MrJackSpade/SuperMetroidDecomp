
namespace SuperMetroid.Core.Frontend;

/// <summary>What the session keeps across the gameplay runtimes it creates, and hands each one at its start.</summary>
internal sealed record GameplaySessionCarryover
{
    /// <summary>The session's frame clock, which counts across runtimes.</summary>
    public required FrameClock Clock { get; init; }

    /// <summary>The session's random-number word, which the runtime's system state reads and advances.</summary>
    public required SharedRandom Random { get; init; }

    /// <summary>The session's unauthored Speed Booster cadence, whose delays outlive each runtime.</summary>
    public required UnauthoredBoostCadence UnauthoredBoostCadence { get; init; }

    /// <summary>The frame the save file found at power-on adds to every earthquake rumble's delay, fixed for the session.</summary>
    public required PowerOnEarthquakeSoundDelay EarthquakeSoundDelay { get; init; }

    /// <summary>When an open save confirmation reads the controller; the runtime's message box consults it.</summary>
    public required SaveChoiceInputCadence SaveChoiceInput { get; init; }
}
