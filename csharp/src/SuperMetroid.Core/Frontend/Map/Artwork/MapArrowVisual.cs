using SuperMetroid.Core.Frontend.Menus;

namespace SuperMetroid.Core.Frontend.Map.Artwork;

/// <summary>Provides one map arrow's screen anchor and cyclic cosmetic phase timing.</summary>
public sealed class MapArrowVisual
{
    /// <summary>Only edited durations are stored; stock phases use native timing.</summary>
    internal readonly Dictionary<int, byte>? durationOverrides;
    /// <summary>Captures the authored phase count and stores deviations from cartridge timing.</summary>
    internal MapArrowVisual(Position16<OamPixels> position, int[] durations)
    {
        Position = position;
        PhaseCount = durations.Length;
        for (int phase = 0; phase < durations.Length; phase++)
            if (durations[phase] != MenuSelectorTiming.Duration(phase))
                (durationOverrides ??= new()).Add(phase, (byte)durations[phase]);
    }
    /// <summary>Screen-pixel anchor used when composing this direction's OAM sprite.</summary>
    public Position16<OamPixels> Position { get; }
    /// <summary>Bounds the cyclic phase index supplied to <see cref="Duration"/>.</summary>
    public int PhaseCount { get; }

    /// <summary>Calculates native arrow timing, with independent authored phase edits.</summary>
    /// <remarks>$82:C137 holds the initial phase for15 ticks and every following
    /// phase for2. The original14-phase cycle repeats this rule without a cache.
    /// Custom cycle lengths and individual delays remain document-owned inputs.</remarks>
    public byte Duration(int phase)
    {
        if ((uint)phase >= (uint)PhaseCount) throw new ArgumentOutOfRangeException(nameof(phase));
        return durationOverrides is not null && durationOverrides.TryGetValue(phase, out byte value)
            ? value : MenuSelectorTiming.Duration(phase);
    }
}
