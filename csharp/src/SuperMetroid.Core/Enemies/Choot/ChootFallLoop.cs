using static SuperMetroid.Core.Enemies.Choot.ChootFallingPathDefinitions;

namespace SuperMetroid.Core.Enemies.Choot;

/// <summary>
/// Where a falling Choot is in one loop of its falling path (#2554): at one of the path's samples, which says where it is
/// from the falling origin, or past the loop's last update, when the next falling update ends the loop. A loop only
/// starts at its path's first sample and only counts forward through its samples, so no place outside the path exists.
/// </summary>
internal abstract partial class ChootFallLoop
{
    /// <summary>Only the two places below exist.</summary>
    private ChootFallLoop() { }

    /// <summary>A loop of <paramref name="fallingPath"/> at its first update, at the path's first sample.</summary>
    /// <exception cref="KeyNotFoundException"><paramref name="fallingPath"/> is not a defined path.</exception>
    internal static AtSample Starting(ChootFallingPath fallingPath) => new(ChootFallingPathDefinitions.Paths[fallingPath].Samples, sample: 0, updatesIntoSample: 0, age: 0);

    /// <summary>A loop at one of its path's samples.</summary>
    internal sealed class AtSample : ChootFallLoop
    {
        /// <summary>The samples of the path the loop follows, each one or more updates long.</summary>
        private readonly ChootFallSample[] _samples;

        /// <summary>Which of <see cref="_samples"/> the loop is at.</summary>
        [IntegerQuantity("sample along the falling path")]
        private readonly int _sample;

        /// <summary>How many of the sample's updates the loop has already counted.</summary>
        [IntegerQuantity("updates the loop has counted at its sample")]
        private readonly int _updatesIntoSample;

        /// <summary>Creates the loop at a sample within its path.</summary>
        internal AtSample(ChootFallSample[] samples, int sample, int updatesIntoSample, int age)
        {
            _samples = samples;
            _sample = sample;
            _updatesIntoSample = updatesIntoSample;
            Age = age;
        }

        /// <summary>The update of the loop the next falling update takes, counted from the loop's first.</summary>
        [IntegerQuantity("updates into the falling loop")]
        internal int Age { get; }

        /// <summary>The Choot's offset from the falling origin at this update of the loop.</summary>
        internal Offset16<RoomPixels> Offset => _samples[_sample].Offset;

        /// <summary>The loop one update on: the same sample's next update, the next sample's first, or past the loop's end.</summary>
        internal ChootFallLoop Counted() =>
            _updatesIntoSample + 1 < _samples[_sample].Updates ? new AtSample(_samples, _sample, _updatesIntoSample + 1, Age + 1)
            : _sample + 1 < _samples.Length ? new AtSample(_samples, _sample + 1, updatesIntoSample: 0, Age + 1)
            : Over.Instance;
    }

    /// <summary>A loop past its last update: the next falling update ends it.</summary>
    internal sealed class Over : ChootFallLoop
    {
        /// <summary>The one loop past its end.</summary>
        internal static Over Instance { get; } = new();

        /// <summary>The place has no data, so one instance serves every Choot.</summary>
        private Over() { }
    }
}
