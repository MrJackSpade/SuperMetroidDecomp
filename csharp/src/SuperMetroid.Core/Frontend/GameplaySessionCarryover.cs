using SuperMetroid.Core.Gameplay;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Frontend;

/// <summary>What the session keeps across the gameplay runtimes it creates, and hands each one at its start.</summary>
internal sealed record GameplaySessionCarryover
{
    /// <summary>The session's frame clock, which counts across runtimes.</summary>
    public required FrameClock Clock { get; init; }

    /// <summary>The session's random-number word, which the runtime's system state reads and advances.</summary>
    public required SharedRandom Random { get; init; }

    /// <summary>The session's floor update residue, which outlives each runtime as WRAM does.</summary>
    public required SuperMetroid.Core.Compatibility.FloorUpdateResidue FloorQueueResidue { get; init; }

    /// <summary>The frame the save file found at power-on adds to every earthquake rumble's delay, fixed for the session.</summary>
    public required PowerOnEarthquakeSoundDelay EarthquakeSoundDelay { get; init; }
}
